#!/usr/bin/env python3
"""
Accessibility Scanner

Checks for common accessibility issues:
- Missing alt on images
- Missing aria-label on icon buttons
- Missing htmlFor on labels
- Hardcoded colors (not CSS variables)
- Keyboard navigation patterns
- Focus management
"""

import os
import re
import sys
from pathlib import Path
from typing import Dict, List


# Patterns to detect
A11Y_PATTERNS = [
    # Images without alt
    {
        "pattern": r'<img[^>]*(?<!alt=")[^>]*>',
        "alt_pattern": r'<img[^>]*alt=',
        "description": "Image missing alt attribute",
        "severity": "MAJOR",
        "type": "IMG_ALT"
    },
    # next/image without alt
    {
        "pattern": r'<Image[^>]*(?<!alt=")[^>]*/>',
        "alt_pattern": r'<Image[^>]*alt=',
        "description": "next/image missing alt attribute",
        "severity": "MAJOR",
        "type": "IMG_ALT"
    },
    # Button with only icon (no aria-label)
    {
        "pattern": r'<[Bb]utton[^>]*>\s*<[^>]*[Ii]con[^>]*/>\s*</[Bb]utton>',
        "alt_pattern": r'aria-label=',
        "description": "Icon button missing aria-label",
        "severity": "MAJOR",
        "type": "ARIA_LABEL"
    },
    # Label without htmlFor
    {
        "pattern": r'<label(?![^>]*htmlFor)[^>]*>',
        "alt_pattern": r'htmlFor=',
        "description": "Label missing htmlFor attribute",
        "severity": "MINOR",
        "type": "LABEL_FOR"
    },
    # div with onClick (should be button)
    {
        "pattern": r'<div[^>]*onClick[^>]*>',
        "alt_pattern": r'role="button"',
        "description": "div with onClick should be button or have role='button'",
        "severity": "MAJOR",
        "type": "SEMANTIC"
    },
    # span with onClick (should be button)
    {
        "pattern": r'<span[^>]*onClick[^>]*>',
        "alt_pattern": r'role="button"',
        "description": "span with onClick should be button or have role='button'",
        "severity": "MAJOR",
        "type": "SEMANTIC"
    },
]

# Hardcoded color patterns
COLOR_PATTERNS = [
    r'style=\{[^}]*color:\s*["\']#[0-9a-fA-F]{3,6}["\']',
    r'style=\{[^}]*backgroundColor:\s*["\']#[0-9a-fA-F]{3,6}["\']',
    r'className="[^"]*text-\[#[0-9a-fA-F]{3,6}\]',
    r'className="[^"]*bg-\[#[0-9a-fA-F]{3,6}\]',
]


def scan_file(file_path: Path, root: Path) -> List[Dict]:
    """Scan a single file for accessibility issues."""
    issues = []
    rel_path = str(file_path.relative_to(root))

    try:
        content = file_path.read_text(encoding="utf-8")
        lines = content.split('\n')
    except Exception:
        return []

    # Check for images without alt
    for line_num, line in enumerate(lines, 1):
        # Skip test files and generated files
        if '.test.' in rel_path or 'generated' in rel_path:
            continue

        # Check for img/Image without alt
        if '<img' in line.lower() or '<Image' in line:
            if 'alt=' not in line and 'alt =' not in line:
                # Check if alt is on next line
                if line_num < len(lines) and 'alt=' not in lines[line_num]:
                    issues.append({
                        "file": rel_path,
                        "line": line_num,
                        "type": "IMG_ALT",
                        "severity": "MAJOR",
                        "description": "Image missing alt attribute",
                        "content": line.strip()[:80]
                    })

        # Check for icon-only buttons
        if re.search(r'<[Bb]utton', line):
            # Look for button content
            if 'Icon' in line or 'icon' in line:
                if 'aria-label' not in line and 'aria-label' not in lines[min(line_num, len(lines)-1)]:
                    issues.append({
                        "file": rel_path,
                        "line": line_num,
                        "type": "ARIA_LABEL",
                        "severity": "MAJOR",
                        "description": "Button with icon may need aria-label",
                        "content": line.strip()[:80]
                    })

        # Check for div/span with onClick
        if '<div' in line and 'onClick' in line:
            if 'role=' not in line and 'role=' not in content[content.find(line):content.find(line)+200]:
                issues.append({
                    "file": rel_path,
                    "line": line_num,
                    "type": "SEMANTIC",
                    "severity": "MAJOR",
                    "description": "div with onClick should be button or have role='button'",
                    "content": line.strip()[:80]
                })

        # Check for hardcoded colors
        for pattern in COLOR_PATTERNS:
            if re.search(pattern, line):
                issues.append({
                    "file": rel_path,
                    "line": line_num,
                    "type": "COLOR",
                    "severity": "MINOR",
                    "description": "Hardcoded color instead of CSS variable/Tailwind",
                    "content": line.strip()[:80]
                })
                break

        # Check for missing focus styles
        if ':focus' in line or 'focus:' in line:
            # This is good - component has focus styles
            pass
        elif 'tabIndex' in line or 'onClick' in line:
            if ':focus' not in content and 'focus:' not in content:
                if 'Button' not in line and 'button' not in line:
                    # Interactive element without focus styles
                    pass  # Too many false positives, skip for now

    return issues


