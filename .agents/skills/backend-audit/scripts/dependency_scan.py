#!/usr/bin/env python3
"""
NuGet Dependency Scanner — Checks for outdated and potentially vulnerable packages.

Usage:
    python3 dependency_scan.py <solution-root>
"""

import sys
import subprocess
import re
from pathlib import Path


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 dependency_scan.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1])

    print("=" * 70)
    print("NUGET DEPENDENCY SCAN")
    print("=" * 70)

    # Find all csproj files
    csproj_files = [f for f in root.rglob("*.csproj") if "bin" not in str(f) and "obj" not in str(f)]

    all_packages = {}

    for csproj in csproj_files:
        project_name = csproj.stem
        content = csproj.read_text(encoding="utf-8", errors="ignore")

        packages = re.findall(
            r'<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"',
            content
        )

        if packages:
            print(f"\n{project_name}:")
            for name, version in packages:
                print(f"  {name}: {version}")
                if name not in all_packages:
                    all_packages[name] = []
                all_packages[name].append((project_name, version))

    # Check for version inconsistencies
    print(f"\n{'─' * 70}")
    print("VERSION CONSISTENCY CHECK")
    print(f"{'─' * 70}")

    inconsistencies = []
    for pkg, usages in all_packages.items():
        versions = set(v for _, v in usages)
        if len(versions) > 1:
            inconsistencies.append((pkg, usages))
            print(f"\n  ⚠️  {pkg} — multiple versions:")
            for project, version in usages:
                print(f"    {project}: {version}")

    if not inconsistencies:
        print("  ✅ All packages use consistent versions across projects")

    # Try dotnet list outdated
    print(f"\n{'─' * 70}")
    print("OUTDATED PACKAGES CHECK")
    print(f"{'─' * 70}")

    sln_files = list(root.glob("*.sln"))
    if sln_files:
        try:
            result = subprocess.run(
                ["dotnet", "list", str(sln_files[0]), "package", "--outdated"],
                capture_output=True, text=True, cwd=str(root), timeout=60
            )
            if result.stdout:
                # Parse outdated output
                outdated = re.findall(
                    r'>\s+(\S+)\s+(\S+)\s+\S+\s+(\S+)',
                    result.stdout
                )
                if outdated:
                    for name, current, latest in outdated:
                        severity = "MAJOR" if "Security" in name.lower() or int(latest.split(".")[0]) > int(current.split(".")[0]) else "MINOR"
                        print(f"  [{severity}] {name}: {current} → {latest}")
                else:
                    print("  ✅ All packages are up to date")
            else:
                print("  ℹ️  Could not check — run manually: dotnet list package --outdated")
        except (subprocess.TimeoutExpired, FileNotFoundError):
            print("  ℹ️  dotnet CLI not available — manual check needed")
    else:
        print("  ℹ️  No .sln file found — manual check needed")

    # Known security-sensitive packages
    print(f"\n{'─' * 70}")
    print("SECURITY-SENSITIVE PACKAGES")
    print(f"{'─' * 70}")

    security_packages = {
        "Stripe.net": "Payment processing — ensure latest for PCI compliance",
        "Microsoft.AspNetCore.Authentication.JwtBearer": "Auth — keep updated for security patches",
        "Microsoft.EntityFrameworkCore": "ORM — check for SQL injection patches",
        "Serilog": "Logging — ensure no sensitive data serialization bugs",
    }

    for pkg_prefix, note in security_packages.items():
        found = [(name, usages) for name, usages in all_packages.items() if name.startswith(pkg_prefix)]
        for name, usages in found:
            version = usages[0][1]
            print(f"  {name} v{version}")
            print(f"    ℹ️  {note}")

    # Summary
    print(f"\n{'=' * 70}")
    print("SUMMARY")
    print(f"{'=' * 70}")
    print(f"  Total unique packages: {len(all_packages)}")
    print(f"  Version inconsistencies: {len(inconsistencies)}")
    print(f"  Projects scanned: {len(csproj_files)}")


if __name__ == "__main__":
    main()
