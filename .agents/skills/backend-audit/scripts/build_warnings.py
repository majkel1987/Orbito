#!/usr/bin/env python3
"""
Build Warnings Analyzer — Runs dotnet build and categorizes warnings.

Usage:
    python3 build_warnings.py <solution-root>
"""

import sys
import subprocess
import re
from pathlib import Path
from collections import defaultdict


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 build_warnings.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1])
    sln_files = list(root.glob("*.sln"))

    print("=" * 70)
    print("BUILD WARNINGS ANALYSIS")
    print("=" * 70)

    # Run dotnet build
    cmd = ["dotnet", "build", "--no-incremental", "-v", "q"]
    if sln_files:
        cmd.append(str(sln_files[0]))
    else:
        cmd.append(str(root))

    print(f"\nRunning: {' '.join(cmd)}")
    result = subprocess.run(cmd, capture_output=True, text=True, cwd=str(root))

    output = result.stdout + result.stderr

    # Parse warnings
    warnings = re.findall(r'(\S+\.cs)\((\d+),\d+\):\s*warning\s+(CS\d+):\s*(.+)', output)
    errors = re.findall(r'(\S+\.cs)\((\d+),\d+\):\s*error\s+(CS\d+):\s*(.+)', output)

    # Categorize warnings
    by_code = defaultdict(list)
    for file, line, code, msg in warnings:
        by_code[code].append({"file": file, "line": line, "message": msg})

    if errors:
        print(f"\n🔴 BUILD ERRORS ({len(errors)}):")
        print(f"{'─' * 70}")
        for file, line, code, msg in errors:
            print(f"  {code}: {msg}")
            print(f"    {file}:{line}")

    if warnings:
        print(f"\n⚠️  BUILD WARNINGS ({len(warnings)}):")
        print(f"{'─' * 70}")

        for code, items in sorted(by_code.items(), key=lambda x: -len(x[1])):
            print(f"\n  {code} ({len(items)}x): {items[0]['message'][:80]}")
            for item in items[:3]:
                print(f"    {item['file']}:{item['line']}")
            if len(items) > 3:
                print(f"    ... and {len(items) - 3} more")
    else:
        print("\n✅ Build completed with 0 warnings")

    # Build result
    print(f"\n{'=' * 70}")
    print("SUMMARY")
    print(f"{'=' * 70}")
    print(f"  Build: {'FAILED' if result.returncode != 0 else 'SUCCESS'}")
    print(f"  Errors:   {len(errors)}")
    print(f"  Warnings: {len(warnings)}")
    print(f"  Unique warning codes: {len(by_code)}")

    sys.exit(0 if result.returncode == 0 else 2)


if __name__ == "__main__":
    main()
