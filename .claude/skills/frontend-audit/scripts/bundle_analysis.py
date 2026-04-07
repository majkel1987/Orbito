#!/usr/bin/env python3
"""
Bundle Analysis Scanner

Analyzes bundle size and identifies optimization opportunities:
- Large imports (lodash, moment, etc.)
- Missing tree-shaking patterns
- Duplicate dependencies
- Dynamic import opportunities
"""

import os
import re
import sys
import json
from pathlib import Path
from typing import Dict, List, Set


# Heavy libraries that should be tree-shaken or replaced
HEAVY_IMPORTS = {
    "lodash": {"suggestion": "Use lodash-es or individual imports: import debounce from 'lodash/debounce'"},
    "moment": {"suggestion": "Replace with date-fns (already in stack)"},
    "axios": {"suggestion": "Should only be in core/api/client.ts, not imported directly"},
    "@fortawesome": {"suggestion": "Use lucide-react icons instead (already in stack)"},
    "react-icons": {"suggestion": "Use lucide-react icons instead"},
    "antd": {"suggestion": "Use shadcn/ui components instead"},
    "material-ui": {"suggestion": "Use shadcn/ui components instead"},
    "@mui": {"suggestion": "Use shadcn/ui components instead"},
    "bootstrap": {"suggestion": "Use Tailwind CSS instead"},
}

# Patterns that indicate missing code splitting
CODE_SPLIT_PATTERNS = [
    (r"import\s+.*\s+from\s+['\"]recharts['\"]", "Recharts should be dynamically imported for charts pages only"),
    (r"import\s+.*\s+from\s+['\"]@stripe/react-stripe-js['\"]", "Stripe should be dynamically imported for payment pages only"),
]

# Files that should NOT have certain imports
IMPORT_RESTRICTIONS = {
    "src/app/": {"axios": "Use Orval hooks, not direct axios imports"},
    "src/features/": {"axios": "Use Orval hooks, not direct axios imports", "fetch": "Use Orval hooks"},
    "src/shared/ui/": {"useQuery": "UI components should not fetch data"},
}


def find_heavy_imports(root: Path) -> List[Dict]:
    """Find imports of heavy libraries."""
    issues = []

    for ext in ["*.ts", "*.tsx"]:
        for file_path in root.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue
            if "generated" in str(file_path):
                continue

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            for lib, info in HEAVY_IMPORTS.items():
                # Match import statements
                pattern = rf"import\s+.*\s+from\s+['\"]({lib})['\"]"
                if re.search(pattern, content):
                    issues.append({
                        "file": str(file_path.relative_to(root)),
                        "library": lib,
                        "severity": "MAJOR",
                        "issue": f"Heavy import: {lib}",
                        "suggestion": info["suggestion"]
                    })

                # Also check for require statements
                pattern = rf"require\(['\"]({lib})['\"]"
                if re.search(pattern, content):
                    issues.append({
                        "file": str(file_path.relative_to(root)),
                        "library": lib,
                        "severity": "MAJOR",
                        "issue": f"Heavy require: {lib}",
                        "suggestion": info["suggestion"]
                    })

    return issues


def find_code_split_opportunities(root: Path) -> List[Dict]:
    """Find components that should use dynamic imports."""
    issues = []

    for ext in ["*.ts", "*.tsx"]:
        for file_path in root.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue
            if "generated" in str(file_path):
                continue

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            for pattern, message in CODE_SPLIT_PATTERNS:
                if re.search(pattern, content):
                    # Check if it's already dynamically imported
                    if "dynamic(" not in content and "lazy(" not in content:
                        issues.append({
                            "file": str(file_path.relative_to(root)),
                            "severity": "MINOR",
                            "issue": message,
                            "suggestion": "Use next/dynamic or React.lazy for code splitting"
                        })

    return issues


def check_import_restrictions(root: Path) -> List[Dict]:
    """Check for imports that violate layer restrictions."""
    issues = []

    for ext in ["*.ts", "*.tsx"]:
        for file_path in root.rglob(ext):
            if "node_modules" in str(file_path) or ".next" in str(file_path):
                continue
            if "generated" in str(file_path):
                continue

            rel_path = str(file_path.relative_to(root)).replace("\\", "/")

            try:
                content = file_path.read_text(encoding="utf-8")
            except Exception:
                continue

            for path_prefix, restrictions in IMPORT_RESTRICTIONS.items():
                if rel_path.startswith(path_prefix):
                    for restricted_import, message in restrictions.items():
                        # Check import statements
                        if restricted_import == "fetch":
                            pattern = r"\bfetch\s*\("
                        else:
                            pattern = rf"import\s+.*\s+from\s+['\"].*{restricted_import}.*['\"]"

                        if re.search(pattern, content):
                            issues.append({
                                "file": rel_path,
                                "severity": "CRITICAL" if restricted_import in ["axios", "fetch"] else "MAJOR",
                                "issue": f"Restricted import: {restricted_import}",
                                "suggestion": message
                            })

    return issues


