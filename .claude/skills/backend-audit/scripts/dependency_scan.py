#!/usr/bin/env python3
"""
Dependency Scanner

Parses .csproj files for PackageReferences.
Checks version consistency and outdated packages.
Flags security-sensitive packages.
"""

import os
import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path
from typing import Dict, List, Tuple


# Security-sensitive packages with notes
SECURITY_SENSITIVE = {
    "Stripe.net": "Payment processing - ensure webhook signature validation, secure API keys",
    "Microsoft.AspNetCore.Authentication.JwtBearer": "JWT auth - verify token validation settings, key rotation",
    "Microsoft.EntityFrameworkCore": "ORM - watch for SQL injection in raw queries, connection string security",
    "Microsoft.EntityFrameworkCore.SqlServer": "SQL Server - check connection string security",
    "Serilog": "Logging - ensure no sensitive data (PII, tokens) in logs",
    "Serilog.Sinks.Seq": "Log aggregation - secure Seq endpoint, no sensitive data",
    "MediatR": "CQRS - pipeline behaviors for auth/validation",
    "FluentValidation": "Validation - ensure comprehensive input validation",
    "Hangfire": "Background jobs - secure dashboard, tenant isolation in jobs",
    "BCrypt.Net-Next": "Password hashing - ensure proper work factor",
    "System.IdentityModel.Tokens.Jwt": "JWT handling - verify signature validation",
}


def find_csproj_files(root: Path) -> List[Path]:
    """Find all .csproj files."""
    return list(root.glob("**/*.csproj"))


def parse_package_references(csproj_path: Path) -> List[Dict]:
    """Parse PackageReferences from a .csproj file."""
    packages = []

    try:
        content = csproj_path.read_text(encoding="utf-8")
    except Exception:
        return packages

    # Pattern: <PackageReference Include="Name" Version="1.0.0" />
    pattern = re.compile(
        r'<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"'
    )

    for match in pattern.finditer(content):
        packages.append({
            "name": match.group(1),
            "version": match.group(2),
            "project": csproj_path.stem,
        })

    return packages


def check_version_consistency(all_packages: List[Dict]) -> List[Dict]:
    """Check if same package has different versions across projects."""
    package_versions = defaultdict(list)

    for pkg in all_packages:
        package_versions[pkg["name"]].append({
            "version": pkg["version"],
            "project": pkg["project"],
        })

    inconsistencies = []
    for name, versions in package_versions.items():
        unique_versions = set(v["version"] for v in versions)
        if len(unique_versions) > 1:
            inconsistencies.append({
                "package": name,
                "versions": versions,
            })

    return inconsistencies


def check_outdated_packages(root: Path) -> str:
    """Run dotnet list package --outdated."""
    sln_files = list(root.glob("*.sln"))
    if not sln_files:
        return "No solution file found"

    try:
        result = subprocess.run(
            ["dotnet", "list", str(sln_files[0]), "package", "--outdated"],
            capture_output=True,
            text=True,
            timeout=60,
            cwd=root
        )
        return result.stdout + result.stderr
    except subprocess.TimeoutExpired:
        return "Command timed out after 60 seconds"
    except FileNotFoundError:
        return "dotnet CLI not found"
    except Exception as e:
        return f"Error: {str(e)}"


def main():
    if len(sys.argv) < 2:
        print("Usage: dependency_scan.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()

    print("=" * 60)
    print("DEPENDENCY SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}\n")

    # Find and parse .csproj files
    csproj_files = find_csproj_files(root)
    print(f"Found {len(csproj_files)} .csproj files\n")

    all_packages = []
    packages_by_project = defaultdict(list)

    for csproj in csproj_files:
        packages = parse_package_references(csproj)
        all_packages.extend(packages)
        packages_by_project[csproj.stem] = packages

    # Print packages per project
    print("=" * 60)
    print("PACKAGES BY PROJECT")
    print("=" * 60)

    for project, packages in sorted(packages_by_project.items()):
        print(f"\n### {project} ({len(packages)} packages) ###")
        for pkg in sorted(packages, key=lambda x: x["name"]):
            security_note = ""
            if pkg["name"] in SECURITY_SENSITIVE:
                security_note = " ⚠️ SECURITY-SENSITIVE"
            print(f"  {pkg['name']} v{pkg['version']}{security_note}")

    # Version consistency check
    print("\n" + "=" * 60)
    print("VERSION CONSISTENCY CHECK")
    print("=" * 60)

    inconsistencies = check_version_consistency(all_packages)
    if inconsistencies:
        print(f"\n⚠️  Found {len(inconsistencies)} packages with inconsistent versions:\n")
        for inc in inconsistencies:
            print(f"  {inc['package']}:")
            for v in inc["versions"]:
                print(f"    {v['project']}: v{v['version']}")
    else:
        print("\n✅ All packages have consistent versions across projects")

    # Security-sensitive packages
    print("\n" + "=" * 60)
    print("SECURITY-SENSITIVE PACKAGES")
    print("=" * 60)

    found_sensitive = []
    for pkg in all_packages:
        if pkg["name"] in SECURITY_SENSITIVE:
            if pkg["name"] not in [p["name"] for p in found_sensitive]:
                found_sensitive.append(pkg)

    if found_sensitive:
        print("\n⚠️  The following security-sensitive packages require careful review:\n")
        for pkg in found_sensitive:
            print(f"  {pkg['name']} v{pkg['version']}")
            print(f"    Note: {SECURITY_SENSITIVE[pkg['name']]}\n")
    else:
        print("\n  No known security-sensitive packages found")

    # Outdated packages
    print("\n" + "=" * 60)
    print("OUTDATED PACKAGES CHECK")
    print("=" * 60)

    print("\nRunning: dotnet list package --outdated")
    print("(This may take a moment...)\n")

    outdated_output = check_outdated_packages(root)
    print(outdated_output)

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    unique_packages = set(pkg["name"] for pkg in all_packages)
    print(f"  Total projects: {len(packages_by_project)}")
    print(f"  Total package references: {len(all_packages)}")
    print(f"  Unique packages: {len(unique_packages)}")
    print(f"  Version inconsistencies: {len(inconsistencies)}")
    print(f"  Security-sensitive packages: {len(found_sensitive)}")

    sys.exit(0)


if __name__ == "__main__":
    main()
