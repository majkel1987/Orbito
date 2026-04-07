#!/usr/bin/env python3
"""
Performance Anti-Pattern Scanner

Scans for 10 common performance anti-patterns in .NET code.
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List, Optional


# Anti-pattern definitions
ANTI_PATTERNS = [
    {
        "name": "Sync-over-async",
        "severity": "CRITICAL",
        "description": "Blocking on async code causes deadlocks and thread pool starvation",
        "patterns": [
            r'\.Result\b',
            r'\.Wait\(\)',
            r'\.GetAwaiter\(\)\.GetResult\(\)',
        ],
        "exclude_projects": ["Orbito.Tests"],
        "exclude_patterns": [r'// OK:', r'// Intentional'],
    },
    {
        "name": "Missing CancellationToken",
        "severity": "MAJOR",
        "description": "Async methods should accept CancellationToken for graceful cancellation",
        "patterns": [
            r'public\s+async\s+Task\s+\w+\s*\([^)]*\)\s*$',
            r'public\s+async\s+Task<[^>]+>\s+\w+\s*\([^)]*\)\s*$',
        ],
        "negative_patterns": [
            r'CancellationToken',
        ],
        "exclude_projects": ["Orbito.Tests"],
    },
    {
        "name": "In-memory filtering",
        "severity": "MAJOR",
        "description": "Loading data then filtering in memory instead of database",
        "patterns": [
            r'\.ToList\(\).*\n.*\.Where\(',
            r'\.ToArray\(\).*\n.*\.Where\(',
            r'\.ToList\(\).*\n.*\.Select\(',
            r'\.ToList\(\).*\n.*\.OrderBy\(',
        ],
        "multiline": True,
    },
    {
        "name": "Missing AsNoTracking",
        "severity": "MINOR",
        "description": "Read-only queries should use AsNoTracking() for better performance",
        "patterns": [
            r'await\s+_\w*[Cc]ontext\.\w+\.Where\(',
            r'await\s+_\w*[Cc]ontext\.\w+\.FirstOrDefault',
            r'await\s+_\w*[Cc]ontext\.\w+\.SingleOrDefault',
        ],
        "negative_patterns": [
            r'AsNoTracking',
        ],
        "include_projects": ["Orbito.Infrastructure"],
        "include_patterns": [r'Repository\.cs$'],
    },
    {
        "name": "SQL injection risk",
        "severity": "CRITICAL",
        "description": "String interpolation in raw SQL allows SQL injection",
        "patterns": [
            r'FromSqlRaw\(\$"',
            r'FromSqlRaw\(".*\+',
            r'ExecuteSqlRaw\(\$"',
            r'ExecuteSqlRaw\(".*\+',
            r'FromSqlInterpolated\(\$".*{[^}]+}"',
        ],
    },
    {
        "name": "Task.Delay in production code",
        "severity": "MAJOR",
        "description": "Task.Delay is often a workaround for race conditions",
        "patterns": [
            r'Task\.Delay\(',
            r'Thread\.Sleep\(',
        ],
        "exclude_projects": ["Orbito.Tests"],
    },
    {
        "name": "Direct HttpClient instantiation",
        "severity": "MAJOR",
        "description": "new HttpClient() causes socket exhaustion. Use IHttpClientFactory",
        "patterns": [
            r'new\s+HttpClient\s*\(',
        ],
        "exclude_projects": ["Orbito.Tests"],
    },
    {
        "name": "Unbounded query",
        "severity": "MAJOR",
        "description": "ToList() without Take/Skip can load entire table into memory",
        "patterns": [
            r'\.ToListAsync\(\)',
            r'\.ToList\(\)',
        ],
        "negative_patterns": [
            r'\.Take\(',
            r'\.Skip\(',
            r'PageSize',
            r'pageSize',
            r'\.FirstOrDefault',
            r'\.SingleOrDefault',
        ],
        "include_projects": ["Orbito.Infrastructure"],
        "include_patterns": [r'Repository\.cs$'],
    },
    {
        "name": "Lock in async code",
        "severity": "MINOR",
        "description": "lock() blocks threads in async code. Use SemaphoreSlim instead",
        "patterns": [
            r'\block\s*\(',
        ],
        "context_patterns": [
            r'async\s+',
        ],
    },
    {
        "name": "Missing ConfigureAwait(false)",
        "severity": "SUGGESTION",
        "description": "Library/infrastructure code should use ConfigureAwait(false)",
        "patterns": [
            r'await\s+\w+\(',
        ],
        "negative_patterns": [
            r'ConfigureAwait',
        ],
        "include_projects": ["Orbito.Infrastructure"],
    },
]


class Issue:
    def __init__(self, severity: str, name: str, file: str, line: int, content: str, description: str):
        self.severity = severity
        self.name = name
        self.file = file
        self.line = line
        self.content = content
        self.description = description

    def __str__(self):
        return f"[{self.severity}] {self.name} — {self.file}:{self.line}\n  Issue: {self.description}\n  Code: {self.content.strip()[:80]}"


def should_scan_file(file_path: Path, pattern_def: Dict, root: Path) -> bool:
    """Check if file should be scanned based on pattern filters."""
    rel_path = str(file_path.relative_to(root))

    # Exclude patterns
    if "exclude_projects" in pattern_def:
        for proj in pattern_def["exclude_projects"]:
            if rel_path.startswith(proj):
                return False

    # Include patterns (if specified, must match)
    if "include_projects" in pattern_def:
        matches = False
        for proj in pattern_def["include_projects"]:
            if rel_path.startswith(proj):
                matches = True
                break
        if not matches:
            return False

    if "include_patterns" in pattern_def:
        matches = False
        for pat in pattern_def["include_patterns"]:
            if re.search(pat, rel_path):
                matches = True
                break
        if not matches:
            return False

    return True


def scan_file(file_path: Path, root: Path) -> List[Issue]:
    """Scan a single file for anti-patterns."""
    issues = []

    try:
        content = file_path.read_text(encoding="utf-8")
    except Exception:
        return issues

    lines = content.split('\n')
    rel_path = str(file_path.relative_to(root))

    for pattern_def in ANTI_PATTERNS:
        if not should_scan_file(file_path, pattern_def, root):
            continue

        for pattern in pattern_def["patterns"]:
            if pattern_def.get("multiline"):
                # Multiline pattern - search entire content
                for match in re.finditer(pattern, content, re.MULTILINE):
                    line_num = content[:match.start()].count('\n') + 1

                    # Check negative patterns
                    if "negative_patterns" in pattern_def:
                        skip = False
                        context = content[max(0, match.start()-200):match.end()+200]
                        for neg_pat in pattern_def["negative_patterns"]:
                            if re.search(neg_pat, context):
                                skip = True
                                break
                        if skip:
                            continue

                    issues.append(Issue(
                        pattern_def["severity"],
                        pattern_def["name"],
                        rel_path,
                        line_num,
                        match.group(0)[:80],
                        pattern_def["description"]
                    ))
            else:
                # Line-by-line pattern
                for line_num, line in enumerate(lines, 1):
                    if re.search(pattern, line):
                        # Check exclude patterns
                        if "exclude_patterns" in pattern_def:
                            skip = False
                            for excl_pat in pattern_def["exclude_patterns"]:
                                if re.search(excl_pat, line):
                                    skip = True
                                    break
                            if skip:
                                continue

                        # Check negative patterns (must NOT match)
                        if "negative_patterns" in pattern_def:
                            # Look in surrounding context (5 lines before/after)
                            context_start = max(0, line_num - 6)
                            context_end = min(len(lines), line_num + 5)
                            context = '\n'.join(lines[context_start:context_end])

                            skip = False
                            for neg_pat in pattern_def["negative_patterns"]:
                                if re.search(neg_pat, context):
                                    skip = True
                                    break
                            if skip:
                                continue

                        # Check context patterns (must match somewhere in file)
                        if "context_patterns" in pattern_def:
                            context_match = False
                            for ctx_pat in pattern_def["context_patterns"]:
                                if re.search(ctx_pat, content):
                                    context_match = True
                                    break
                            if not context_match:
                                continue

                        issues.append(Issue(
                            pattern_def["severity"],
                            pattern_def["name"],
                            rel_path,
                            line_num,
                            line,
                            pattern_def["description"]
                        ))

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: performance_scan.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()

    print("=" * 60)
    print("PERFORMANCE ANTI-PATTERN SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}")
    print(f"Checking {len(ANTI_PATTERNS)} anti-patterns\n")

    # Find all .cs files
    cs_files = []
    for f in root.rglob("*.cs"):
        if "/bin/" not in str(f) and "/obj/" not in str(f):
            if "\\bin\\" not in str(f) and "\\obj\\" not in str(f):
                cs_files.append(f)

    print(f"Found {len(cs_files)} .cs files to scan\n")

    # Scan files
    all_issues = []
    for file_path in cs_files:
        issues = scan_file(file_path, root)
        all_issues.extend(issues)

    # Group by severity
    critical = [i for i in all_issues if i.severity == "CRITICAL"]
    major = [i for i in all_issues if i.severity == "MAJOR"]
    minor = [i for i in all_issues if i.severity == "MINOR"]
    suggestions = [i for i in all_issues if i.severity == "SUGGESTION"]

    print("=" * 60)
    print("RESULTS")
    print("=" * 60)
    print(f"\nSummary: {len(critical)} CRITICAL, {len(major)} MAJOR, {len(minor)} MINOR, {len(suggestions)} SUGGESTION\n")

    if critical:
        print("\n### CRITICAL ISSUES ###")
        for issue in critical:
            print(f"\n{issue}")

    if major:
        print("\n### MAJOR ISSUES ###")
        for issue in major[:20]:  # Limit output
            print(f"\n{issue}")
        if len(major) > 20:
            print(f"\n... and {len(major) - 20} more MAJOR issues")

    if minor:
        print("\n### MINOR ISSUES ###")
        for issue in minor[:10]:
            print(f"\n{issue}")
        if len(minor) > 10:
            print(f"\n... and {len(minor) - 10} more MINOR issues")

    if suggestions and not (critical or major):
        print("\n### SUGGESTIONS ###")
        for issue in suggestions[:10]:
            print(f"\n{issue}")
        if len(suggestions) > 10:
            print(f"\n... and {len(suggestions) - 10} more suggestions")

    # Summary by anti-pattern
    print("\n" + "=" * 60)
    print("ISSUES BY ANTI-PATTERN")
    print("=" * 60)

    pattern_counts = {}
    for issue in all_issues:
        key = f"[{issue.severity}] {issue.name}"
        pattern_counts[key] = pattern_counts.get(key, 0) + 1

    for key, count in sorted(pattern_counts.items(), key=lambda x: x[1], reverse=True):
        print(f"  {count:3} {key}")

    if not all_issues:
        print("\n✅ No performance anti-patterns found!")

    # Exit code
    if critical:
        sys.exit(2)
    elif major:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()
