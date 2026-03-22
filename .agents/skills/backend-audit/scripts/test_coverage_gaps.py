#!/usr/bin/env python3
"""
Test Coverage Gap Detector — Finds handlers, services, and entities without test files.

Scans Application and Domain layers for classes that should have tests,
then checks if corresponding test files exist.

Usage:
    python3 test_coverage_gaps.py <solution-root>
"""

import sys
import re
from pathlib import Path
from collections import defaultdict


def find_testable_classes(root):
    """Find all classes that should have tests."""
    testables = []
    app_path = root / "Orbito.Application"
    domain_path = root / "Orbito.Domain"
    infra_path = root / "Orbito.Infrastructure"

    # Command/Query Handlers
    for handler_file in app_path.rglob("*Handler.cs"):
        if "bin" in str(handler_file) or "obj" in str(handler_file):
            continue
        testables.append({
            "name": handler_file.stem,
            "path": str(handler_file),
            "category": "Handler",
            "priority": "HIGH",
        })

    # Validators
    for validator_file in app_path.rglob("*Validator.cs"):
        if "bin" in str(validator_file) or "obj" in str(validator_file):
            continue
        testables.append({
            "name": validator_file.stem,
            "path": str(validator_file),
            "category": "Validator",
            "priority": "MEDIUM",
        })

    # Application Services
    for service_file in app_path.rglob("*Service.cs"):
        if "bin" in str(service_file) or "obj" in str(service_file):
            continue
        if "Interface" in str(service_file) or service_file.stem.startswith("I"):
            continue
        testables.append({
            "name": service_file.stem,
            "path": str(service_file),
            "category": "Service",
            "priority": "HIGH",
        })

    # Background Jobs
    for job_file in list(app_path.rglob("*Job.cs")) + list(infra_path.rglob("*Job.cs")):
        if "bin" in str(job_file) or "obj" in str(job_file):
            continue
        testables.append({
            "name": job_file.stem,
            "path": str(job_file),
            "category": "BackgroundJob",
            "priority": "HIGH",
        })

    # Domain Entities
    entities_path = domain_path / "Entities"
    if entities_path.exists():
        for entity_file in entities_path.rglob("*.cs"):
            if "bin" in str(entity_file) or "obj" in str(entity_file):
                continue
            testables.append({
                "name": entity_file.stem,
                "path": str(entity_file),
                "category": "Entity",
                "priority": "MEDIUM",
            })

    # Infrastructure Repositories
    for repo_file in infra_path.rglob("*Repository.cs"):
        if "bin" in str(repo_file) or "obj" in str(repo_file):
            continue
        if repo_file.stem.startswith("I"):
            continue
        testables.append({
            "name": repo_file.stem,
            "path": str(repo_file),
            "category": "Repository",
            "priority": "LOW",
        })

    return testables


def find_test_files(root):
    """Find all test files and extract what they test."""
    test_path = root / "Orbito.Tests"
    if not test_path.exists():
        return set()

    test_names = set()
    for test_file in test_path.rglob("*Tests.cs"):
        # Extract the base name: FooBarTests.cs -> FooBar
        base = test_file.stem.replace("Tests", "").replace("Test", "")
        test_names.add(base)

    # Also check for test files with slightly different naming
    for test_file in test_path.rglob("*Test.cs"):
        base = test_file.stem.replace("Tests", "").replace("Test", "")
        test_names.add(base)

    return test_names


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 test_coverage_gaps.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1])

    print("=" * 70)
    print("TEST COVERAGE GAP ANALYSIS")
    print("=" * 70)

    testables = find_testable_classes(root)
    test_names = find_test_files(root)

    print(f"\nTestable classes found: {len(testables)}")
    print(f"Test files found: {len(test_names)}")

    # Check coverage
    covered = []
    gaps = []

    for item in testables:
        # Check various naming patterns
        name = item["name"]
        is_covered = (
            name in test_names or
            name.replace("Handler", "") in test_names or
            name.replace("CommandHandler", "") in test_names or
            name.replace("QueryHandler", "") in test_names or
            f"{name}Handler" in test_names or
            any(name in tn for tn in test_names)
        )

        if is_covered:
            covered.append(item)
        else:
            gaps.append(item)

    # Report gaps by category
    if gaps:
        print(f"\n{'─' * 70}")
        print(f"MISSING TESTS ({len(gaps)})")
        print(f"{'─' * 70}")

        by_category = defaultdict(list)
        for gap in gaps:
            by_category[gap["category"]].append(gap)

        for category in ["Handler", "Validator", "Service", "BackgroundJob", "Entity", "Repository"]:
            items = by_category.get(category, [])
            if items:
                high = [i for i in items if i["priority"] == "HIGH"]
                medium = [i for i in items if i["priority"] == "MEDIUM"]
                low = [i for i in items if i["priority"] == "LOW"]

                print(f"\n  {category} ({len(items)} missing):")
                for item in sorted(items, key=lambda x: x["name"]):
                    priority_icon = {"HIGH": "🔴", "MEDIUM": "🟡", "LOW": "🟢"}[item["priority"]]
                    print(f"    {priority_icon} {item['name']}")
                    print(f"       {item['path']}")

    # Report covered
    print(f"\n{'─' * 70}")
    print(f"COVERED ({len(covered)})")
    print(f"{'─' * 70}")
    by_cat = defaultdict(int)
    for item in covered:
        by_cat[item["category"]] += 1
    for cat, count in sorted(by_cat.items()):
        print(f"  {cat}: {count}")

    # Summary
    total = len(testables)
    covered_count = len(covered)
    gap_count = len(gaps)
    coverage_pct = (covered_count / total * 100) if total > 0 else 0

    print(f"\n{'=' * 70}")
    print("SUMMARY")
    print(f"{'=' * 70}")
    print(f"  Total testable classes: {total}")
    print(f"  With tests:            {covered_count} ({coverage_pct:.0f}%)")
    print(f"  Missing tests:         {gap_count}")
    print(f"  High priority gaps:    {len([g for g in gaps if g['priority'] == 'HIGH'])}")

    if gap_count > 0:
        sys.exit(1)
    else:
        print("\n  ✅ All testable classes have corresponding test files.")
        sys.exit(0)


if __name__ == "__main__":
    main()
