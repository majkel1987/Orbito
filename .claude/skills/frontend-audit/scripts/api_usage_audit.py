#!/usr/bin/env python3
"""
API Usage Audit — KRYTYCZNY SKRYPT

Sprawdza czy używane są TYLKO hooki Orval (nie raw fetch/axios):
- Czy KAŻDY komponent z danymi ma loading/error/empty state
- Czy mutacje invalidują cache
- Czy nie ma hardcoded danych
- Czy nie ma TODO/mock/placeholder
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List, Set, Tuple


# ZERO TOLERANCE patterns
CRITICAL_PATTERNS = [
    # Raw API calls (nie Orval)
    (r'\bfetch\s*\(', "Raw fetch() instead of Orval hook", "CRITICAL"),
    (r'axios\.(get|post|put|delete|patch)\s*\(', "Raw axios instead of Orval hook", "CRITICAL"),
    (r'axios\s*\(', "Raw axios instance instead of Orval hook", "CRITICAL"),

    # Hardcoded data
    (r'>\s*0\s*</', "Possible hardcoded zero value", "CRITICAL"),
    (r'>\s*\$0\s*</', "Hardcoded $0 value", "CRITICAL"),
    (r'const\s+\w+\s*=\s*\[\s*\]', "Empty array (should use API data)", "MAJOR"),

    # Mock/TODO/placeholder
    (r'//\s*TODO', "TODO comment", "MAJOR"),
    (r'//\s*FIXME', "FIXME comment", "MAJOR"),
    (r'console\.log\(["\']TODO', "TODO in console.log", "CRITICAL"),
    (r'placeholder', "Placeholder text (check if intentional)", "MINOR"),
]

# Components that fetch data MUST have these patterns
REQUIRED_PATTERNS = {
    "loading": [
        r'isLoading',
        r'isPending',
        r'loading\b',
        r'<Skeleton',
        r'<Loading',
    ],
    "error": [
        r'\berror\b',
        r'isError',
        r'<Error',
        r'toast\.error',
    ],
    "empty": [
        r'\.length\s*===?\s*0',
        r'!data',
        r'<Empty',
        r'No\s+\w+\s+found',
        r'EmptyState',
    ],
}

# Mutation patterns that should invalidate cache
MUTATION_PATTERNS = [
    r'useMutation',
    r'usePost\w+',
    r'usePut\w+',
    r'useDelete\w+',
    r'usePatch\w+',
]

CACHE_INVALIDATION_PATTERNS = [
    r'invalidateQueries',
    r'setQueryData',
    r'resetQueries',
]


def check_api_usage(file_path: Path, root: Path) -> List[Dict]:
    """Check a file for API usage violations."""
    issues = []
    rel_path = str(file_path.relative_to(root))

    try:
        content = file_path.read_text(encoding="utf-8")
        lines = content.split('\n')
    except Exception:
        return []

    # Check CRITICAL patterns
    for line_num, line in enumerate(lines, 1):
        for pattern, description, severity in CRITICAL_PATTERNS:
            if re.search(pattern, line, re.IGNORECASE):
                # Skip in client.ts (Axios config is OK there)
                if 'client.ts' in rel_path and 'axios' in pattern:
                    continue
                # Skip in test files
                if '.test.' in rel_path or '.spec.' in rel_path:
                    continue
                # Skip in generated files
                if 'generated' in rel_path:
                    continue

                issues.append({
                    "file": rel_path,
                    "line": line_num,
                    "type": "API_VIOLATION",
                    "severity": severity,
                    "description": description,
                    "content": line.strip()[:80]
                })

    return issues


def check_data_states(file_path: Path, root: Path) -> List[Dict]:
    """Check if components with data have loading/error/empty states."""
    issues = []
    rel_path = str(file_path.relative_to(root))

    try:
        content = file_path.read_text(encoding="utf-8")
    except Exception:
        return []

    # Skip non-component files
    if 'generated' in rel_path:
        return []
    if '.test.' in rel_path or '.spec.' in rel_path:
        return []

    # Check if file uses data fetching hooks
    has_query = bool(re.search(r'useQuery|useGet\w+|use\w+Query', content))

    if not has_query:
        return []

    # Check for required patterns
    missing_states = []

    # Check loading state
    has_loading = any(re.search(p, content) for p in REQUIRED_PATTERNS["loading"])
    if not has_loading:
        missing_states.append("loading")

    # Check error state
    has_error = any(re.search(p, content) for p in REQUIRED_PATTERNS["error"])
    if not has_error:
        missing_states.append("error")

    # Check empty state
    has_empty = any(re.search(p, content) for p in REQUIRED_PATTERNS["empty"])
    if not has_empty:
        missing_states.append("empty")

    if missing_states:
        issues.append({
            "file": rel_path,
            "line": 1,
            "type": "MISSING_STATES",
            "severity": "MAJOR",
            "description": f"Component fetches data but missing states: {', '.join(missing_states)}",
            "content": ""
        })

    return issues


def check_mutation_invalidation(file_path: Path, root: Path) -> List[Dict]:
    """Check if mutations properly invalidate cache."""
    issues = []
    rel_path = str(file_path.relative_to(root))

    try:
        content = file_path.read_text(encoding="utf-8")
    except Exception:
        return []

    # Skip generated and test files
    if 'generated' in rel_path or '.test.' in rel_path:
        return []

    # Check if file has mutations
    has_mutation = any(re.search(p, content) for p in MUTATION_PATTERNS)

    if not has_mutation:
        return []

    # Check for cache invalidation
    has_invalidation = any(re.search(p, content) for p in CACHE_INVALIDATION_PATTERNS)

    if not has_invalidation:
        # Check if mutation has onSuccess callback
        if 'onSuccess' in content:
            # onSuccess exists but no invalidation
            issues.append({
                "file": rel_path,
                "line": 1,
                "type": "NO_CACHE_INVALIDATION",
                "severity": "MAJOR",
                "description": "Mutation has onSuccess but no cache invalidation",
                "content": ""
            })
        else:
            issues.append({
                "file": rel_path,
                "line": 1,
                "type": "NO_CACHE_INVALIDATION",
                "severity": "MAJOR",
                "description": "Mutation without onSuccess/cache invalidation",
                "content": ""
            })

    return issues


def check_orval_usage(root: Path) -> Dict:
    """Verify Orval hooks are being used correctly."""
    src = root / "src"
    stats = {
        "files_with_queries": 0,
        "files_with_mutations": 0,
        "files_using_raw_fetch": 0,
        "files_using_raw_axios": 0,
    }

    for ext in ["*.ts", "*.tsx"]:
        for file_path in src.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue
            if "generated" in str(file_path):
                continue
            if "client.ts" in str(file_path):
                continue

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            if re.search(r'useQuery|useGet\w+', content):
                stats["files_with_queries"] += 1

            if any(re.search(p, content) for p in MUTATION_PATTERNS):
                stats["files_with_mutations"] += 1

            if re.search(r'\bfetch\s*\(', content):
                stats["files_using_raw_fetch"] += 1

            if re.search(r'axios\.(get|post|put|delete|patch)\s*\(', content):
                stats["files_using_raw_axios"] += 1

    return stats


def main():
    if len(sys.argv) < 2:
        print("Usage: api_usage_audit.py <frontend-root>")
        print("Example: api_usage_audit.py orbito-frontend/")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()
    src = root / "src"

    if not src.exists():
        print(f"Error: src/ directory not found in {root}")
        sys.exit(1)

    print("=" * 60)
    print("API USAGE AUDIT — KRYTYCZNY SKRYPT")
    print("=" * 60)
    print(f"Scanning: {root}\n")
    print("Zero Tolerance: fetch(), axios (use Orval hooks only)\n")

    # Collect all issues
    api_issues = []
    state_issues = []
    mutation_issues = []

    for ext in ["*.ts", "*.tsx"]:
        for file_path in src.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue

            api_issues.extend(check_api_usage(file_path, root))
            state_issues.extend(check_data_states(file_path, root))
            mutation_issues.extend(check_mutation_invalidation(file_path, root))

    # Usage statistics
    stats = check_orval_usage(root)

    print("=" * 60)
    print("API USAGE STATISTICS")
    print("=" * 60)
    print(f"  Files using queries: {stats['files_with_queries']}")
    print(f"  Files using mutations: {stats['files_with_mutations']}")
    print(f"  Files using raw fetch(): {stats['files_using_raw_fetch']}")
    print(f"  Files using raw axios: {stats['files_using_raw_axios']}")

    # Critical violations
    critical = [i for i in api_issues if i['severity'] == 'CRITICAL']
    major = [i for i in api_issues if i['severity'] == 'MAJOR']

    print("\n" + "=" * 60)
    print(f"CRITICAL API VIOLATIONS ({len(critical)} found)")
    print("=" * 60)

    if critical:
        print("\n  ⚠️  CRITICAL: These must be fixed immediately!\n")
        for issue in critical[:15]:
            print(f"  [CRITICAL] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
            print(f"    {issue['content']}")
            print()
        if len(critical) > 15:
            print(f"  ... and {len(critical) - 15} more")
    else:
        print("\n  ✅ No critical API violations")

    # Major violations
    print("\n" + "=" * 60)
    print(f"MAJOR API VIOLATIONS ({len(major)} found)")
    print("=" * 60)

    if major:
        for issue in major[:10]:
            print(f"\n  [MAJOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
        if len(major) > 10:
            print(f"\n  ... and {len(major) - 10} more")
    else:
        print("\n  ✅ No major API violations")

    # Missing states
    print("\n" + "=" * 60)
    print(f"MISSING DATA STATES ({len(state_issues)} found)")
    print("=" * 60)

    if state_issues:
        print("\n  Components that fetch data must have: loading, error, empty states\n")
        for issue in state_issues[:15]:
            print(f"  [MAJOR] {issue['file']}")
            print(f"    {issue['description']}")
        if len(state_issues) > 15:
            print(f"\n  ... and {len(state_issues) - 15} more")
    else:
        print("\n  ✅ All data components have proper states")

    # Mutation cache invalidation
    print("\n" + "=" * 60)
    print(f"MUTATION CACHE ISSUES ({len(mutation_issues)} found)")
    print("=" * 60)

    if mutation_issues:
        print("\n  Mutations should invalidate relevant queries on success\n")
        for issue in mutation_issues[:10]:
            print(f"  [MAJOR] {issue['file']}")
            print(f"    {issue['description']}")
        if len(mutation_issues) > 10:
            print(f"\n  ... and {len(mutation_issues) - 10} more")
    else:
        print("\n  ✅ All mutations properly invalidate cache")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    total_critical = len(critical)
    total_major = len(major) + len(state_issues) + len(mutation_issues)

    print(f"  Critical issues: {total_critical}")
    print(f"  Major issues: {total_major}")

    if total_critical > 0:
        print("\n  ❌ FAIL: Critical API violations found!")
        print("  Fix: Replace fetch()/axios with Orval hooks from @/core/api/generated/")
        sys.exit(1)
    elif total_major > 0:
        print("\n  ⚠️  WARNING: Major issues need attention")
        sys.exit(0)
    else:
        print("\n  ✅ API usage looks GOOD!")
        sys.exit(0)


if __name__ == "__main__":
    main()
