#!/usr/bin/env python3
"""
Clean Architecture Boundary Checker

Checks that layer dependencies follow Clean Architecture rules:
- Domain: ZERO dependencies on other layers
- Application: Only Domain
- Infrastructure: Domain + Application
- API: Application (Infrastructure only for DI)
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List, Set, Tuple

LAYER_RULES = {
    "Orbito.Domain": {
        "allowed": [],
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
        "allowed": ["Orbito.Application", "Orbito.Infrastructure"],
        "forbidden": ["Orbito.Domain"],  # Exception: DI files can import Domain
    },
}

# Files that are allowed to break the rules (DI setup)
DI_EXCEPTION_PATTERNS = [
    r"DependencyInjection\.cs$",
    r"Program\.cs$",
    r"Startup\.cs$",
]

# Infra concerns that should NOT appear in Domain
INFRA_CONCERNS = [
    r"\[JsonProperty",
    r"\[JsonIgnore",
    r"\[Column\(",
    r"\[Table\(",
    r"\[Key\]",
    r"\[Required\]",
    r"\[MaxLength",
    r"\[ForeignKey",
    r"DbContext",
    r"ILogger<",
    r"Microsoft\.EntityFrameworkCore",
    r"System\.Text\.Json",
    r"Newtonsoft\.Json",
]

# Naming conventions
NAMING_CONVENTIONS = {
    "Orbito.API": {
        "Controllers": r".*Controller\.cs$",
    },
    "Orbito.Application": {
        "Commands": r".*Command\.cs$",
        "Queries": r".*Query\.cs$",
        "Handlers": r".*Handler\.cs$",
        "Validators": r".*Validator\.cs$",
    },
    "Orbito.Domain": {
        "Entities": r"^[A-Z][a-zA-Z]+\.cs$",
        "Interfaces": r"^I[A-Z][a-zA-Z]+\.cs$",
    },
    "Orbito.Infrastructure": {
        "Repositories": r".*Repository\.cs$",
        "Services": r".*Service\.cs$",
    },
}


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


def find_project_files(root: Path) -> Dict[str, List[Path]]:
    """Find all .cs files grouped by project."""
    projects = {}
    for layer in LAYER_RULES.keys():
        layer_path = root / layer
        if layer_path.exists():
            cs_files = []
            for f in layer_path.rglob("*.cs"):
                if "/bin/" not in str(f) and "/obj/" not in str(f) and "\\bin\\" not in str(f) and "\\obj\\" not in str(f):
                    cs_files.append(f)
            projects[layer] = cs_files
    return projects


def check_csproj_references(root: Path) -> List[Issue]:
    """Check .csproj files for forbidden ProjectReferences."""
    issues = []

    for layer, rules in LAYER_RULES.items():
        csproj_path = root / layer / f"{layer}.csproj"
        if not csproj_path.exists():
            continue

        content = csproj_path.read_text(encoding="utf-8")

        for forbidden in rules["forbidden"]:
            pattern = rf'<ProjectReference\s+Include="[^"]*{forbidden}[^"]*"'
            matches = re.finditer(pattern, content)
            for match in matches:
                line_num = content[:match.start()].count('\n') + 1
                issues.append(Issue(
                    "CRITICAL",
                    "Layer Boundary",
                    str(csproj_path.relative_to(root)),
                    line_num,
                    f"Forbidden reference from {layer} to {forbidden}",
                    f"Remove the ProjectReference to {forbidden}"
                ))

    return issues


def check_using_statements(root: Path, projects: Dict[str, List[Path]]) -> List[Issue]:
    """Check using statements for forbidden imports."""
    issues = []

    for layer, files in projects.items():
        rules = LAYER_RULES[layer]

        for file_path in files:
            # Check if this file is a DI exception
            is_di_file = any(re.search(pattern, str(file_path)) for pattern in DI_EXCEPTION_PATTERNS)

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            lines = content.split('\n')
            for line_num, line in enumerate(lines, 1):
                if not line.strip().startswith("using "):
                    continue

                for forbidden in rules["forbidden"]:
                    if forbidden in line:
                        # API->Domain exception for DI files
                        if layer == "Orbito.API" and forbidden == "Orbito.Domain" and is_di_file:
                            continue

                        issues.append(Issue(
                            "MAJOR",
                            "Layer Boundary",
                            str(file_path.relative_to(root)),
                            line_num,
                            f"Forbidden using: {layer} imports {forbidden}",
                            f"Remove the using statement or move code to appropriate layer"
                        ))

    return issues


def check_domain_purity(root: Path, domain_files: List[Path]) -> List[Issue]:
    """Check that Domain entities don't have infrastructure concerns."""
    issues = []

    for file_path in domain_files:
        try:
            content = file_path.read_text(encoding="utf-8")
        except Exception:
            continue

        lines = content.split('\n')
        for line_num, line in enumerate(lines, 1):
            for pattern in INFRA_CONCERNS:
                if re.search(pattern, line):
                    issues.append(Issue(
                        "MAJOR",
                        "Domain Purity",
                        str(file_path.relative_to(root)),
                        line_num,
                        f"Infrastructure concern in Domain: {pattern}",
                        "Move data annotations to EF configuration, remove logger from entity"
                    ))

    return issues


