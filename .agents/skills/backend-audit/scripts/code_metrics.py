#!/usr/bin/env python3
"""
Code Metrics Scanner — Project-wide health overview.

Provides file counts, line counts, largest files, and complexity indicators
to prioritize audit effort.

Usage:
    python3 code_metrics.py <solution-root>
"""

import sys
import re
from pathlib import Path
from collections import defaultdict


def count_lines(filepath):
    """Count lines in a file."""
    try:
        return len(filepath.read_text(encoding="utf-8", errors="ignore").split("\n"))
    except Exception:
        return 0


def analyze_complexity(filepath):
    """Quick complexity heuristics for a C# file."""
    try:
        content = filepath.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        return {}

    return {
        "lines": len(content.split("\n")),
        "methods": len(re.findall(r'(?:public|private|protected|internal)\s+(?:async\s+)?(?:Task|void|string|int|bool|Result|IActionResult)', content)),
        "dependencies": len(re.findall(r'private\s+(?:readonly\s+)?I\w+', content)),
        "try_catches": len(re.findall(r'\bcatch\s*\(', content)),
        "todos": len(re.findall(r'TODO|FIXME|HACK|XXX|TEMP', content, re.IGNORECASE)),
    }


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 code_metrics.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1])
    projects = ["Orbito.API", "Orbito.Application", "Orbito.Domain", "Orbito.Infrastructure", "Orbito.Tests"]

    print("=" * 70)
    print("CODE METRICS OVERVIEW")
    print("=" * 70)

    total_files = 0
    total_lines = 0
    all_files_data = []
    todos_found = []

    for project in projects:
        project_path = root / project
        if not project_path.exists():
            continue

        cs_files = [f for f in project_path.rglob("*.cs") if "bin" not in str(f) and "obj" not in str(f)]
        project_lines = sum(count_lines(f) for f in cs_files)
        total_files += len(cs_files)
        total_lines += project_lines

        print(f"\n{project}:")
        print(f"  Files: {len(cs_files)}")
        print(f"  Lines: {project_lines:,}")
        print(f"  Avg lines/file: {project_lines // max(len(cs_files), 1)}")

        # Find largest files
        file_sizes = [(f, count_lines(f)) for f in cs_files]
        file_sizes.sort(key=lambda x: x[1], reverse=True)

        if file_sizes:
            print(f"  Largest files:")
            for f, lines in file_sizes[:5]:
                flag = " ⚠️" if lines > 200 else ""
                print(f"    {lines:>5} lines — {f.name}{flag}")
                all_files_data.append((f, lines))

        # Count TODOs
        for f in cs_files:
            try:
                content = f.read_text(encoding="utf-8", errors="ignore")
                for i, line in enumerate(content.split("\n"), 1):
                    if re.search(r'TODO|FIXME|HACK|XXX', line, re.IGNORECASE):
                        todos_found.append((f, i, line.strip()[:100]))
            except Exception:
                pass

    # Complexity hotspots
    print(f"\n{'─' * 70}")
    print("COMPLEXITY HOTSPOTS (files > 200 lines)")
    print(f"{'─' * 70}")

    hotspots = [(f, l) for f, l in all_files_data if l > 200]
    hotspots.sort(key=lambda x: x[1], reverse=True)

    if hotspots:
        for f, lines in hotspots[:15]:
            metrics = analyze_complexity(f)
            deps = metrics.get("dependencies", 0)
            dep_flag = " 🔴 too many deps" if deps > 7 else ""
            print(f"  {lines:>5} lines | {metrics.get('methods', 0):>3} methods | {deps:>2} deps — {f.relative_to(root)}{dep_flag}")
    else:
        print("  ✅ No files over 200 lines")

    # TODOs/FIXMEs
    if todos_found:
        print(f"\n{'─' * 70}")
        print(f"TODO/FIXME ({len(todos_found)})")
        print(f"{'─' * 70}")
        for f, line, text in todos_found[:20]:
            print(f"  {f.relative_to(root)}:{line}")
            print(f"    {text}")
        if len(todos_found) > 20:
            print(f"  ... and {len(todos_found) - 20} more")

    # Duplicate file detection
    print(f"\n{'─' * 70}")
    print("POTENTIAL DUPLICATES / BACKUPS")
    print(f"{'─' * 70}")
    bak_files = list(root.rglob("*.bak")) + list(root.rglob("*.cs.bak")) + list(root.rglob("*.old"))
    if bak_files:
        for f in bak_files:
            print(f"  ⚠️ {f.relative_to(root)}")
    else:
        print("  ✅ No backup files found")

    # Summary
    print(f"\n{'=' * 70}")
    print("SUMMARY")
    print(f"{'=' * 70}")
    print(f"  Total .cs files: {total_files}")
    print(f"  Total lines:     {total_lines:,}")
    print(f"  Hotspot files:   {len(hotspots)} (> 200 lines)")
    print(f"  TODOs/FIXMEs:    {len(todos_found)}")
    print(f"  Backup files:    {len(bak_files)}")


if __name__ == "__main__":
    main()
