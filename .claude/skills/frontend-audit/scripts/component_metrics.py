#!/usr/bin/env python3
"""
Component Metrics Scanner

Measures component complexity:
- Lines of code per component (max 200)
- Number of props (max 8)
- JSX nesting depth (max 5)
- Number of hooks per component (max 7)
- Re-export pattern compliance
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List, Tuple


# Thresholds
MAX_LOC = 200
MAX_PROPS = 8
MAX_HOOKS = 7
MAX_JSX_DEPTH = 5


def count_lines(file_path: Path) -> int:
    """Count non-empty, non-comment lines."""
    try:
        content = file_path.read_text(encoding="utf-8")
        lines = content.split('\n')
        count = 0
        in_multiline_comment = False

        for line in lines:
            stripped = line.strip()

            # Handle multiline comments
            if "/*" in stripped:
                in_multiline_comment = True
            if "*/" in stripped:
                in_multiline_comment = False
                continue

            if in_multiline_comment:
                continue

            # Skip empty lines and single-line comments
            if not stripped or stripped.startswith("//"):
                continue

            count += 1

        return count
    except Exception:
        return 0


def count_props(content: str) -> int:
    """Count props in component interface/type."""
    # Match interface/type Props definitions
    props_pattern = r"(?:interface|type)\s+\w*Props\w*\s*(?:extends[^{]+)?\{([^}]+)\}"
    matches = re.findall(props_pattern, content, re.DOTALL)

    max_props = 0
    for match in matches:
        # Count properties (lines with : )
        props = [p.strip() for p in match.split('\n') if ':' in p and not p.strip().startswith('//')]
        max_props = max(max_props, len(props))

    return max_props


def count_hooks(content: str) -> int:
    """Count React hooks usage."""
    hook_patterns = [
        r'\buse[A-Z]\w+\s*\(',  # useXxx(
        r'\buseState\s*[<(]',
        r'\buseEffect\s*\(',
        r'\buseCallback\s*\(',
        r'\buseMemo\s*\(',
        r'\buseRef\s*[<(]',
        r'\buseContext\s*\(',
        r'\buseReducer\s*\(',
        r'\buseQuery\s*\(',
        r'\buseMutation\s*\(',
    ]

    hooks = set()
    for pattern in hook_patterns:
        matches = re.findall(pattern, content)
        hooks.update(matches)

    # More accurate: count unique hook calls
    all_hooks = re.findall(r'\b(use[A-Z][a-zA-Z]+)\s*[<(]', content)
    return len(set(all_hooks))


def measure_jsx_depth(content: str) -> int:
    """Measure maximum JSX nesting depth."""
    # Simplified: count nested opening tags
    max_depth = 0
    current_depth = 0

    # Look for return statement with JSX
    jsx_match = re.search(r'return\s*\(?\s*(<[\s\S]+>)\s*\)?;?\s*\}', content)
    if not jsx_match:
        return 0

    jsx_content = jsx_match.group(1)

    # Count depth by matching opening/closing tags
    for char in jsx_content:
        if char == '<':
            next_chars = jsx_content[jsx_content.index(char):jsx_content.index(char)+2]
            if not next_chars.startswith('</'):
                current_depth += 1
                max_depth = max(max_depth, current_depth)
        elif char == '>':
            # Check if it's a self-closing tag
            prev_char = jsx_content[jsx_content.index(char)-1] if jsx_content.index(char) > 0 else ''
            if prev_char == '/':
                current_depth -= 1

    return max_depth


def analyze_component(file_path: Path, root: Path) -> Dict:
    """Analyze a single component file."""
    try:
        content = file_path.read_text(encoding="utf-8")
    except Exception:
        return None

    # Skip non-component files
    if "export default function" not in content and "export function" not in content:
        if "export const" not in content or "React.FC" not in content:
            return None

    loc = count_lines(file_path)
    props = count_props(content)
    hooks = count_hooks(content)
    jsx_depth = measure_jsx_depth(content)

    issues = []

    if loc > MAX_LOC:
        issues.append({
            "type": "LOC",
            "value": loc,
            "threshold": MAX_LOC,
            "severity": "MINOR"
        })

    if props > MAX_PROPS:
        issues.append({
            "type": "PROPS",
            "value": props,
            "threshold": MAX_PROPS,
            "severity": "MINOR"
        })

    if hooks > MAX_HOOKS:
        issues.append({
            "type": "HOOKS",
            "value": hooks,
            "threshold": MAX_HOOKS,
            "severity": "MINOR"
        })

    if jsx_depth > MAX_JSX_DEPTH:
        issues.append({
            "type": "JSX_DEPTH",
            "value": jsx_depth,
            "threshold": MAX_JSX_DEPTH,
            "severity": "MINOR"
        })

    return {
        "file": str(file_path.relative_to(root)),
        "loc": loc,
        "props": props,
        "hooks": hooks,
        "jsx_depth": jsx_depth,
        "issues": issues
    }


def find_index_exports(root: Path) -> List[Dict]:
    """Check for proper re-export patterns in index.ts files."""
    issues = []

    for index_file in root.rglob("index.ts"):
        if "node_modules" in str(index_file) or ".next" in str(index_file):
            continue
        if "generated" in str(index_file):
            continue

        try:
            content = index_file.read_text(encoding="utf-8")
        except Exception:
            continue

        # Check if index.ts has barrel exports
        if "export *" not in content and "export {" not in content:
            # Check if directory has components that should be exported
            dir_path = index_file.parent
            tsx_files = list(dir_path.glob("*.tsx"))
            ts_files = [f for f in dir_path.glob("*.ts") if f.name != "index.ts"]

            if len(tsx_files) > 0:
                issues.append({
                    "file": str(index_file.relative_to(root)),
                    "issue": "index.ts exists but doesn't export components",
                    "severity": "SUGGESTION"
                })

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: component_metrics.py <frontend-root>")
        print("Example: component_metrics.py orbito-frontend/")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()
    src = root / "src"

    if not src.exists():
        print(f"Error: src/ directory not found in {root}")
        sys.exit(1)

    print("=" * 60)
    print("COMPONENT METRICS SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}\n")
    print(f"Thresholds: LOC={MAX_LOC}, Props={MAX_PROPS}, Hooks={MAX_HOOKS}, JSX Depth={MAX_JSX_DEPTH}\n")

    # Analyze components
    components = []
    for ext in ["*.tsx"]:
        for file_path in src.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue
            if "generated" in str(file_path):
                continue
            if ".test." in str(file_path) or ".spec." in str(file_path):
                continue

            result = analyze_component(file_path, root)
            if result:
                components.append(result)

    # Summary stats
    print("=" * 60)
    print("COMPONENT STATISTICS")
    print("=" * 60)

    total_components = len(components)
    if total_components == 0:
        print("  No components found!")
        sys.exit(0)

    avg_loc = sum(c['loc'] for c in components) / total_components
    avg_props = sum(c['props'] for c in components) / total_components
    avg_hooks = sum(c['hooks'] for c in components) / total_components

    print(f"  Total components: {total_components}")
    print(f"  Average LOC: {avg_loc:.1f}")
    print(f"  Average props: {avg_props:.1f}")
    print(f"  Average hooks: {avg_hooks:.1f}")

    # Components with issues
    print("\n" + "=" * 60)
    print("COMPONENTS EXCEEDING THRESHOLDS")
    print("=" * 60)

    components_with_issues = [c for c in components if c['issues']]

    if components_with_issues:
        # Sort by total severity
        components_with_issues.sort(key=lambda c: len(c['issues']), reverse=True)

        print(f"\n  Found {len(components_with_issues)} components with issues:")

        for comp in components_with_issues[:15]:
            print(f"\n  {comp['file']}")
            print(f"    LOC: {comp['loc']}, Props: {comp['props']}, Hooks: {comp['hooks']}")
            for issue in comp['issues']:
                print(f"    [{issue['severity']}] {issue['type']}: {issue['value']} (max: {issue['threshold']})")

        if len(components_with_issues) > 15:
            print(f"\n  ... and {len(components_with_issues) - 15} more")
    else:
        print("\n  ✅ All components within thresholds")

    # Largest components
    print("\n" + "=" * 60)
    print("TOP 10 LARGEST COMPONENTS")
    print("=" * 60)

    largest = sorted(components, key=lambda c: c['loc'], reverse=True)[:10]
    for comp in largest:
        flag = " ⚠️" if comp['loc'] > MAX_LOC else ""
        print(f"  {comp['loc']:4} LOC  {comp['file']}{flag}")

    # Most complex (hooks)
    print("\n" + "=" * 60)
    print("TOP 10 MOST COMPLEX (BY HOOKS)")
    print("=" * 60)

    most_hooks = sorted(components, key=lambda c: c['hooks'], reverse=True)[:10]
    for comp in most_hooks:
        flag = " ⚠️" if comp['hooks'] > MAX_HOOKS else ""
        print(f"  {comp['hooks']:2} hooks  {comp['file']}{flag}")

    # Index exports check
    print("\n" + "=" * 60)
    print("INDEX.TS EXPORT PATTERNS")
    print("=" * 60)

    index_issues = find_index_exports(src)
    if index_issues:
        print(f"\n  Found {len(index_issues)} index.ts issues:")
        for issue in index_issues[:10]:
            print(f"\n  [{issue['severity']}] {issue['file']}")
            print(f"    {issue['issue']}")
    else:
        print("\n  ✅ Index exports look good")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    total_issues = sum(len(c['issues']) for c in components)
    over_loc = len([c for c in components if c['loc'] > MAX_LOC])
    over_props = len([c for c in components if c['props'] > MAX_PROPS])
    over_hooks = len([c for c in components if c['hooks'] > MAX_HOOKS])

    print(f"  Components over LOC limit ({MAX_LOC}): {over_loc}")
    print(f"  Components over props limit ({MAX_PROPS}): {over_props}")
    print(f"  Components over hooks limit ({MAX_HOOKS}): {over_hooks}")
    print(f"  Total threshold violations: {total_issues}")

    sys.exit(0)


if __name__ == "__main__":
    main()
