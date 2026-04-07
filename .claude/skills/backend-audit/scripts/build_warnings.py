#!/usr/bin/env python3
"""
Build Warnings Scanner

Runs dotnet build and parses warnings/errors from output.
Groups warnings by code and shows top occurrences.
"""

import os
import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path
from typing import Dict, List, Tuple


def find_solution(root: Path) -> Path:
    """Find .sln file in root directory."""
    sln_files = list(root.glob("*.sln"))
    if not sln_files:
        print(f"ERROR: No .sln file found in {root}")
        sys.exit(1)
    return sln_files[0]


def run_build(solution_path: Path) -> Tuple[str, str, int]:
    """Run dotnet build and capture output."""
    cmd = ["dotnet", "build", str(solution_path), "--no-incremental", "-v", "q"]

    try:
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=300,  # 5 minute timeout
            cwd=solution_path.parent
        )
        return result.stdout, result.stderr, result.returncode
    except subprocess.TimeoutExpired:
        return "", "Build timed out after 5 minutes", 1
    except FileNotFoundError:
        return "", "dotnet CLI not found. Please install .NET SDK.", 1


def parse_warnings(output: str) -> Dict[str, List[Dict]]:
    """Parse warnings from build output."""
    # Pattern: path\to\file.cs(line,col): warning CS1234: message
    warning_pattern = re.compile(
        r'([^\s]+\.cs)\((\d+),\d+\):\s*warning\s+(CS\d+):\s*(.+)'
    )

    warnings = defaultdict(list)

    for line in output.split('\n'):
        match = warning_pattern.search(line)
        if match:
            file_path, line_num, code, message = match.groups()
            warnings[code].append({
                "file": file_path,
                "line": int(line_num),
                "message": message.strip()
            })

    return warnings


def parse_errors(output: str) -> List[Dict]:
    """Parse errors from build output."""
    # Pattern: path\to\file.cs(line,col): error CS1234: message
    error_pattern = re.compile(
        r'([^\s]+\.cs)\((\d+),\d+\):\s*error\s+(CS\d+):\s*(.+)'
    )

    errors = []

    for line in output.split('\n'):
        match = error_pattern.search(line)
        if match:
            file_path, line_num, code, message = match.groups()
            errors.append({
                "file": file_path,
                "line": int(line_num),
                "code": code,
                "message": message.strip()
            })

    return errors


def main():
    if len(sys.argv) < 2:
        print("Usage: build_warnings.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()
    solution = find_solution(root)

    print("=" * 60)
    print("BUILD WARNINGS SCANNER")
    print("=" * 60)
    print(f"Solution: {solution.name}")
    print(f"Running: dotnet build --no-incremental -v q\n")

    stdout, stderr, returncode = run_build(solution)

    # Combine outputs
    full_output = stdout + "\n" + stderr

    # Parse warnings and errors
    warnings = parse_warnings(full_output)
    errors = parse_errors(full_output)

    print("=" * 60)
    print("RESULTS")
    print("=" * 60)

    # Build status
    if returncode == 0:
        print("\n✅ BUILD SUCCESS")
    else:
        print("\n❌ BUILD FAILED")

    # Errors
    print(f"\nErrors: {len(errors)}")
    if errors:
        print("\n### ERRORS ###")
        for err in errors[:10]:  # Show first 10
            print(f"  [{err['code']}] {err['file']}:{err['line']}")
            print(f"    {err['message']}")
        if len(errors) > 10:
            print(f"  ... and {len(errors) - 10} more errors")

    # Warnings summary
    total_warnings = sum(len(w) for w in warnings.values())
    print(f"\nWarnings: {total_warnings} ({len(warnings)} unique codes)")

    if warnings:
        print("\n### WARNINGS BY CODE ###")
        # Sort by count descending
        sorted_warnings = sorted(warnings.items(), key=lambda x: len(x[1]), reverse=True)

        for code, occurrences in sorted_warnings[:15]:  # Top 15 codes
            print(f"\n  {code}: {len(occurrences)} occurrences")
            # Show first 3 examples
            for occ in occurrences[:3]:
                print(f"    - {occ['file']}:{occ['line']}")
                print(f"      {occ['message'][:80]}...")
            if len(occurrences) > 3:
                print(f"    ... and {len(occurrences) - 3} more")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)
    print(f"  Build: {'SUCCESS' if returncode == 0 else 'FAILED'}")
    print(f"  Errors: {len(errors)}")
    print(f"  Warnings: {total_warnings}")
    print(f"  Unique warning codes: {len(warnings)}")

    # Most common warnings
    if warnings:
        print("\n  Top 5 warning codes:")
        for code, occurrences in sorted(warnings.items(), key=lambda x: len(x[1]), reverse=True)[:5]:
            print(f"    {code}: {len(occurrences)}")

    sys.exit(0 if returncode == 0 else 1)


if __name__ == "__main__":
    main()
