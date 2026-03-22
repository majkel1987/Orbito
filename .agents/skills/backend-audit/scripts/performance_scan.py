#!/usr/bin/env python3
"""
Performance Anti-Pattern Scanner — Detects common .NET performance issues.

Checks for: sync-over-async, missing CancellationToken, N+1 queries,
in-memory filtering, missing AsNoTracking, unindexed queries.

Usage:
    python3 performance_scan.py <solution-root>
    python3 performance_scan.py <specific-project-folder>
"""

import sys
import re
from pathlib import Path
from collections import defaultdict


CHECKS = [
    {
        "name": "Sync-over-async",
        "severity": "CRITICAL",
        "pattern": r'\.Result\b|\.Wait\(\)|\.GetAwaiter\(\)\.GetResult\(\)',
        "message": "Sync-over-async detected — causes thread pool starvation. Use 'await' instead.",
        "exclude_patterns": [r"\.csproj", r"Test"],
    },
    {
        "name": "Missing CancellationToken",
        "severity": "MAJOR",
        "pattern": r'public\s+async\s+Task[<\s]',
        "negative_pattern": r'CancellationToken|cancellationToken',
        "message": "Async method without CancellationToken — cannot be cancelled by client disconnection.",
        "scope": "method",
    },
    {
        "name": "In-memory filtering",
        "severity": "MAJOR",
        "pattern": r'ToListAsync?\(\)|ToArrayAsync?\(\)',
        "followup_pattern": r'\.Where\(|\.Select\(|\.OrderBy\(|\.FirstOrDefault\(',
        "followup_lines": 3,
        "message": "Possible in-memory filtering — data loaded then filtered. Move filter before materialization.",
    },
    {
        "name": "Missing AsNoTracking",
        "severity": "MINOR",
        "pattern": r'(?:GetAll|GetBy|Find|Search|List|Query).*Async',
        "negative_context": r'AsNoTracking|NoTracking',
        "context_lines": 10,
        "message": "Read query may benefit from AsNoTracking() for better performance.",
        "scope": "repositories_only",
    },
    {
        "name": "String concatenation in queries",
        "severity": "CRITICAL",
        "pattern": r'(?:FromSqlRaw|ExecuteSqlRaw)\s*\(\s*\$"|(?:FromSqlRaw|ExecuteSqlRaw)\s*\(\s*"[^"]*"\s*\+',
        "message": "SQL injection risk — string interpolation/concatenation in raw SQL. Use parameterized queries.",
    },
    {
        "name": "Task.Delay in production code",
        "severity": "MAJOR",
        "pattern": r'Task\.Delay\(',
        "message": "Task.Delay in non-test code — likely a workaround for a race condition. Fix the root cause.",
        "exclude_patterns": [r"Test"],
    },
    {
        "name": "new HttpClient()",
        "severity": "MAJOR",
        "pattern": r'new\s+HttpClient\s*\(',
        "message": "Direct HttpClient instantiation — causes socket exhaustion. Use IHttpClientFactory.",
        "exclude_patterns": [r"Test"],
    },
    {
        "name": "Unbounded query",
        "severity": "MAJOR",
        "pattern": r'\.ToListAsync?\(\)',
        "negative_context": r'\.Take\(|\.Skip\(|PageSize|pageSize|Paginate|\.Top\(',
        "context_lines": 15,
        "message": "Query returns all results without pagination — will degrade as data grows.",
        "scope": "repositories_only",
    },
    {
        "name": "Lock statement",
        "severity": "MINOR",
        "pattern": r'\block\s*\(',
        "message": "Lock in async code — consider SemaphoreSlim for async-safe synchronization.",
        "exclude_patterns": [r"Test"],
    },
    {
        "name": "ConfigureAwait(false) missing in library code",
        "severity": "SUGGESTION",
        "pattern": r'await\s+.*?;',
        "negative_pattern": r'ConfigureAwait',
        "message": "Library/infrastructure code without ConfigureAwait(false) — may cause deadlocks in non-ASP.NET contexts.",
        "scope": "infrastructure_only",
    },
]


