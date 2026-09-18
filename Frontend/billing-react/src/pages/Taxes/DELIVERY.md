## Current demo mode update

The user subsequently requested smaller cards and mock data. Tax pages now default to an explicitly labeled demo mode with six sample rules, derived summaries, filters, and in-memory create/edit support. Demo changes reset on refresh and never call the backend. Set `VITE_TAX_DEMO_MODE=false` and restart/rebuild to restore the live service (still blocked by its unpublished backend contract). Demo and live query cache keys are separate. Added `taxDemoService.js`; compacted `TaxSettings.css` and updated `Taxes.jsx`. Demo list/create/edit/copy-isolation checks passed.

The original delivery notes below describe the earlier live-only implementation.
# Tax Settings delivery

Scope: IBMSFE-001 through IBMSFE-004, frontend only. Existing `/taxes/*` route and navigation are preserved. No stored records were deleted; the previous tax screen contained only an empty placeholder.

- IBMSFE-001: Coffee-themed table, derived summary, filters, skeleton, empty/error/retry states and responsive CSS implemented. Live list display is blocked by the missing response contract.
- IBMSFE-002: Separate `/taxes/new` and `/taxes/:id/edit` pages follow existing product/customer patterns. Required fields, percentage, priority and date validation, submission lock, success notification and refresh lifecycle are implemented. Persistence/edit loading remain blocked by the backend.
- IBMSFE-003: Inclusive/exclusive, intra/inter-state GST, component tax, VAT and custom preview implemented. Preview uses INR and rounds to cents, keeping split components equal to total tax. Backend remains authoritative.
- IBMSFE-004: GET and PUT transport use the existing shared API client and authentication. Integration is NOT complete: live GET returned HTTP 404 on 2026-09-18, and live Swagger has no tax settings GET/PUT endpoint or DTO. PUT was deliberately not sent with an invented payload. The save service reports a clear blocker. No fake data or local persistence is used.

## Backend handoff

Publish GET/PUT `/api/v1/settings/taxes` with response shape, writable properties, tax IDs/enums, bulk-vs-single update semantics, and concurrency/version requirements. Then implement the `taxService.list` mapping and `taxService.save` payload using the existing `taxApi` transport. The UI model in `taxModel.js` is a frontend model, not a claimed backend DTO. Preserve existing records according to the confirmed PUT semantics.

## Files

Modified: `Taxes.jsx`.
Created: `TaxSettings.css`, `taxModel.js`, `taxModel.test.mjs`, `../../services/taxService.js`, this delivery note.

## Checks

Run `node --test Frontend/billing-react/src/pages/Taxes/taxModel.test.mjs` and `npm.cmd run build` from the repository root. No packages were added. Browser console, interactive form behavior and visual viewport checks require an available browser session; do not infer these from the build.

Verification results (2026-09-18): all 5 Node tests passed. Vite started successfully at http://127.0.0.1:3000. HTTP requests to `/taxes`, `/taxes/new`, and the transformed Tax JSX module returned 200. These are serving/transformation checks, not browser interaction checks. `git diff --check` passed.
Production build passed (Vite, 2m 29s). Vite reported the existing application bundle size warning (>500 kB); no build errors.
