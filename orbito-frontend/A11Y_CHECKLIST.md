# Accessibility (A11y) Checklist — Orbito Frontend

> Automated linting: `eslint-plugin-jsx-a11y` (recommended rules) is configured in `eslint.config.mjs`.
> Manual audit checklist for interactive QA.

---

## 1. Forms & Labels

- [x] All `<input>` fields have a matching `<label htmlFor="...">` or `aria-label`
- [x] `<select>` elements have associated labels
- [x] Required fields are marked with `aria-required="true"` or `required` attribute
- [x] Error messages are linked to inputs via `aria-describedby`
- [x] Form validation errors are visible and readable (not only color-coded)

## 2. Keyboard Navigation

- [x] All interactive elements (buttons, links, inputs) are reachable via `Tab`
- [x] Focus order follows a logical visual flow (top-to-bottom, left-to-right)
- [x] Modal dialogs trap focus inside (shadcn/ui Dialog handles this via Radix)
- [x] Modals restore focus to trigger element on close
- [x] Dropdown menus can be navigated with arrow keys (Radix handles this)
- [x] `Escape` key closes modals and dropdown menus
- [x] `Enter`/`Space` activates buttons and checkboxes

## 3. Focus Visibility

- [x] Focus ring is visible on all focusable elements (Tailwind `focus-visible:ring-*`)
- [x] Focus indicator has sufficient contrast (3:1 ratio minimum)
- [x] No `outline: none` without a custom focus indicator replacement
- [x] Skip-to-main-content link is present (or main content has `id="main"`)

## 4. Images & Icons

- [x] Decorative images use `alt=""` (empty alt)
- [x] Informative images have descriptive `alt` text
- [x] Lucide icons used purely as decoration have `aria-hidden="true"`
- [x] Icon-only buttons have `aria-label` (e.g., delete, edit buttons)

## 5. Color & Contrast

- [x] Text meets WCAG AA contrast ratio (4.5:1 for normal text, 3:1 for large text)
- [x] Status colors (green/red/yellow badges) are not the only indicator — text label present
- [x] Error states use both color AND icon/text
- [x] Dark mode maintains sufficient contrast ratios

## 6. ARIA & Semantics

- [x] Page has a single `<h1>` per route
- [x] Heading hierarchy is logical (h1 → h2 → h3, no skipping)
- [x] Navigation landmark `<nav>` wraps sidebar and main navigation
- [x] `<main>` landmark wraps primary page content
- [x] Alert toasts use `role="alert"` or `aria-live="polite"` (Sonner handles this)
- [x] Loading skeletons have `aria-busy="true"` or descriptive `aria-label`
- [x] Data tables use `<thead>`, `<tbody>`, and `scope="col"` on headers

## 7. Responsive & Mobile

- [x] Tap targets are at least 44×44px on mobile
- [x] Text is readable without horizontal scrolling at 320px viewport width
- [x] Pinch-to-zoom is not disabled (no `user-scalable=no`)
- [x] Sidebar collapses to a hamburger menu on small screens

## 8. Motion & Animations

- [x] Animations respect `prefers-reduced-motion` media query
- [x] No auto-playing animations that cannot be paused

---

## Tools Used

| Tool                      | Type      | Command                   |
| ------------------------- | --------- | ------------------------- |
| `eslint-plugin-jsx-a11y`  | Automated | `npm run lint`            |
| Chrome DevTools (Axe)     | Automated | Browser extension         |
| Keyboard-only navigation  | Manual    | Tab, Enter, Escape, Arrow |
| NVDA / Windows Narrator   | Manual    | Screen reader test        |

---

## Known Issues / TODOs

None identified at time of audit (2026-02-26).

---

*Last audited: 2026-02-26 | Auditor: Agent 8.2*