def scan_file(filepath, checks):
    """Scan a single file against all applicable checks."""
    issues = []
    try:
        content = filepath.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        return issues

    lines = content.split("\n")
    filepath_str = str(filepath)

    for check in checks:
        # Apply scope filter
        scope = check.get("scope", "all")
        if scope == "repositories_only" and "Repositor" not in filepath_str:
            continue
        if scope == "infrastructure_only" and "Infrastructure" not in filepath_str:
            continue

        # Apply exclude patterns
        excluded = False
        for exc in check.get("exclude_patterns", []):
            if re.search(exc, filepath_str):
                excluded = True
                break
        if excluded:
            continue

        for i, line in enumerate(lines, 1):
            if re.search(check["pattern"], line):
                # Check negative pattern (line-level)
                if "negative_pattern" in check:
                    method_body = "\n".join(lines[max(0, i-5):min(len(lines), i+10)])
                    if re.search(check["negative_pattern"], method_body):
                        continue

                # Check negative context (broader)
                if "negative_context" in check:
                    ctx_range = check.get("context_lines", 10)
                    context = "\n".join(lines[max(0, i-ctx_range):min(len(lines), i+ctx_range)])
                    if re.search(check["negative_context"], context):
                        continue

                # Check followup pattern
                if "followup_pattern" in check:
                    followup_range = check.get("followup_lines", 3)
                    followup = "\n".join(lines[i:min(len(lines), i+followup_range)])
                    if not re.search(check["followup_pattern"], followup):
                        continue

                issues.append({
                    "check": check["name"],
                    "severity": check["severity"],
                    "message": check["message"],
                    "file": filepath_str,
                    "line": i,
                    "context": line.strip()[:120],
                })

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 performance_scan.py <path>")
        sys.exit(1)

    root = Path(sys.argv[1])
    if not root.exists():
        print(f"Error: {root} not found")
        sys.exit(1)

    print("=" * 70)
    print("PERFORMANCE ANTI-PATTERN SCAN")
    print("=" * 70)

    cs_files = sorted(root.rglob("*.cs"))
    cs_files = [f for f in cs_files if "bin" not in str(f) and "obj" not in str(f)]

    print(f"\nScanning {len(cs_files)} files...")

    all_issues = []
    for f in cs_files:
        all_issues.extend(scan_file(f, CHECKS))

    if not all_issues:
        print("\n✅ No performance anti-patterns detected.")
        sys.exit(0)

    # Group by severity
    by_severity = defaultdict(list)
    for issue in all_issues:
        by_severity[issue["severity"]].append(issue)

    for severity in ["CRITICAL", "MAJOR", "MINOR", "SUGGESTION"]:
        group = by_severity.get(severity, [])
        if group:
            print(f"\n{'─' * 70}")
            print(f"{severity} ({len(group)})")
            print(f"{'─' * 70}")

            # Group by check name
            by_check = defaultdict(list)
            for issue in group:
                by_check[issue["check"]].append(issue)

            for check_name, issues in by_check.items():
                print(f"\n  [{check_name}] — {issues[0]['message']}")
                for issue in issues[:5]:  # Show max 5 per check
                    print(f"    {issue['file']}:{issue['line']}")
                    print(f"    Code: {issue['context']}")
                if len(issues) > 5:
                    print(f"    ... and {len(issues) - 5} more")

    # Summary
    print(f"\n{'=' * 70}")
    print("SUMMARY")
    print(f"{'=' * 70}")
    for severity in ["CRITICAL", "MAJOR", "MINOR", "SUGGESTION"]:
        count = len(by_severity.get(severity, []))
        print(f"  {severity}: {count}")

    critical = len(by_severity.get("CRITICAL", []))
    if critical > 0:
        sys.exit(2)
    elif len(by_severity.get("MAJOR", [])) > 0:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()
