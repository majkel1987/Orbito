#!/usr/bin/env python3
"""
Architecture Layer Dependency Checker — Enforces Clean Architecture boundaries.

Validates that:
- Domain layer has ZERO dependencies on other layers
- Application layer depends only on Domain
- Infrastructure depends on Application + Domain
- API depends on Application (not Infrastructure directly, except DI)

Usage:
    python3 architecture_check.py <solution-root>
"""

import sys
import re
from pathlib import Path
from collections import defaultdict


LAYER_RULES = {
    "Orbito.Domain": {
        "allowed": [],  # Domain depends on NOTHING
        "forbidden": ["Orbito.Application", "Orbito.Infrastructure", "Orbito.API"],
    },
    "Orbito.Application": {
        "allowed": ["Orbito.Domain"],
        "forbidden": ["Orbito.Infrastructure", "Orbito.API"],
    },
    "Orbito.Infrastructure": {
        "allowed": ["Orbito.Domain", "Orbito.Application"],
        "forbidden": ["Orbito.API"],
    },
    "Orbito.API": {
        "allowed": ["Orbito.Application", "Orbito.Infrastructure"],  # Infra only for DI
        "forbidden": ["Orbito.Domain"],  # Should go through Application
    },
}

NAMING_CONVENTIONS = {
    "Controllers": r"^\w+Controller\.cs$",
    "Commands": r"^\w+Command(Handler|Validator)?\.cs$",
    "Queries": r"^\w+Query(Handler)?\.cs$",
    "Entities": r"^[A-Z]\w+\.cs$",  # PascalCase, no prefix
    "Interfaces": r"^I[A-Z]\w+\.cs$",  # I-prefix
    "DTOs": r"^\w+Dto\.cs$",
    "Services": r"^\w+Service\.cs$",
    "Repositories": r"^\w+Repository\.cs$",
}


def check_project_references(root):
    """Check .csproj files for illegal layer references."""
    issues = []

    for project_name, rules in LAYER_RULES.items():
        csproj = root / project_name / f"{project_name}.csproj"
        if not csproj.exists():
            continue

        content = csproj.read_text(encoding="utf-8", errors="ignore")
        references = re.findall(r'<ProjectReference\s+Include="[^"]*\\(\w+)\.csproj"', content)

        for ref in references:
            if ref in rules["forbidden"]:
                issues.append({
                    "severity": "CRITICAL",
                    "message": f"{project_name} references {ref} — violates Clean Architecture",
                    "file": str(csproj),
                    "category": "Layer violation",
                })

    return issues


def check_using_statements(root):
    """Check using statements for cross-layer imports."""
    issues = []

    for project_name, rules in LAYER_RULES.items():
        project_path = root / project_name
        if not project_path.exists():
            continue

        cs_files = [f for f in project_path.rglob("*.cs") if "bin" not in str(f) and "obj" not in str(f)]

        for filepath in cs_files:
            content = filepath.read_text(encoding="utf-8", errors="ignore")

            for forbidden in rules["forbidden"]:
                pattern = f"using\\s+{forbidden.replace('.', '\\.')}"
                matches = re.findall(pattern, content)
                if matches:
                    # Special case: API can reference Infrastructure for DI registration
                    if project_name == "Orbito.API" and forbidden == "Orbito.Domain":
                        if "DependencyInjection" in filepath.stem or "Program" in filepath.stem:
                            continue

                    issues.append({
                        "severity": "MAJOR",
                        "message": f"{filepath.name} in {project_name} imports from {forbidden}",
                        "file": str(filepath),
                        "category": "Using violation",
                    })

    return issues


def check_naming_conventions(root):
    """Check file naming conventions per folder."""
    issues = []

    for project_name in LAYER_RULES:
        project_path = root / project_name
        if not project_path.exists():
            continue

        cs_files = [f for f in project_path.rglob("*.cs") if "bin" not in str(f) and "obj" not in str(f)]

        for filepath in cs_files:
            parent = filepath.parent.name

            if parent in NAMING_CONVENTIONS:
                pattern = NAMING_CONVENTIONS[parent]
                if not re.match(pattern, filepath.name):
                    issues.append({
                        "severity": "MINOR",
                        "message": f"File '{filepath.name}' in {parent}/ doesn't follow naming convention: {pattern}",
                        "file": str(filepath),
                        "category": "Naming convention",
                    })

    return issues


def check_entity_purity(root):
    """Check that Domain entities don't reference infrastructure concerns."""
    issues = []
    entities_path = root / "Orbito.Domain" / "Entities"
    if not entities_path.exists():
        return issues

    infra_patterns = [
        (r'\[Table\(', "EF Core [Table] attribute in domain entity"),
        (r'\[Column\(', "EF Core [Column] attribute in domain entity"),
        (r'\[JsonProperty', "JSON attribute in domain entity"),
        (r'\[BsonElement', "MongoDB attribute in domain entity"),
        (r'DbContext', "DbContext reference in domain entity"),
        (r'ILogger', "ILogger in domain entity — use domain events instead"),
    ]

    for filepath in entities_path.rglob("*.cs"):
        content = filepath.read_text(encoding="utf-8", errors="ignore")
        for pattern, message in infra_patterns:
            if re.search(pattern, content):
                issues.append({
                    "severity": "MAJOR",
                    "message": f"{filepath.name}: {message}",
                    "file": str(filepath),
                    "category": "Domain purity",
                })

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 architecture_check.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1])

    print("=" * 70)
    print("ARCHITECTURE DEPENDENCY CHECK")
    print("=" * 70)

    all_issues = []

    print("\n1. Checking project references...")
    all_issues.extend(check_project_references(root))

    print("2. Checking using statements...")
    all_issues.extend(check_using_statements(root))

    print("3. Checking naming conventions...")
    all_issues.extend(check_naming_conventions(root))

    print("4. Checking domain entity purity...")
    all_issues.extend(check_entity_purity(root))

    if not all_issues:
        print("\n✅ No architecture violations detected.")
        sys.exit(0)

    # Report
    by_severity = defaultdict(list)
    for issue in all_issues:
        by_severity[issue["severity"]].append(issue)

    for severity in ["CRITICAL", "MAJOR", "MINOR"]:
        group = by_severity.get(severity, [])
        if group:
            print(f"\n{'─' * 70}")
            print(f"{severity} ({len(group)})")
            print(f"{'─' * 70}")
            for issue in group:
                print(f"  [{issue['category']}] {issue['message']}")
                print(f"    File: {issue['file']}")

    print(f"\n{'=' * 70}")
    print("SUMMARY")
    print(f"{'=' * 70}")
    for s in ["CRITICAL", "MAJOR", "MINOR"]:
        print(f"  {s}: {len(by_severity.get(s, []))}")

    if by_severity.get("CRITICAL"):
        sys.exit(2)
    elif by_severity.get("MAJOR"):
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()
