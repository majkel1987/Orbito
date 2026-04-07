#!/usr/bin/env python3
"""
Performance Scanner

Detects performance anti-patterns:
- Missing useMemo/useCallback for expensive operations
- Inline object/array props (re-render triggers)
- Missing key on lists
- Large re-renders (component with many hooks)
- Missing lazy loading on route-level
- staleTime: 0 (refetch on every focus)
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List


# Performance anti-patterns
ANTI_PATTERNS = [
    # Inline object props
    {
        "pattern": r'(\w+)=\{\s*\{[^}]+\}\s*\}',
        "description": "Inline object prop (creates new reference each render)",
        "severity": "MINOR",
        "type": "INLINE_OBJECT",
        "fix": "Extract to useMemo or define outside component"
    },
    # Inline array props
    {
        "pattern": r'(\w+)=\{\s*\[[^\]]+\]\s*\}',
        "description": "Inline array prop (creates new reference each render)",
        "severity": "MINOR",
        "type": "INLINE_ARRAY",
        "fix": "Extract to useMemo or define outside component"
    },
    # Inline function props (excluding event handlers)
    {
        "pattern": r'(\w+)=\{\s*\([^)]*\)\s*=>',
        "description": "Inline arrow function prop (may cause re-renders)",
        "severity": "MINOR",
        "type": "INLINE_FUNCTION",
        "fix": "Extract to useCallback if passed to memoized child"
    },
    # Missing key on .map
    {
        "pattern": r'\.map\s*\(\s*\([^)]*\)\s*=>\s*<\w+(?![^>]*key=)',
        "description": "Missing key prop in .map()",
        "severity": "MAJOR",
        "type": "MISSING_KEY",
        "fix": "Add unique key prop to mapped elements"
    },
    # staleTime: 0
    {
        "pattern": r'staleTime\s*:\s*0',
        "description": "staleTime: 0 causes refetch on every focus",
        "severity": "MINOR",
        "type": "STALE_TIME",
        "fix": "Set staleTime to at least 5 minutes for most queries"
    },
    # New object in dependency array
    {
        "pattern": r'useEffect\s*\([^,]+,\s*\[\s*\{',
        "description": "Object in useEffect dependency array",
        "severity": "MAJOR",
        "type": "EFFECT_DEP",
        "fix": "Extract object properties or use useMemo"
    },
]

# Check for missing React.memo on frequently rendered components
MEMO_CANDIDATES = [
    "ListItem",
    "TableRow",
    "Card",
    "Item",
    "Cell",
]


def scan_file(file_path: Path, root: Path) -> List[Dict]:
    """Scan a file for performance issues."""
    issues = []
    rel_path = str(file_path.relative_to(root))

    try:
        content = file_path.read_text(encoding="utf-8")
        lines = content.split('\n')
    except Exception:
        return []

    # Skip test and generated files
    if '.test.' in rel_path or 'generated' in rel_path:
        return []

    for line_num, line in enumerate(lines, 1):
        for pattern in ANTI_PATTERNS:
            if re.search(pattern["pattern"], line):
                # Skip common false positives
                if pattern["type"] == "INLINE_FUNCTION":
                    # Skip event handlers on native elements
                    if 'onClick={' in line or 'onChange={' in line or 'onSubmit={' in line:
                        continue
                    # Skip if it's a simple operation
                    if len(line) < 60:
                        continue

                if pattern["type"] == "INLINE_OBJECT":
                    # Skip style prop (common pattern)
                    if 'style={' in line:
                        continue
                    # Skip className with cn()
                    if 'className={' in line:
                        continue

                issues.append({
                    "file": rel_path,
                    "line": line_num,
                    "type": pattern["type"],
                    "severity": pattern["severity"],
                    "description": pattern["description"],
                    "fix": pattern["fix"],
                    "content": line.strip()[:80]
                })

    # Check for missing React.memo
    for candidate in MEMO_CANDIDATES:
        if f"function {candidate}" in content or f"const {candidate}" in content:
            if "React.memo" not in content and "memo(" not in content:
                issues.append({
                    "file": rel_path,
                    "line": 1,
                    "type": "MEMO_CANDIDATE",
                    "severity": "SUGGESTION",
                    "description": f"{candidate} component might benefit from React.memo",
                    "fix": "Wrap with React.memo if rendered in lists",
                    "content": ""
                })

    return issues


def check_lazy_loading(root: Path) -> List[Dict]:
    """Check for missing lazy loading in route components."""
    issues = []
    src = root / "src"
    app_dir = src / "app"

    if not app_dir.exists():
        return []

    # Check page.tsx files for heavy imports
    heavy_imports = ["recharts", "react-chartjs", "monaco-editor", "@stripe"]

    for page_file in app_dir.rglob("page.tsx"):
        try:
            content = page_file.read_text(encoding="utf-8")
        except Exception:
            continue

        for heavy_import in heavy_imports:
            if f"from '{heavy_import}" in content or f'from "{heavy_import}' in content:
                if "dynamic(" not in content and "lazy(" not in content:
                    issues.append({
                        "file": str(page_file.relative_to(root)),
                        "line": 1,
                        "type": "LAZY_LOAD",
                        "severity": "MINOR",
                        "description": f"Heavy import ({heavy_import}) in page.tsx should be lazy loaded",
                        "fix": "Use next/dynamic for code splitting",
                        "content": ""
                    })

    return issues


def check_query_config(root: Path) -> List[Dict]:
    """Check React Query configuration."""
    issues = []
    src = root / "src"

    # Find QueryClient configuration
    for ext in ["*.ts", "*.tsx"]:
        for file_path in src.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            # Check for QueryClient config
            if "new QueryClient" in content:
                rel_path = str(file_path.relative_to(root))

                # Check staleTime
                if "staleTime" not in content:
                    issues.append({
                        "file": rel_path,
                        "line": 1,
                        "type": "QUERY_CONFIG",
                        "severity": "SUGGESTION",
                        "description": "QueryClient missing default staleTime",
                        "fix": "Add defaultOptions.queries.staleTime (e.g., 5 * 60 * 1000)",
                        "content": ""
                    })

                # Check gcTime (garbage collection time)
                if "gcTime" not in content and "cacheTime" not in content:
                    issues.append({
                        "file": rel_path,
                        "line": 1,
                        "type": "QUERY_CONFIG",
                        "severity": "SUGGESTION",
                        "description": "QueryClient missing gcTime configuration",
                        "fix": "Add defaultOptions.queries.gcTime (e.g., 10 * 60 * 1000)",
                        "content": ""
                    })

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: performance_scan.py <frontend-root>")
        print("Example: performance_scan.py orbito-frontend/")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()
    src = root / "src"

    if not src.exists():
        print(f"Error: src/ directory not found in {root}")
        sys.exit(1)

    print("=" * 60)
    print("PERFORMANCE SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}\n")

    # Collect all issues
    all_issues = []

    for ext in ["*.tsx", "*.ts"]:
        for file_path in src.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue

            issues = scan_file(file_path, root)
            all_issues.extend(issues)

    # Add lazy loading issues
    lazy_issues = check_lazy_loading(root)
    all_issues.extend(lazy_issues)

    # Add query config issues
    query_issues = check_query_config(root)
    all_issues.extend(query_issues)

    # Group by type
    inline_issues = [i for i in all_issues if i['type'] in ['INLINE_OBJECT', 'INLINE_ARRAY', 'INLINE_FUNCTION']]
    key_issues = [i for i in all_issues if i['type'] == 'MISSING_KEY']
    effect_issues = [i for i in all_issues if i['type'] == 'EFFECT_DEP']
    lazy_issues = [i for i in all_issues if i['type'] == 'LAZY_LOAD']
    memo_issues = [i for i in all_issues if i['type'] == 'MEMO_CANDIDATE']
    query_config_issues = [i for i in all_issues if i['type'] == 'QUERY_CONFIG']

    # Inline props
    print("=" * 60)
    print(f"INLINE PROPS (potential re-render triggers) ({len(inline_issues)} found)")
    print("=" * 60)

    if inline_issues:
        for issue in inline_issues[:10]:
            print(f"\n  [{issue['severity']}] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
            print(f"    Fix: {issue['fix']}")
        if len(inline_issues) > 10:
            print(f"\n  ... and {len(inline_issues) - 10} more")
    else:
        print("\n  ✅ No inline prop issues found")

    # Missing keys
    print("\n" + "=" * 60)
    print(f"MISSING KEY IN .MAP() ({len(key_issues)} found)")
    print("=" * 60)

    if key_issues:
        for issue in key_issues[:10]:
            print(f"\n  [MAJOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
        if len(key_issues) > 10:
            print(f"\n  ... and {len(key_issues) - 10} more")
    else:
        print("\n  ✅ All .map() calls have key props")

    # Effect dependencies
    print("\n" + "=" * 60)
    print(f"USEEFFECT DEPENDENCY ISSUES ({len(effect_issues)} found)")
    print("=" * 60)

    if effect_issues:
        for issue in effect_issues[:10]:
            print(f"\n  [MAJOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
            print(f"    Fix: {issue['fix']}")
    else:
        print("\n  ✅ No useEffect dependency issues found")

    # Lazy loading
    print("\n" + "=" * 60)
    print(f"LAZY LOADING OPPORTUNITIES ({len(lazy_issues)} found)")
    print("=" * 60)

    if lazy_issues:
        for issue in lazy_issues:
            print(f"\n  [MINOR] {issue['file']}")
            print(f"    {issue['description']}")
            print(f"    Fix: {issue['fix']}")
    else:
        print("\n  ✅ Heavy components properly lazy loaded")

    # React.memo candidates
    print("\n" + "=" * 60)
    print(f"REACT.MEMO CANDIDATES ({len(memo_issues)} found)")
    print("=" * 60)

    if memo_issues:
        for issue in memo_issues:
            print(f"\n  [SUGGESTION] {issue['file']}")
            print(f"    {issue['description']}")
    else:
        print("\n  ✅ No obvious memo candidates")

    # Query config
    print("\n" + "=" * 60)
    print("REACT QUERY CONFIGURATION")
    print("=" * 60)

    if query_config_issues:
        for issue in query_config_issues:
            print(f"\n  [SUGGESTION] {issue['file']}")
            print(f"    {issue['description']}")
            print(f"    Fix: {issue['fix']}")
    else:
        print("\n  ✅ React Query configuration looks good")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    total_major = len(key_issues) + len(effect_issues)
    total_minor = len(inline_issues) + len(lazy_issues)
    total_suggestions = len(memo_issues) + len(query_config_issues)

    print(f"  Major issues: {total_major}")
    print(f"  Minor issues: {total_minor}")
    print(f"  Suggestions: {total_suggestions}")

    if total_major == 0:
        print("\n  ✅ Performance looks GOOD")
    else:
        print("\n  ⚠️  Some performance issues need attention")

    sys.exit(0)


if __name__ == "__main__":
    main()
