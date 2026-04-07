#!/usr/bin/env python3
"""
Multi-Tenant Security Scanner

CRITICAL scanner for detecting cross-tenant data leaks.
Checks:
1. Repositories - ITenantContext injection and filtering
2. Query handlers - ITenantContext references
3. Controllers - TenantId source and authorization
4. Cache keys - tenant isolation
5. Background jobs - tenant context setup
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List, Optional


class Issue:
    def __init__(self, severity: str, category: str, file: str, line: int, message: str, fix: str = ""):
        self.severity = severity
        self.category = category
        self.file = file
        self.line = line
        self.message = message
        self.fix = fix

    def __str__(self):
        result = f"[{self.severity}] {self.category} — {self.file}:{self.line}\n  Issue: {self.message}"
        if self.fix:
            result += f"\n  Fix: {self.fix}"
        return result


def scan_repositories(root: Path) -> List[Issue]:
    """Check that repositories inject and use ITenantContext."""
    issues = []

    # Find repository files
    infra_path = root / "Orbito.Infrastructure"
    if not infra_path.exists():
        return issues

    repo_files = []
    for f in infra_path.rglob("*Repository.cs"):
        if "/bin/" not in str(f) and "/obj/" not in str(f):
            if "\\bin\\" not in str(f) and "\\obj\\" not in str(f):
                repo_files.append(f)

    for file_path in repo_files:
        try:
            content = file_path.read_text(encoding="utf-8")
        except Exception:
            continue

        rel_path = str(file_path.relative_to(root))
        lines = content.split('\n')

        # Check 1: ITenantContext injection
        if "ITenantContext" not in content:
            issues.append(Issue(
                "CRITICAL",
                "Repository Security",
                rel_path,
                1,
                "Repository does not inject ITenantContext — potential cross-tenant data leak",
                "Add ITenantContext to constructor and use it for filtering"
            ))
            continue  # No point checking further if no tenant context

        # Check 2: Public async methods should filter by TenantId
        method_pattern = re.compile(
            r'public\s+async\s+Task[<\s].*?\s+(\w+)\s*\([^)]*\)',
            re.DOTALL
        )

        for match in method_pattern.finditer(content):
            method_name = match.group(1)

            # Skip known safe methods
            if method_name in ["SaveChangesAsync", "Dispose", "CommitAsync", "BeginTransactionAsync"]:
                continue

            # Find method body
            method_start = match.end()
            brace_count = 0
            method_end = method_start
            started = False

            for i, char in enumerate(content[method_start:], method_start):
                if char == '{':
                    brace_count += 1
                    started = True
                elif char == '}':
                    brace_count -= 1
                    if started and brace_count == 0:
                        method_end = i
                        break

            method_body = content[method_start:method_end]

            # Check if method uses TenantId filtering
            if "TenantId" not in method_body and "_tenantContext" not in method_body:
                line_num = content[:match.start()].count('\n') + 1
                issues.append(Issue(
                    "CRITICAL",
                    "Repository Security",
                    rel_path,
                    line_num,
                    f"Method {method_name} does not filter by TenantId",
                    f"Add .Where(x => x.TenantId == _tenantContext.CurrentTenantId) to query"
                ))

    return issues


def scan_query_handlers(root: Path) -> List[Issue]:
    """Check that query handlers reference ITenantContext."""
    issues = []

    app_path = root / "Orbito.Application"
    if not app_path.exists():
        return issues

    handler_files = []
    for f in app_path.rglob("*Handler.cs"):
        if "/bin/" not in str(f) and "/obj/" not in str(f):
            if "\\bin\\" not in str(f) and "\\obj\\" not in str(f):
                handler_files.append(f)

    for file_path in handler_files:
        try:
            content = file_path.read_text(encoding="utf-8")
        except Exception:
            continue

        rel_path = str(file_path.relative_to(root))

        # Skip admin-only handlers
        if "Admin" in file_path.name or "[Authorize(Roles = \"Admin\")]" in content:
            continue

        # Check if handler implements IRequestHandler
        if "IRequestHandler" not in content:
            continue

        # Check for ITenantContext
        if "ITenantContext" not in content:
            issues.append(Issue(
                "MAJOR",
                "Handler Security",
                rel_path,
                1,
                "Handler does not reference ITenantContext — may access wrong tenant data",
                "Inject ITenantContext or ensure repository handles tenant filtering"
            ))

    return issues


def scan_controllers(root: Path) -> List[Issue]:
    """Check controller security: TenantId source and authorization."""
    issues = []

    api_path = root / "Orbito.API"
    if not api_path.exists():
        return issues

    controller_files = []
    for f in api_path.rglob("*Controller.cs"):
        if "/bin/" not in str(f) and "/obj/" not in str(f):
            if "\\bin\\" not in str(f) and "\\obj\\" not in str(f):
                controller_files.append(f)

    for file_path in controller_files:
        try:
            content = file_path.read_text(encoding="utf-8")
        except Exception:
            continue

        rel_path = str(file_path.relative_to(root))
        lines = content.split('\n')

        # Check 1: TenantId from FromBody/FromQuery (CRITICAL - must come from claims)
        for line_num, line in enumerate(lines, 1):
            if re.search(r'\[FromBody\].*[Tt]enant[Ii]d', line) or \
               re.search(r'\[FromQuery\].*[Tt]enant[Ii]d', line) or \
               re.search(r'[Tt]enant[Ii]d.*\[FromBody\]', line) or \
               re.search(r'[Tt]enant[Ii]d.*\[FromQuery\]', line):
                issues.append(Issue(
                    "CRITICAL",
                    "Controller Security",
                    rel_path,
                    line_num,
                    "TenantId comes from request body/query — must come from auth claims",
                    "Get TenantId from User.Claims or ITenantContext"
                ))

        # Check 2: [AllowAnonymous] on mutation endpoints
        allow_anon_lines = []
        for line_num, line in enumerate(lines, 1):
            if "[AllowAnonymous]" in line:
                allow_anon_lines.append(line_num)

        for anon_line in allow_anon_lines:
            # Look for HTTP method in next 5 lines
            for i in range(anon_line, min(anon_line + 5, len(lines) + 1)):
                line = lines[i - 1] if i <= len(lines) else ""
                if re.search(r'\[Http(Post|Put|Delete|Patch)\]', line):
                    issues.append(Issue(
                        "CRITICAL",
                        "Controller Security",
                        rel_path,
                        anon_line,
                        "[AllowAnonymous] on mutation endpoint — potential unauthorized access",
                        "Remove [AllowAnonymous] or verify this is intentional (e.g., registration)"
                    ))
                    break

        # Check 3: Missing [Authorize] on controller class
        if "[Authorize]" not in content and "[AllowAnonymous]" not in content:
            if "Controller" in file_path.name and "Base" not in file_path.name:
                issues.append(Issue(
                    "MAJOR",
                    "Controller Security",
                    rel_path,
                    1,
                    "Controller has no [Authorize] attribute — endpoints may be unprotected",
                    "Add [Authorize] to controller class or specific actions"
                ))

    return issues


def scan_cache_keys(root: Path) -> List[Issue]:
    """Check that cache operations include TenantId in keys."""
    issues = []

    # Scan all .cs files for cache operations
    for project in ["Orbito.Application", "Orbito.Infrastructure"]:
        project_path = root / project
        if not project_path.exists():
            continue

        for file_path in project_path.rglob("*.cs"):
            if "/bin/" in str(file_path) or "/obj/" in str(file_path):
                continue
            if "\\bin\\" in str(file_path) or "\\obj\\" in str(file_path):
                continue

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            rel_path = str(file_path.relative_to(root))
            lines = content.split('\n')

            for line_num, line in enumerate(lines, 1):
                # Check for cache operations
                if re.search(r'Cache\.(Get|Set|Remove)', line) or \
                   re.search(r'_cache\.(Get|Set|Remove)', line) or \
                   re.search(r'IDistributedCache', line):

                    # Look for TenantId in cache key (current line and next 2)
                    context = '\n'.join(lines[line_num-1:line_num+2])
                    if "TenantId" not in context and "tenantId" not in context:
                        issues.append(Issue(
                            "MAJOR",
                            "Cache Security",
                            rel_path,
                            line_num,
                            "Cache operation may not include TenantId in key — cross-tenant cache pollution",
                            "Include TenantId in cache key: $\"{tenantId}:{entityType}:{id}\""
                        ))

    return issues


def scan_background_jobs(root: Path) -> List[Issue]:
    """Check that background jobs set tenant context before data access."""
    issues = []

    # Find job files
    for project in ["Orbito.Application", "Orbito.Infrastructure"]:
        project_path = root / project
        if not project_path.exists():
            continue

        job_files = []
        for f in project_path.rglob("*Job.cs"):
            if "/bin/" not in str(f) and "/obj/" not in str(f):
                if "\\bin\\" not in str(f) and "\\obj\\" not in str(f):
                    job_files.append(f)

        for file_path in job_files:
            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            rel_path = str(file_path.relative_to(root))

            # Check if job does data access
            has_data_access = any([
                "Repository" in content,
                "DbContext" in content,
                "SaveChanges" in content,
                "_context" in content,
            ])

            if not has_data_access:
                continue

            # Check if job sets tenant context
            sets_tenant = any([
                "SetTenant" in content,
                "ITenantContext" in content,
                "TenantScope" in content,
                "WithTenant" in content,
            ])

            if not sets_tenant:
                issues.append(Issue(
                    "CRITICAL",
                    "Background Job Security",
                    rel_path,
                    1,
                    "Background job accesses data without setting tenant context",
                    "Inject ITenantContext and set tenant before data access"
                ))

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: tenant_security_scan.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()

    print("=" * 60)
    print("MULTI-TENANT SECURITY SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}")
    print("\n⚠️  This is the MOST CRITICAL security scan!")
    print("    Cross-tenant data leaks are the #1 security risk.\n")

    all_issues = []

    # Scan 1: Repositories
    print("Scanning repositories...")
    all_issues.extend(scan_repositories(root))

    # Scan 2: Query handlers
    print("Scanning query handlers...")
    all_issues.extend(scan_query_handlers(root))

    # Scan 3: Controllers
    print("Scanning controllers...")
    all_issues.extend(scan_controllers(root))

    # Scan 4: Cache keys
    print("Scanning cache operations...")
    all_issues.extend(scan_cache_keys(root))

    # Scan 5: Background jobs
    print("Scanning background jobs...")
    all_issues.extend(scan_background_jobs(root))

    # Results
    print("\n" + "=" * 60)
    print("RESULTS")
    print("=" * 60)

    critical = [i for i in all_issues if i.severity == "CRITICAL"]
    major = [i for i in all_issues if i.severity == "MAJOR"]

    print(f"\nSummary: {len(critical)} CRITICAL, {len(major)} MAJOR\n")

    if critical:
        print("\n### CRITICAL ISSUES — FIX IMMEDIATELY ###")
        for issue in critical:
            print(f"\n{issue}")

    if major:
        print("\n### MAJOR ISSUES ###")
        for issue in major:
            print(f"\n{issue}")

    if not all_issues:
        print("\n✅ No tenant security issues found!")

    # Summary by category
    print("\n" + "=" * 60)
    print("ISSUES BY CATEGORY")
    print("=" * 60)

    category_counts = {}
    for issue in all_issues:
        key = f"[{issue.severity}] {issue.category}"
        category_counts[key] = category_counts.get(key, 0) + 1

    for key, count in sorted(category_counts.items(), key=lambda x: x[1], reverse=True):
        print(f"  {count:3} {key}")

    # Exit code
    if critical:
        print("\n🚨 CRITICAL tenant security issues found! Fix before deployment!")
        sys.exit(2)
    elif major:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()
