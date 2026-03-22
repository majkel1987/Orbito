# Backend Audit — Quick Start Guide

## Installation

Copy the entire `backend-audit` folder into your project:

```bash
# From your Orbito project root:
cp -r <downloaded-path>/backend-audit .agents/skills/backend-audit
# OR if using .claude/skills:
cp -r <downloaded-path>/backend-audit .claude/skills/backend-audit
```

## Recommended Audit Order

Run phases in this order for maximum efficiency — later phases depend on context from earlier ones.

### Sprint 1: Automated Scans (30 min total)

These are fast, script-based — run all three in one session:

```bash
claude "run backend-audit --phase 0.1"   # Build warnings
claude "run backend-audit --phase 0.2"   # Dependency scan
claude "run backend-audit --phase 0.3"   # Code metrics
```

### Sprint 2: Critical Security (1 session per block)

These are the highest-value audits — run them BEFORE any other manual review:

```bash
claude "run backend-audit --phase 5.1"   # Multi-tenancy security ⚠️
claude "run backend-audit --phase 5.4"   # Security hardening
claude "run backend-audit --phase 3.3"   # Stripe integration
```

### Sprint 3: Domain Layer (1 session per block)

Bottom-up audit — start from the domain core:

```bash
claude "run backend-audit --phase 1.1"   # Entities
claude "run backend-audit --phase 1.2"   # Value Objects
claude "run backend-audit --phase 1.3"   # Enums
claude "run backend-audit --phase 1.4"   # Events & Errors
claude "run backend-audit --phase 1.5"   # Domain Interfaces
```

### Sprint 4: Application Layer (1 session per block)

The biggest layer — strictly one block per session:

```bash
claude "run backend-audit --phase 2.1"   # Client Commands
claude "run backend-audit --phase 2.2"   # Client Queries
claude "run backend-audit --phase 2.3"   # Payment Commands
claude "run backend-audit --phase 2.4"   # Subscription Commands
claude "run backend-audit --phase 2.5"   # Other Commands
claude "run backend-audit --phase 2.6"   # Interfaces (may split!)
claude "run backend-audit --phase 2.7"   # Validators
claude "run backend-audit --phase 2.8"   # Services
claude "run backend-audit --phase 2.9"   # Background Jobs
claude "run backend-audit --phase 2.10"  # DTOs & Models
claude "run backend-audit --phase 2.11"  # Behaviours
```

### Sprint 5: Infrastructure (1 session per block)

```bash
claude "run backend-audit --phase 3.1"   # DbContext & Config
claude "run backend-audit --phase 3.2"   # Repositories
claude "run backend-audit --phase 3.4"   # Services
claude "run backend-audit --phase 3.5"   # Background Jobs
claude "run backend-audit --phase 3.6"   # DI Registration
```

### Sprint 6: API Layer (1 session per block)

```bash
claude "run backend-audit --phase 4.1"   # Controllers Part 1
claude "run backend-audit --phase 4.2"   # Controllers Part 2
claude "run backend-audit --phase 4.3"   # Middleware
claude "run backend-audit --phase 4.4"   # Program.cs
claude "run backend-audit --phase 4.5"   # Health Checks
```

### Sprint 7: Cross-Cutting & Performance (1 session per block)

```bash
claude "run backend-audit --phase 5.2"   # Error Handling
claude "run backend-audit --phase 5.3"   # Logging
claude "run backend-audit --phase 5.5"   # Performance
```

### Sprint 8: Test Quality (1 session per block)

```bash
claude "run backend-audit --phase 6.1"   # Coverage Gaps
claude "run backend-audit --phase 6.2"   # Unit Test Quality
claude "run backend-audit --phase 6.3"   # Integration Test Quality
claude "run backend-audit --phase 6.4"   # Failing Tests Triage
```

## Tips

1. **Check progress anytime:**
   ```bash
   claude "run backend-audit --phase list"
   ```

2. **Auto-fix mode** — let Claude fix critical/major issues:
   ```bash
   claude "run backend-audit --phase 1.1 --fix"
   ```

3. **Single file audit** — useful for files you just changed:
   ```bash
   claude "run backend-audit --path Orbito.API/Controllers/PaymentController.cs"
   ```

4. **If a phase is too large**, Claude will tell you. Split it:
   ```bash
   claude "run backend-audit --phase 2.6a"  # First half of interfaces
   claude "run backend-audit --phase 2.6b"  # Second half
   ```

5. **Known issues from screenshots:**
   - `Orbito.Infrastructure/Persistance/` — typo in folder name (should be Persistence)
   - `PaymentRepository.cs.bak` exists — should be deleted
   - Both `Persistance/` and `Persistence/` folders exist — consolidate

## File Structure

```
.agents/skills/backend-audit/
├── SKILL.md                          # Main skill definition
├── audit-progress.json               # Progress tracker (updates per phase)
├── QUICKSTART.md                     # This file
├── scripts/
│   ├── architecture_check.py         # Clean Architecture violations
│   ├── build_warnings.py             # dotnet build warning parser
│   ├── code_metrics.py               # File/line counts, complexity
│   ├── dependency_scan.py            # NuGet package audit
│   ├── performance_scan.py           # Performance anti-patterns
│   ├── tenant_security_scan.py       # Multi-tenancy data leak detector
│   └── test_coverage_gaps.py         # Missing test file finder
└── references/
    └── backend-checklist.md          # Detailed heuristics per category
```
