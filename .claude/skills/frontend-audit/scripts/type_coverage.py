#!/usr/bin/env python3
"""
Type Coverage Scanner

Finds TypeScript type issues:
- `any` usage
- `as` type assertions
- @ts-ignore / @ts-expect-error comments
- Missing types on props
- Implicit `any` in event handlers
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List


# Patterns to detect
ANY_PATTERNS = [
    (r'\b:\s*any\b', "Explicit any type"),
    (r'\bany\[\]', "Array of any"),
    (r'\bRecord<\w+,\s*any>', "Record with any value"),
    (r'\bPromise<any>', "Promise<any>"),
    (r'\bas\s+any\b', "Cast to any"),
]

ASSERTION_PATTERNS = [
    (r'\bas\s+[A-Z]\w+', "Type assertion (as)"),
    (r'<[A-Z]\w+>\s*\w+', "Type assertion (angle bracket)"),
]

IGNORE_PATTERNS = [
    (r'@ts-ignore', "@ts-ignore comment"),
    (r'@ts-expect-error', "@ts-expect-error comment"),
    (r'@ts-nocheck', "@ts-nocheck comment"),
]

# Event handler implicit any patterns
EVENT_HANDLER_PATTERNS = [
    (r'onChange=\{\s*\(e\)\s*=>', "Implicit any on onChange handler"),
    (r'onClick=\{\s*\(e\)\s*=>', "Implicit any on onClick handler"),
    (r'onSubmit=\{\s*\(e\)\s*=>', "Implicit any on onSubmit handler"),
    (r'onInput=\{\s*\(e\)\s*=>', "Implicit any on onInput handler"),
]

# Files/dirs to exclude
EXCLUDE_PATTERNS = [
    "node_modules",
    ".next",
    "generated",
    ".d.ts",
    ".test.",
    ".spec.",
]


def should_skip(file_path: str) -> bool:
    """Check if file should be skipped."""
    for pattern in EXCLUDE_PATTERNS:
        if pattern in file_path:
            return True
    return False


def scan_file(file_path: Path, root: Path) -> List[Dict]:
    """Scan a single file for type issues."""
    issues = []
    rel_path = str(file_path.relative_to(root))

    try:
        content = file_path.read_text(encoding="utf-8")
        lines = content.split('\n')
    except Exception:
        return []

    for line_num, line in enumerate(lines, 1):
        # Skip comment lines
        stripped = line.strip()
        if stripped.startswith("//") and "@ts-" not in stripped:
            continue
        if stripped.startswith("*"):
            continue

        # Check for any patterns
        for pattern, description in ANY_PATTERNS:
            if re.search(pattern, line):
                issues.append({
                    "file": rel_path,
                    "line": line_num,
                    "type": "ANY",
                    "severity": "MAJOR",
                    "description": description,
                    "content": stripped[:80]
                })

        # Check for @ts-ignore patterns
        for pattern, description in IGNORE_PATTERNS:
            if re.search(pattern, line, re.IGNORECASE):
                issues.append({
                    "file": rel_path,
                    "line": line_num,
                    "type": "TS_IGNORE",
                    "severity": "MAJOR",
                    "description": description,
                    "content": stripped[:80]
                })

        # Check for type assertions (as casts) - lower severity
        for pattern, description in ASSERTION_PATTERNS:
            if re.search(pattern, line):
                # Skip common safe assertions
                if " as const" in line or " as React" in line:
                    continue
                if " as string" in line or " as number" in line:
                    continue

                issues.append({
                    "file": rel_path,
                    "line": line_num,
                    "type": "ASSERTION",
                    "severity": "MINOR",
                    "description": description,
                    "content": stripped[:80]
                })

        # Check for implicit any in event handlers
        for pattern, description in EVENT_HANDLER_PATTERNS:
            if re.search(pattern, line):
                # Check if type is specified
                if ": React." not in line and ": ChangeEvent" not in line and ": FormEvent" not in line:
                    issues.append({
                        "file": rel_path,
                        "line": line_num,
                        "type": "IMPLICIT_ANY",
                        "severity": "MINOR",
                        "description": description,
                        "content": stripped[:80]
                    })

    return issues


def analyze_coverage(root: Path) -> Dict:
    """Calculate overall type coverage statistics."""
    stats = {
        "total_files": 0,
        "files_with_any": 0,
        "files_with_ignore": 0,
        "total_any": 0,
        "total_ignore": 0,
        "total_assertions": 0,
        "total_implicit_any": 0,
    }

    src = root / "src"
    if not src.exists():
        return stats

    for ext in ["*.ts", "*.tsx"]:
        for file_path in src.rglob(ext):
            if should_skip(str(file_path)):
                continue

            stats["total_files"] += 1
            issues = scan_file(file_path, root)

            has_any = False
            has_ignore = False

            for issue in issues:
                if issue["type"] == "ANY":
                    stats["total_any"] += 1
                    has_any = True
                elif issue["type"] == "TS_IGNORE":
                    stats["total_ignore"] += 1
                    has_ignore = True
                elif issue["type"] == "ASSERTION":
                    stats["total_assertions"] += 1
                elif issue["type"] == "IMPLICIT_ANY":
                    stats["total_implicit_any"] += 1

            if has_any:
                stats["files_with_any"] += 1
            if has_ignore:
                stats["files_with_ignore"] += 1

    return stats


def main():
    if len(sys.argv) < 2:
        print("Usage: type_coverage.py <frontend-root>")
        print("Example: type_coverage.py orbito-frontend/")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()
    src = root / "src"

    if not src.exists():
        print(f"Error: src/ directory not found in {root}")
        sys.exit(1)

    print("=" * 60)
    print("TYPE COVERAGE SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}\n")

    # Collect all issues
    all_issues = []

    for ext in ["*.ts", "*.tsx"]:
        for file_path in src.rglob(ext):
            if should_skip(str(file_path)):
                continue

            issues = scan_file(file_path, root)
            all_issues.extend(issues)

    # Coverage statistics
    stats = analyze_coverage(root)

    print("=" * 60)
    print("TYPE COVERAGE STATISTICS")
    print("=" * 60)

    print(f"  Total TypeScript files: {stats['total_files']}")
    print(f"  Files with `any`: {stats['files_with_any']} ({stats['files_with_any']/max(1,stats['total_files'])*100:.1f}%)")
    print(f"  Files with @ts-ignore: {stats['files_with_ignore']}")

    # Calculate strict compliance
    strict_compliance = 100 - (stats['total_any'] / max(1, stats['total_files'] * 10) * 100)
    print(f"\n  TypeScript Strict Compliance: {strict_compliance:.1f}%")

    # Group issues by type
    any_issues = [i for i in all_issues if i['type'] == 'ANY']
    ignore_issues = [i for i in all_issues if i['type'] == 'TS_IGNORE']
    assertion_issues = [i for i in all_issues if i['type'] == 'ASSERTION']
    implicit_any_issues = [i for i in all_issues if i['type'] == 'IMPLICIT_ANY']

    # ANY usages
    print("\n" + "=" * 60)
    print(f"ANY USAGES ({len(any_issues)} found)")
    print("=" * 60)

    if any_issues:
        for issue in any_issues[:15]:
            print(f"\n  [MAJOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
            print(f"    {issue['content']}")
        if len(any_issues) > 15:
            print(f"\n  ... and {len(any_issues) - 15} more")
    else:
        print("\n  ✅ No `any` usages found!")

    # @ts-ignore usages
    print("\n" + "=" * 60)
    print(f"@TS-IGNORE / @TS-EXPECT-ERROR ({len(ignore_issues)} found)")
    print("=" * 60)

    if ignore_issues:
        for issue in ignore_issues[:10]:
            print(f"\n  [MAJOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
            print(f"    {issue['content']}")
        if len(ignore_issues) > 10:
            print(f"\n  ... and {len(ignore_issues) - 10} more")
    else:
        print("\n  ✅ No @ts-ignore comments found!")

    # Type assertions
    print("\n" + "=" * 60)
    print(f"TYPE ASSERTIONS ({len(assertion_issues)} found)")
    print("=" * 60)

    if assertion_issues:
        print(f"\n  Found {len(assertion_issues)} type assertions (some may be intentional)")
        for issue in assertion_issues[:10]:
            print(f"\n  [MINOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['content']}")
        if len(assertion_issues) > 10:
            print(f"\n  ... and {len(assertion_issues) - 10} more")
    else:
        print("\n  ✅ No problematic type assertions found")

    # Implicit any in handlers
    print("\n" + "=" * 60)
    print(f"IMPLICIT ANY IN EVENT HANDLERS ({len(implicit_any_issues)} found)")
    print("=" * 60)

    if implicit_any_issues:
        for issue in implicit_any_issues[:10]:
            print(f"\n  [MINOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
            print(f"    Fix: Add type annotation like (e: React.ChangeEvent<HTMLInputElement>)")
        if len(implicit_any_issues) > 10:
            print(f"\n  ... and {len(implicit_any_issues) - 10} more")
    else:
        print("\n  ✅ All event handlers properly typed")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    total_major = len(any_issues) + len(ignore_issues)
    total_minor = len(assertion_issues) + len(implicit_any_issues)

    print(f"  Major issues (any, @ts-ignore): {total_major}")
    print(f"  Minor issues (assertions, implicit any): {total_minor}")
    print(f"  Strict compliance: {strict_compliance:.1f}%")

    if strict_compliance >= 90:
        print("\n  ✅ Type coverage is GOOD (≥90%)")
    elif strict_compliance >= 80:
        print("\n  ⚠️  Type coverage is ACCEPTABLE (80-90%)")
    else:
        print("\n  ❌ Type coverage NEEDS IMPROVEMENT (<80%)")

    sys.exit(0 if total_major == 0 else 1)


if __name__ == "__main__":
    main()
