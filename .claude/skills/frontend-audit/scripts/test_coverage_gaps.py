#!/usr/bin/env python3
"""
Test Coverage Gaps Scanner

Analyzes which components/hooks/utils lack tests:
- Components without corresponding .test.tsx files
- Hooks without tests
- Utils without tests
- Test-to-code ratio
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List, Set, Tuple


# Directories to scan
SCAN_DIRS = [
    "features",
    "shared/hooks",
    "shared/lib",
    "shared/components",
    "core",
]

# Directories to skip
SKIP_DIRS = [
    "generated",
    "ui",  # shadcn/ui components don't need custom tests
    "__tests__",
    "test",
]


def find_source_files(src: Path) -> List[Tuple[Path, str]]:
    """Find all source files that should have tests."""
    source_files = []

    for ext in ["*.tsx", "*.ts"]:
        for file_path in src.rglob(ext):
            rel_path = str(file_path.relative_to(src))

            # Skip if in skip directories
            if any(skip in rel_path for skip in SKIP_DIRS):
                continue

            # Skip test files
            if ".test." in rel_path or ".spec." in rel_path:
                continue

            # Skip type definition files
            if rel_path.endswith(".d.ts"):
                continue

            # Skip index files (re-exports)
            if file_path.name == "index.ts" or file_path.name == "index.tsx":
                continue

            # Categorize file
            if "/hooks/" in rel_path or rel_path.startswith("hooks/"):
                category = "hook"
            elif "/components/" in rel_path or file_path.suffix == ".tsx":
                category = "component"
            elif "/lib/" in rel_path or "/utils/" in rel_path:
                category = "util"
            else:
                category = "other"

            source_files.append((file_path, category))

    return source_files


def find_test_files(src: Path) -> Set[str]:
    """Find all test files and extract what they test."""
    test_files = set()

    for ext in ["*.test.ts", "*.test.tsx", "*.spec.ts", "*.spec.tsx"]:
        for file_path in src.rglob(ext):
            # Extract the base name (without .test/.spec)
            name = file_path.stem
            name = name.replace(".test", "").replace(".spec", "")
            test_files.add(name)

    return test_files


def check_test_exists(source_path: Path, test_files: Set[str]) -> bool:
    """Check if a test file exists for the source file."""
    base_name = source_path.stem
    return base_name in test_files or base_name.lower() in {t.lower() for t in test_files}


def analyze_coverage(root: Path) -> Dict:
    """Analyze test coverage statistics."""
    src = root / "src"
    stats = {
        "total_source_files": 0,
        "total_test_files": 0,
        "files_with_tests": 0,
        "files_without_tests": 0,
        "coverage_by_category": {
            "component": {"total": 0, "tested": 0},
            "hook": {"total": 0, "tested": 0},
            "util": {"total": 0, "tested": 0},
            "other": {"total": 0, "tested": 0},
        },
        "missing_tests": [],
    }

    source_files = find_source_files(src)
    test_files = find_test_files(src)

    stats["total_source_files"] = len(source_files)
    stats["total_test_files"] = len(test_files)

    for source_path, category in source_files:
        stats["coverage_by_category"][category]["total"] += 1

        if check_test_exists(source_path, test_files):
            stats["files_with_tests"] += 1
            stats["coverage_by_category"][category]["tested"] += 1
        else:
            stats["files_without_tests"] += 1
            stats["missing_tests"].append({
                "file": str(source_path.relative_to(root)),
                "category": category,
            })

    return stats


def prioritize_missing_tests(missing: List[Dict]) -> List[Dict]:
    """Prioritize which files need tests most urgently."""
    priority_order = {
        "hook": 1,  # Hooks are most important to test
        "util": 2,  # Utils are also important
        "component": 3,  # Components
        "other": 4,
    }

    # Sort by priority
    missing.sort(key=lambda x: (priority_order.get(x["category"], 5), x["file"]))

    # Add priority label
    for item in missing:
        if item["category"] == "hook":
            item["priority"] = "HIGH"
        elif item["category"] == "util":
            item["priority"] = "HIGH"
        elif "guard" in item["file"].lower() or "auth" in item["file"].lower():
            item["priority"] = "HIGH"
        else:
            item["priority"] = "MEDIUM"

    return missing


def main():
    if len(sys.argv) < 2:
        print("Usage: test_coverage_gaps.py <frontend-root>")
        print("Example: test_coverage_gaps.py orbito-frontend/")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()
    src = root / "src"

    if not src.exists():
        print(f"Error: src/ directory not found in {root}")
        sys.exit(1)

    print("=" * 60)
    print("TEST COVERAGE GAPS SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}\n")

    # Analyze coverage
    stats = analyze_coverage(root)

    # Overall statistics
    print("=" * 60)
    print("COVERAGE STATISTICS")
    print("=" * 60)

    coverage_pct = stats["files_with_tests"] / max(1, stats["total_source_files"]) * 100

    print(f"  Total source files: {stats['total_source_files']}")
    print(f"  Total test files: {stats['total_test_files']}")
    print(f"  Files with tests: {stats['files_with_tests']}")
    print(f"  Files without tests: {stats['files_without_tests']}")
    print(f"\n  Overall coverage: {coverage_pct:.1f}%")

    # Coverage by category
    print("\n" + "=" * 60)
    print("COVERAGE BY CATEGORY")
    print("=" * 60)

    for category, data in stats["coverage_by_category"].items():
        if data["total"] > 0:
            pct = data["tested"] / data["total"] * 100
            print(f"  {category.capitalize()}: {data['tested']}/{data['total']} ({pct:.1f}%)")

    # Missing tests
    print("\n" + "=" * 60)
    print("FILES MISSING TESTS")
    print("=" * 60)

    missing = prioritize_missing_tests(stats["missing_tests"])

    if missing:
        # Group by priority
        high_priority = [m for m in missing if m["priority"] == "HIGH"]
        medium_priority = [m for m in missing if m["priority"] == "MEDIUM"]

        print(f"\n  HIGH PRIORITY ({len(high_priority)} files):")
        for item in high_priority[:15]:
            print(f"    [{item['category']}] {item['file']}")
        if len(high_priority) > 15:
            print(f"    ... and {len(high_priority) - 15} more")

        print(f"\n  MEDIUM PRIORITY ({len(medium_priority)} files):")
        for item in medium_priority[:15]:
            print(f"    [{item['category']}] {item['file']}")
        if len(medium_priority) > 15:
            print(f"    ... and {len(medium_priority) - 15} more")
    else:
        print("\n  ✅ All files have tests!")

    # Recommendations
    print("\n" + "=" * 60)
    print("RECOMMENDATIONS")
    print("=" * 60)

    hooks_coverage = stats["coverage_by_category"]["hook"]
    if hooks_coverage["total"] > 0:
        hooks_pct = hooks_coverage["tested"] / hooks_coverage["total"] * 100
        if hooks_pct < 80:
            print(f"\n  ⚠️  Hooks coverage is low ({hooks_pct:.0f}%)")
            print("     Hooks are critical for app logic - prioritize testing them")

    utils_coverage = stats["coverage_by_category"]["util"]
    if utils_coverage["total"] > 0:
        utils_pct = utils_coverage["tested"] / utils_coverage["total"] * 100
        if utils_pct < 90:
            print(f"\n  ⚠️  Utils coverage is low ({utils_pct:.0f}%)")
            print("     Utils should have 90%+ coverage as they're reused")

    if coverage_pct < 60:
        print("\n  ❌ Overall coverage is LOW (<60%)")
        print("     Focus on testing critical paths first:")
        print("     - Auth guards (TenantGuard, PortalGuard)")
        print("     - Custom hooks")
        print("     - Form validation utilities")
    elif coverage_pct < 80:
        print("\n  ⚠️  Coverage is ACCEPTABLE but could improve")
    else:
        print("\n  ✅ Coverage is GOOD (≥80%)")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    print(f"  Overall test coverage: {coverage_pct:.1f}%")
    print(f"  Files needing tests: {len(missing)}")
    print(f"  High priority: {len(high_priority)}")

    sys.exit(0 if coverage_pct >= 60 else 1)


if __name__ == "__main__":
    main()
