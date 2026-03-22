#!/usr/bin/env python3
"""
Multi-Tenancy Security Scanner — Detects potential cross-tenant data leaks.

Scans repository implementations and query handlers for missing TenantId filters.
This is the MOST CRITICAL audit check — any miss is a data leak.

Usage:
    python3 tenant_security_scan.py <solution-root>
    python3 tenant_security_scan.py /path/to/Orbito
"""

import sys
import re
from pathlib import Path
from collections import defaultdict


class Issue:
    def __init__(self, severity, message, file, line, context=""):
        self.severity = severity
        self.message = message
        self.file = str(file)
        self.line = line
        self.context = context


def scan_repositories(infra_path):
    """Scan repository files for missing tenant filtering."""
    issues = []
    repo_files = list(infra_path.rglob("*Repository.cs"))

    for filepath in repo_files:
        content = filepath.read_text(encoding="utf-8", errors="ignore")
        lines = content.split("\n")

        # Check: does this repository inject ITenantContext?
        has_tenant_context = bool(re.search(
            r'ITenantContext|_tenantContext|tenantContext', content
        ))

        if not has_tenant_context:
            issues.append(Issue(
                "CRITICAL",
                f"Repository does NOT inject ITenantContext — all queries are unscoped",
                filepath, 1,
                "Missing ITenantContext dependency injection"
            ))
            continue

        # Find all public async methods
        for i, line in enumerate(lines, 1):
            # Match method signatures
            if re.search(r'public\s+(?:async\s+)?(?:Task|IAsyncEnumerable)', line):
                method_name = re.search(r'(\w+)\s*\(', line)
                if not method_name:
                    continue
                name = method_name.group(1)

                # Look at the method body (next 30 lines)
                body = "\n".join(lines[i:min(i+30, len(lines))])

                # Check if method filters by TenantId
                has_tenant_filter = bool(re.search(
                    r'TenantId|tenantId|_tenantContext|\.ProviderId|ForProvider|ForClient',
                    body
                ))

                # Skip methods that are clearly utility
                if name in ('SaveChangesAsync', 'Dispose', 'CommitAsync'):
                    continue

                if not has_tenant_filter:
                    issues.append(Issue(
                        "CRITICAL",
                        f"Method '{name}' may not filter by TenantId",
                        filepath, i,
                        line.strip()[:120]
                    ))

    return issues


def scan_query_handlers(app_path):
    """Scan query handlers for missing tenant context."""
    issues = []
    handler_files = list(app_path.rglob("*Handler.cs"))

    for filepath in handler_files:
        content = filepath.read_text(encoding="utf-8", errors="ignore")

        # Only scan query handlers (not command handlers — they mutate)
        if "IRequestHandler" not in content:
            continue

        # Check for tenant context
        has_tenant = bool(re.search(
            r'ITenantContext|_tenantContext|TenantId|GetProviderId|ProviderId',
            content
        ))

        if not has_tenant:
            # Might be ok for admin queries — check if it's admin-only
            is_admin = bool(re.search(r'Admin|PlatformAdmin|AdminOnly', content))
            if not is_admin:
                issues.append(Issue(
                    "MAJOR",
                    "Handler does not reference tenant context — verify it's admin-only or properly scoped",
                    filepath, 1,
                    "Missing ITenantContext usage"
                ))

    return issues


def scan_controllers(api_path):
    """Scan controllers for tenant-bypassing patterns."""
    issues = []
    controller_files = list(api_path.rglob("*Controller.cs"))

    for filepath in controller_files:
        content = filepath.read_text(encoding="utf-8", errors="ignore")
        lines = content.split("\n")

        for i, line in enumerate(lines, 1):
            # Check: TenantId taken from request body (should come from auth claims)
            if re.search(r'(?:FromBody|FromQuery).*(?:tenantId|TenantId|providerId|ProviderId)', line, re.IGNORECASE):
                issues.append(Issue(
                    "CRITICAL",
                    "TenantId/ProviderId comes from request input — must come from authenticated claims only",
                    filepath, i,
                    line.strip()[:120]
                ))

            # Check: AllowAnonymous on mutation endpoints
            if '[AllowAnonymous]' in line:
                # Look at next few lines for HTTP method
                next_lines = "\n".join(lines[i:min(i+5, len(lines))])
                if re.search(r'HttpPost|HttpPut|HttpDelete|HttpPatch', next_lines):
                    issues.append(Issue(
                        "CRITICAL",
                        "[AllowAnonymous] on mutation endpoint — potential unauthorized data modification",
                        filepath, i,
                        line.strip()[:120]
                    ))

    return issues