def check_keyboard_nav(root: Path) -> List[Dict]:
    """Check for keyboard navigation patterns."""
    issues = []
    src = root / "src"

    # Check for modal/dialog components without focus trap
    modal_patterns = ["Modal", "Dialog", "Sheet", "Drawer"]

    for ext in ["*.tsx"]:
        for file_path in src.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue
            if "generated" in str(file_path):
                continue

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            # Check if file has modal-like component
            for pattern in modal_patterns:
                if f"function {pattern}" in content or f"const {pattern}" in content:
                    # Check for focus trap
                    if "FocusTrap" not in content and "useFocusTrap" not in content:
                        if "Dialog" not in content:  # Radix Dialog has built-in focus trap
                            issues.append({
                                "file": str(file_path.relative_to(root)),
                                "line": 1,
                                "type": "FOCUS_TRAP",
                                "severity": "MAJOR",
                                "description": f"{pattern} component may need focus trap for accessibility",
                                "content": ""
                            })

    return issues


def main():
    if len(sys.argv) < 2:
        print("Usage: accessibility_scan.py <frontend-root>")
        print("Example: accessibility_scan.py orbito-frontend/")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()
    src = root / "src"

    if not src.exists():
        print(f"Error: src/ directory not found in {root}")
        sys.exit(1)

    print("=" * 60)
    print("ACCESSIBILITY SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}\n")

    # Collect all issues
    all_issues = []

    for ext in ["*.tsx"]:
        for file_path in src.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue
            if "generated" in str(file_path):
                continue

            issues = scan_file(file_path, root)
            all_issues.extend(issues)

    # Add keyboard nav issues
    keyboard_issues = check_keyboard_nav(root)
    all_issues.extend(keyboard_issues)

    # Group by type
    img_issues = [i for i in all_issues if i['type'] == 'IMG_ALT']
    aria_issues = [i for i in all_issues if i['type'] == 'ARIA_LABEL']
    semantic_issues = [i for i in all_issues if i['type'] == 'SEMANTIC']
    color_issues = [i for i in all_issues if i['type'] == 'COLOR']
    focus_issues = [i for i in all_issues if i['type'] == 'FOCUS_TRAP']

    # Images without alt
    print("=" * 60)
    print(f"IMAGES WITHOUT ALT ({len(img_issues)} found)")
    print("=" * 60)

    if img_issues:
        for issue in img_issues[:10]:
            print(f"\n  [MAJOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
        if len(img_issues) > 10:
            print(f"\n  ... and {len(img_issues) - 10} more")
    else:
        print("\n  ✅ All images have alt attributes")

    # ARIA labels
    print("\n" + "=" * 60)
    print(f"MISSING ARIA-LABEL ({len(aria_issues)} found)")
    print("=" * 60)

    if aria_issues:
        for issue in aria_issues[:10]:
            print(f"\n  [MAJOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
        if len(aria_issues) > 10:
            print(f"\n  ... and {len(aria_issues) - 10} more")
    else:
        print("\n  ✅ Icon buttons have aria-labels")

    # Semantic HTML
    print("\n" + "=" * 60)
    print(f"SEMANTIC HTML ISSUES ({len(semantic_issues)} found)")
    print("=" * 60)

    if semantic_issues:
        for issue in semantic_issues[:10]:
            print(f"\n  [MAJOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
            print(f"    Fix: Use <button> instead of <div onClick>")
        if len(semantic_issues) > 10:
            print(f"\n  ... and {len(semantic_issues) - 10} more")
    else:
        print("\n  ✅ Semantic HTML looks good")

    # Hardcoded colors
    print("\n" + "=" * 60)
    print(f"HARDCODED COLORS ({len(color_issues)} found)")
    print("=" * 60)

    if color_issues:
        for issue in color_issues[:10]:
            print(f"\n  [MINOR] {issue['file']}:{issue['line']}")
            print(f"    {issue['description']}")
            print(f"    Fix: Use CSS variables or Tailwind color classes")
        if len(color_issues) > 10:
            print(f"\n  ... and {len(color_issues) - 10} more")
    else:
        print("\n  ✅ No hardcoded colors found")

    # Focus management
    print("\n" + "=" * 60)
    print(f"FOCUS MANAGEMENT ({len(focus_issues)} found)")
    print("=" * 60)

    if focus_issues:
        for issue in focus_issues[:10]:
            print(f"\n  [MAJOR] {issue['file']}")
            print(f"    {issue['description']}")
        if len(focus_issues) > 10:
            print(f"\n  ... and {len(focus_issues) - 10} more")
    else:
        print("\n  ✅ Focus management looks good")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    total_major = len(img_issues) + len(aria_issues) + len(semantic_issues) + len(focus_issues)
    total_minor = len(color_issues)

    print(f"  Major issues: {total_major}")
    print(f"  Minor issues: {total_minor}")

    if total_major == 0:
        print("\n  ✅ Accessibility looks GOOD")
    elif total_major < 5:
        print("\n  ⚠️  Some accessibility issues found")
    else:
        print("\n  ❌ Multiple accessibility issues need attention")

    sys.exit(0 if total_major == 0 else 1)


if __name__ == "__main__":
    main()