def analyze_package_json(root: Path) -> Dict:
    """Analyze package.json for bundle concerns."""
    package_path = root / "package.json"
    if not package_path.exists():
        return {"error": "package.json not found"}

    try:
        data = json.loads(package_path.read_text(encoding="utf-8"))
    except Exception as e:
        return {"error": f"Failed to parse package.json: {e}"}

    deps = {**data.get("dependencies", {}), **data.get("devDependencies", {})}

    issues = []
    for lib in HEAVY_IMPORTS:
        if lib in deps:
            issues.append({
                "library": lib,
                "version": deps[lib],
                "suggestion": HEAVY_IMPORTS[lib]["suggestion"]
            })

    # Check for duplicate React versions
    react_deps = [k for k in deps if "react" in k.lower()]

    return {
        "total_dependencies": len(data.get("dependencies", {})),
        "total_devDependencies": len(data.get("devDependencies", {})),
        "heavy_libraries": issues,
        "react_related": react_deps
    }


def main():
    if len(sys.argv) < 2:
        print("Usage: bundle_analysis.py <frontend-root>")
        print("Example: bundle_analysis.py orbito-frontend/")
        sys.exit(1)

    root = Path(sys.argv[1]).resolve()
    src = root / "src"

    if not src.exists():
        print(f"Error: src/ directory not found in {root}")
        sys.exit(1)

    print("=" * 60)
    print("BUNDLE ANALYSIS SCANNER")
    print("=" * 60)
    print(f"Scanning: {root}\n")

    # Package.json analysis
    print("=" * 60)
    print("PACKAGE.JSON ANALYSIS")
    print("=" * 60)

    pkg_analysis = analyze_package_json(root)
    if "error" in pkg_analysis:
        print(f"  Error: {pkg_analysis['error']}")
    else:
        print(f"  Dependencies: {pkg_analysis['total_dependencies']}")
        print(f"  Dev Dependencies: {pkg_analysis['total_devDependencies']}")

        if pkg_analysis['heavy_libraries']:
            print(f"\n  Heavy libraries found:")
            for lib in pkg_analysis['heavy_libraries']:
                print(f"    - {lib['library']} ({lib['version']})")
                print(f"      Suggestion: {lib['suggestion']}")

    # Heavy imports scan
    print("\n" + "=" * 60)
    print("HEAVY IMPORTS SCAN")
    print("=" * 60)

    heavy = find_heavy_imports(src)
    if heavy:
        print(f"\n  Found {len(heavy)} heavy import issues:")
        for issue in heavy[:15]:
            print(f"\n  [{issue['severity']}] {issue['file']}")
            print(f"    Issue: {issue['issue']}")
            print(f"    Fix: {issue['suggestion']}")
        if len(heavy) > 15:
            print(f"\n  ... and {len(heavy) - 15} more")
    else:
        print("\n  ✅ No heavy imports found")

    # Import restrictions
    print("\n" + "=" * 60)
    print("IMPORT RESTRICTIONS CHECK")
    print("=" * 60)

    restrictions = check_import_restrictions(src)
    if restrictions:
        criticals = [i for i in restrictions if i['severity'] == 'CRITICAL']
        majors = [i for i in restrictions if i['severity'] == 'MAJOR']

        print(f"\n  Found {len(restrictions)} issues ({len(criticals)} critical, {len(majors)} major):")
        for issue in restrictions[:15]:
            print(f"\n  [{issue['severity']}] {issue['file']}")
            print(f"    Issue: {issue['issue']}")
            print(f"    Fix: {issue['suggestion']}")
        if len(restrictions) > 15:
            print(f"\n  ... and {len(restrictions) - 15} more")
    else:
        print("\n  ✅ No import restriction violations")

    # Code splitting opportunities
    print("\n" + "=" * 60)
    print("CODE SPLITTING OPPORTUNITIES")
    print("=" * 60)

    code_split = find_code_split_opportunities(src)
    if code_split:
        print(f"\n  Found {len(code_split)} code splitting opportunities:")
        for issue in code_split[:10]:
            print(f"\n  [{issue['severity']}] {issue['file']}")
            print(f"    Issue: {issue['issue']}")
            print(f"    Fix: {issue['suggestion']}")
    else:
        print("\n  ✅ Heavy libraries properly code-split")

    # Summary
    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    total_critical = len([i for i in restrictions if i['severity'] == 'CRITICAL'])
    total_major = len(heavy) + len([i for i in restrictions if i['severity'] == 'MAJOR'])
    total_minor = len(code_split)

    print(f"  Critical: {total_critical}")
    print(f"  Major: {total_major}")
    print(f"  Minor: {total_minor}")

    if total_critical > 0:
        print("\n  ⚠️  CRITICAL issues found - fix immediately!")
        sys.exit(1)
    elif total_major > 5:
        print("\n  ⚠️  Multiple MAJOR issues found")
        sys.exit(0)
    else:
        print("\n  ✅ Bundle looks healthy")
        sys.exit(0)


if __name__ == "__main__":
    main()