def scan_cache_keys(root_path):
    """Check if cache keys include tenant context."""
    issues = []
    cs_files = list(root_path.rglob("*.cs"))

    for filepath in cs_files:
        content = filepath.read_text(encoding="utf-8", errors="ignore")
        lines = content.split("\n")

        for i, line in enumerate(lines, 1):
            # Cache set/get operations
            if re.search(r'\.Set(?:Async)?\s*\(|\.Get(?:Async)?\s*\(|CacheKey|cacheKey', line):
                # Check surrounding context for tenant in cache key
                context = "\n".join(lines[max(0, i-3):min(len(lines), i+3)])
                if re.search(r'Cache', line) and not re.search(r'TenantId|tenantId|ProviderId|providerId', context):
                    issues.append(Issue(
                        "MAJOR",
                        "Cache operation may not include TenantId in key — risk of cross-tenant cache pollution",
                        filepath, i,
                        line.strip()[:120]
                    ))

    return issues


def scan_background_jobs(root_path):
    """Check background jobs set tenant context."""
    issues = []
    job_files = list(root_path.rglob("*Job.cs")) + list(root_path.rglob("*Job*.cs"))

    for filepath in job_files:
        if "Test" in str(filepath):
            continue
        content = filepath.read_text(encoding="utf-8", errors="ignore")

        has_tenant_scope = bool(re.search(
            r'ITenantContext|TenantContext|SetTenant|tenantId|ProviderId|ForEachTenant',
            content
        ))

        has_data_access = bool(re.search(
            r'Repository|DbContext|_db|_context|SaveChanges',
            content
        ))

        if has_data_access and not has_tenant_scope:
            issues.append(Issue(
                "CRITICAL",
                "Background job accesses data but does not set tenant context — all operations are unscoped",
                filepath, 1,
                "Missing tenant context in background job"
            ))

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 tenant_security_scan.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1])

    print("=" * 70)
    print("MULTI-TENANCY SECURITY SCAN")
    print("=" * 70)

    all_issues = []

    # Scan repositories
    infra_paths = [
        root / "Orbito.Infrastructure" / "Persistance",
        root / "Orbito.Infrastructure" / "Persistence",
        root / "Orbito.Infrastructure" / "Repositories",
    ]
    for p in infra_paths:
        if p.exists():
            print(f"\nScanning repositories: {p}")
            all_issues.extend(scan_repositories(p))

    # Scan query handlers
    app_path = root / "Orbito.Application"
    if app_path.exists():
        print(f"\nScanning query handlers: {app_path}")
        all_issues.extend(scan_query_handlers(app_path))

    # Scan controllers
    api_path = root / "Orbito.API" / "Controllers"
    if api_path.exists():
        print(f"\nScanning controllers: {api_path}")
        all_issues.extend(scan_controllers(api_path))

    # Scan cache keys
    print(f"\nScanning cache keys...")
    all_issues.extend(scan_cache_keys(root))

    # Scan background jobs
    print(f"\nScanning background jobs...")
    all_issues.extend(scan_background_jobs(root))

    # Report
    by_severity = defaultdict(list)
    for issue in all_issues:
        by_severity[issue.severity].append(issue)

    for severity in ["CRITICAL", "MAJOR", "MINOR"]:
        group = by_severity.get(severity, [])
        if group:
            print(f"\n{'─' * 70}")
            print(f"{severity} ({len(group)})")
            print(f"{'─' * 70}")
            for issue in group:
                print(f"\n  {issue.message}")
                print(f"  File: {issue.file}:{issue.line}")
                if issue.context:
                    print(f"  Context: {issue.context}")

    # Summary
    critical = len(by_severity.get("CRITICAL", []))
    major = len(by_severity.get("MAJOR", []))
    minor = len(by_severity.get("MINOR", []))

    print(f"\n{'=' * 70}")
    print("SUMMARY")
    print(f"{'=' * 70}")
    print(f"  Critical: {critical}")
    print(f"  Major:    {major}")
    print(f"  Minor:    {minor}")

    if critical > 0:
        print(f"\n  ⚠️  {critical} CRITICAL tenant isolation issues found!")
        print(f"  These MUST be fixed before production deployment.")
        sys.exit(2)
    elif major > 0:
        print(f"\n  ⚠️  {major} potential tenant issues — verify manually.")
        sys.exit(1)
    else:
        print(f"\n  ✅  No tenant isolation issues detected.")
        sys.exit(0)


if __name__ == "__main__":
    main()