def check_naming_conventions(root: Path, projects: Dict[str, List[Path]]) -> List[Issue]:
    """Check naming conventions per layer."""
    issues = []

    for layer, conventions in NAMING_CONVENTIONS.items():
        if layer not in projects:
            continue

        for file_path in projects[layer]:
            filename = file_path.name
            parent_dir = file_path.parent.name

            # Check if file is in a conventional folder but doesn't follow naming
            for folder_name, pattern in conventions.items():
                if folder_name.lower() in str(file_path).lower():
                    if not re.match(pattern, filename):
                        issues.append(Issue(
                            "MINOR",
                            "Naming Convention",
                            str(file_path.relative_to(root)),
                            1,
                            f"File in {folder_name} folder doesn't follow naming convention: {pattern}",
                            f"Rename to match pattern: {pattern}"
                        ))

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: architecture_check.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()

    print("=" * 60)
    print("CLEAN ARCHITECTURE BOUNDARY CHECK")
    print("=" * 60)
    print(f"Scanning: {root}\n")

    # Find all project files
    projects = find_project_files(root)

    for layer, files in projects.items():
        print(f"  {layer}: {len(files)} files")
    print()

    all_issues: List[Issue] = []

    # Check 1: .csproj references
    print("Checking .csproj ProjectReferences...")
    all_issues.extend(check_csproj_references(root))

    # Check 2: using statements
    print("Checking using statements...")
    all_issues.extend(check_using_statements(root, projects))

    # Check 3: Domain purity
    print("Checking Domain purity...")
    if "Orbito.Domain" in projects:
        all_issues.extend(check_domain_purity(root, projects["Orbito.Domain"]))

    # Check 4: Naming conventions
    print("Checking naming conventions...")
    all_issues.extend(check_naming_conventions(root, projects))

    print("\n" + "=" * 60)
    print("RESULTS")
    print("=" * 60)

    # Group by severity
    critical = [i for i in all_issues if i.severity == "CRITICAL"]
    major = [i for i in all_issues if i.severity == "MAJOR"]
    minor = [i for i in all_issues if i.severity == "MINOR"]

    print(f"\nSummary: {len(critical)} CRITICAL, {len(major)} MAJOR, {len(minor)} MINOR\n")

    if critical:
        print("\n### CRITICAL ISSUES ###")
        for issue in critical:
            print(f"\n{issue}")

    if major:
        print("\n### MAJOR ISSUES ###")
        for issue in major:
            print(f"\n{issue}")

    if minor:
        print("\n### MINOR ISSUES ###")
        for issue in minor:
            print(f"\n{issue}")

    if not all_issues:
        print("\n✅ No architecture violations found!")

    # Exit code
    if critical:
        sys.exit(2)
    elif major:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == "__main__":
    main()
