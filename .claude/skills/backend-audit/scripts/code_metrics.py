#!/usr/bin/env python3
"""
Code Metrics Scanner

Analyzes code metrics per project:
- File count, line count, avg lines/file
- Complexity hotspots (files >200 lines)
- TODO/FIXME scan
- Backup file detection
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List, Tuple


PROJECTS = [
    "Orbito.API",
    "Orbito.Application",
    "Orbito.Domain",
    "Orbito.Infrastructure",
    "Orbito.Tests",
]

# Patterns for TODOs
TODO_PATTERNS = [
    r'\bTODO\b',
    r'\bFIXME\b',
    r'\bHACK\b',
    r'\bXXX\b',
    r'\bBUG\b',
]

# Backup file patterns
BACKUP_PATTERNS = [
    "*.bak",
    "*.cs.bak",
    "*.old",
    "*.orig",
    "*.backup",
    "*~",
]


def count_lines(file_path: Path) -> int:
    """Count lines in a file."""
    try:
        return len(file_path.read_text(encoding="utf-8").split('\n'))
    except Exception:
        return 0


def count_methods(content: str) -> int:
    """Count methods in file (approximation)."""
    method_pattern = re.compile(
        r'(public|private|protected|internal)\s+(static\s+)?(async\s+)?(Task|void|string|int|bool|[A-Z][a-zA-Z<>]+)\s+\w+\s*\('
    )
    return len(method_pattern.findall(content))


def count_dependencies(content: str) -> int:
    """Count injected dependencies (private readonly I* fields)."""
    dep_pattern = re.compile(r'private\s+readonly\s+I[A-Z][a-zA-Z]+\s+_')
    return len(dep_pattern.findall(content))


def count_try_catch(content: str) -> int:
    """Count try-catch blocks."""
    return len(re.findall(r'\btry\s*{', content))


def find_todos(root: Path) -> List[Dict]:
    """Find all TODO/FIXME comments."""
    todos = []

    for project in PROJECTS:
        project_path = root / project
        if not project_path.exists():
            continue

        for file_path in project_path.rglob("*.cs"):
            if "/bin/" in str(file_path) or "/obj/" in str(file_path):
                continue
            if "\\bin\\" in str(file_path) or "\\obj\\" in str(file_path):
                continue

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            lines = content.split('\n')
            for line_num, line in enumerate(lines, 1):
                for pattern in TODO_PATTERNS:
                    if re.search(pattern, line, re.IGNORECASE):
                        todos.append({
                            "file": str(file_path.relative_to(root)),
                            "line": line_num,
                            "content": line.strip()[:100]
                        })
                        break

    return todos


def find_backup_files(root: Path) -> List[str]:
    """Find backup/temp files."""
    backups = []

    for pattern in BACKUP_PATTERNS:
        for file_path in root.rglob(pattern):
            if "/bin/" not in str(file_path) and "/obj/" not in str(file_path):
                if "\\bin\\" not in str(file_path) and "\\obj\\" not in str(file_path):
                    backups.append(str(file_path.relative_to(root)))

    return backups


def analyze_project(root: Path, project_name: str) -> Dict:
    """Analyze a single project."""
    project_path = root / project_name
    if not project_path.exists():
        return None

    files = []
    for f in project_path.rglob("*.cs"):
        if "/bin/" not in str(f) and "/obj/" not in str(f):
            if "\\bin\\" not in str(f) and "\\obj\\" not in str(f):
                files.append(f)

    if not files:
        return None

    # Basic metrics
    total_lines = 0
    file_sizes = []

    for f in files:
        lines = count_lines(f)
        total_lines += lines
        file_sizes.append((f, lines))

    avg_lines = total_lines / len(files) if files else 0

    # Top 5 largest files
    file_sizes.sort(key=lambda x: x[1], reverse=True)
    largest = [(str(f.relative_to(root)), lines) for f, lines in file_sizes[:5]]

    # Complexity hotspots (>200 lines)
    hotspots = []
    for f, lines in file_sizes:
        if lines > 200:
            try:
                content = f.read_text(encoding="utf-8")
                hotspots.append({
                    "file": str(f.relative_to(root)),
                    "lines": lines,
                    "methods": count_methods(content),
                    "dependencies": count_dependencies(content),
                    "try_catch": count_try_catch(content),
                })
            except Exception:
                pass

    return {
        "name": project_name,
        "file_count": len(files),
        "line_count": total_lines,
        "avg_lines": round(avg_lines, 1),
        "largest_files": largest,
        "hotspots": hotspots,
    }


def main():
    if len(sys.argv) < 2:
        print("Usage: code_metrics.py <solution-root>")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()

    print("=" * 60)
    print("CODE METRICS SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}\n")

    # Analyze each project
    all_metrics = []
    for project in PROJECTS:
        metrics = analyze_project(root, project)
        if metrics:
            all_metrics.append(metrics)

    # Print project metrics
    print("=" * 60)
    print("PROJECT METRICS")
    print("=" * 60)

    total_files = 0
    total_lines = 0

    for m in all_metrics:
        print(f"\n### {m['name']} ###")
        print(f"  Files: {m['file_count']}")
        print(f"  Lines: {m['line_count']:,}")
        print(f"  Avg lines/file: {m['avg_lines']}")

        if m['largest_files']:
            print(f"\n  Top 5 largest files:")
            for file, lines in m['largest_files']:
                flag = " ⚠️ >200" if lines > 200 else ""
                print(f"    {lines:4} lines  {file}{flag}")

        total_files += m['file_count']
        total_lines += m['line_count']

    # Complexity hotspots
    print("\n" + "=" * 60)
    print("COMPLEXITY HOTSPOTS (>200 lines)")
    print("=" * 60)

    all_hotspots = []
    for m in all_metrics:
        all_hotspots.extend(m['hotspots'])

    if all_hotspots:
        all_hotspots.sort(key=lambda x: x['lines'], reverse=True)
        for h in all_hotspots[:15]:
            print(f"\n  {h['file']}")
            print(f"    Lines: {h['lines']}, Methods: {h['methods']}, Dependencies: {h['dependencies']}, Try/Catch: {h['try_catch']}")
    else:
        print("\n  ✅ No files over 200 lines")

    # TODOs
    print("\n" + "=" * 60)
    print("TODO/FIXME SCAN")
    print("=" * 60)

    todos = find_todos(root)
    if todos:
        print(f"\n  Found {len(todos)} TODO/FIXME comments:")
        for t in todos[:20]:
            print(f"\n    {t['file']}:{t['line']}")
            print(f"      {t['content']}")
        if len(todos) > 20:
            print(f"\n    ... and {len(todos) - 20} more")
    else:
        print("\n  ✅ No TODO/FIXME comments found")

    # Backup files
    print("\n" + "=" * 60)
    print("BACKUP FILE DETECTION")
    print("=" * 60)

    backups = find_backup_files(root)
    if backups:
        print(f"\n  Found {len(backups)} backup files:")
        for b in backups:
            print(f"    ⚠️  {b}")
    else:
        print("\n  ✅ No backup files found")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)
    print(f"  Total projects: {len(all_metrics)}")
    print(f"  Total files: {total_files}")
    print(f"  Total lines: {total_lines:,}")
    print(f"  Hotspots (>200 lines): {len(all_hotspots)}")
    print(f"  TODOs/FIXMEs: {len(todos)}")
    print(f"  Backup files: {len(backups)}")

    sys.exit(0)


if __name__ == "__main__":
    main()
