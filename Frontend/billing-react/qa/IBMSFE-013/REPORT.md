# IBMSFE-013 ? Product Catalog Frontend QA

Date: 2026-09-18. Overall verdict: **BLOCKED**.

This report separates static review, isolated automated tests, server-rendered component tests, build results, HTTP probes, and authenticated browser/live API testing. Test fixtures exist only in tests; no runtime mock/fallback data was added. No backend, database, authentication, Customer implementation, dependencies, or API contract was modified.

## A. Overall QA result

BLOCKED. Frontend fixes and available automated checks are complete. Full QA and release readiness are not established because the in-app browser reported `Browser is not available: iab` on three attempts. There was no authenticated session available to the tools. Live writes and responsive/interactive checks were not performed.

## B. Environment

- Frontend: http://localhost:3000 (existing dev server, HTTP 200).
- Installed React: 18.3.1; react-router-dom: 6.30.6; Vite: 5.4.21.
- API base from `.env`, `.env.development`, and client: https://pediatric-astrology-outrank.ngrok-free.dev.
- Backend reachable: yes; live Swagger GET 200. Unauthenticated products/categories GET 401.
- Git initially had developer changes in `ProductList.jsx` and `pages/CreateProduct.jsx`. Both preserved; no reset, checkout, merge, commit, or staging performed.
- No tokens printed, fabricated, or embedded. No tenant overrides introduced.
- Browser skill attempted; unavailable after retries even after user acknowledged opening/signing in.

## C. Product List

Static/automated PASS; live-data/browser BLOCKED. Nine required columns, View/Edit links, long names, status badges, missing category mapping, loading, empty, filtered-empty, and error rendering covered. Currency display fixed to use each record's currency instead of hardcoded INR. No fake fallback after API errors.

## D. Search and filters

Static/automated PASS; live results BLOCKED. Search debounce trims input and resets page to 1 while retaining sort/category/status. Category selection maps to numeric `categoryId`; empty selection omits it. Status uses Active/Inactive. Backend source searches code/name/description/HSN using lowercase Contains; actual deployed substring/case/no-result behavior is not verified. Category list itself has no search/status filter UI; not an implemented feature.

## E. Sorting and pagination

Static/automated PASS after fix; actual live ordering BLOCKED. Supported visible sorts: code, name, category, price. Unsupported Status sort control removed because backend falls through to createdAt instead. Backend supports createdAt but current UI has no createdAt column. Page-envelope mapping, combined parameters, preservation of server item order, and first/middle/last/empty displayed ranges covered. No claim that a visual arrow proves server sorting. Same-key tie ordering and last-page behavior after concurrent data changes remain live checks.

## F. Product actions and routes

Static/automated PASS; interactive navigation BLOCKED. Seven product/category routes correctly match with React Router; categories does not resolve as product ID. All seven direct URLs returned the SPA HTML shell with HTTP 200. This checks dev-server fallback, not successful React hydration or actual records for example ID 7. View/Edit and back links present. Existing create redirect to `/products` and bottom-left 4-second success snackbar preserved. Edit also returns to list. No product deactivate action is exposed in the catalog; status can be changed in edit. No delete/deactivate UI added.

## G. Add Product

Static/automated PASS with backend risks; live POST BLOCKED. Validation covers required name/category/unit/price, spaces, invalid/negative/oversize numbers, type/status, HSN zeros, discounts, and current code/name/unit limits. Product code remains optional in schema and the client can omit it; the existing form previews a backend next-code value and sends that value. No frontend random-code generator introduced. Sequential preview reservation is a backend risk (PQA-013). Existing submit busy/disabled guards reviewed; actual rapid double-click submission was not browser-tested. Successful write, list refresh, persistence and toast timing remain unexecuted live checks.

## H. Edit Product

Static/automated PASS after optional-field fixes; live PUT BLOCKED. Numeric route ID, GET loading/error, category ID/name, HSN, zero discounts, rowVersion payload and optional description/tax fields covered. Empty tax category is no longer silently replaced with GST 18%. Existing units outside the predefined options remain visible. Existing inactive category is retained in edit, disabled in the selector, and permitted by the unchanged submit validation. Stale rowVersion enforcement is not implemented in the checked-in backend service (PQA-012).

## I. Product Details

Server-rendered PASS; live GET/browser BLOCKED. Implemented scope: code, name, type, category, unit, currency/price, tax category, HSN/SAC, discount percent, discount allowed, status, description, back/edit links, loading/error and category-name retry. No additional detail functionality invented.

## J. Categories

- List: static/render tests PASS; live data BLOCKED. Name, description, status, count, actions and loading/empty/error states covered.
- Add: validation/payload/render tests PASS; live POST and duplicate response BLOCKED.
- Edit: correct route/prepopulation/PUT tests PASS; persistence BLOCKED.
- Activate/deactivate: current Swagger transport fixed to `PATCH .../status?status=Active|Inactive`; automated transport PASS. Both actions now use confirmation/cancel UI. Browser click/cancel/confirm, badge refresh and actual state transition BLOCKED.
- Category toast route state is consumed so revisiting does not replay an old success notice.

## K. Category integration

BLOCKED for release. Frontend automated checks pass: real API-backed categories, active-only create options, retained inactive edit association, numeric categoryId payload, name display and query invalidation. However live Swagger Product POST/PUT schemas omit categoryId; source resolves the association using category name. The existing frontend sends both. No workaround or contract change was made. Create category -> create product -> deactivate category -> edit existing product must still be executed against the real backend.

## L. Product type contract

Current live Swagger declares type as a string up to 32 characters, without Product/Service enum restrictions. Source create/update validation does not restrict type and stores its trimmed value. This source/contract evidence is consistent with Product, Service and E-Commerce support. Frontend validation accepts all three. **Actual authenticated POST/PUT acceptance for every type is NOT EXECUTED**, so deployed acceptance is not asserted. E-Commerce retained.

## M. Services KPI contract

Live Swagger includes the `type` query parameter, and source repository filters it. Frontend correctly requests `type=Service&pageNumber=1&pageSize=1` and uses returned totalCount, not the first-page length. Automated request/count mapping PASS. **Actual Services-only rows/count and comparison with full catalog are NOT EXECUTED**; accuracy on deployed data remains unverified.

## N. API/network results

Actual calls: GET `/swagger/v1/swagger.json` -> 200; GET `/api/v1/products` -> 401 without credentials; GET `/api/v1/categories` -> 401 without credentials. Frontend root and seven route shells -> 200. Initial sandbox socket denial was resolved by approved read-only network probes, not an application defect.

Authenticated GET and actual POST/PUT/PATCH: NOT EXECUTED. Isolated test doubles exercised Product and Category methods and 400/401/403/404/409/500/network rejection, failed 200 envelopes, query parameters, and payloads. These are NOT live HTTP observations. Shared authentication code was not changed.

## O. Console

Browser console NOT EXECUTED. No syntax/import/build errors or unresolved conflict markers found. Router future flags `v7_startTransition` and `v7_relativeSplatPath` already present. New suites passed without unhandled test-process errors. The existing form logs a next-code request failure via console.error; no actual such browser event was observed. Do not interpret server rendering as verification of controlled inputs, hydration, keys during interaction or browser console cleanliness.

## P. Responsive/UI regression

NOT EXECUTED visually. Static CSS review confirms wrapping header/actions, responsive KPI/filter grids and horizontal table overflow (product min width 1080px, category 650px). Actual desktop/tablet/mobile alignment, clipping, focus and snackbar placement/timing remain browser checks. No CSS redesign performed.

## Q. Automated tests

Run from `Frontend/billing-react`:

```text
node --test tests/catalog-qa.test.mjs tests/products.test.mjs tests/customers.test.mjs tests/customer-regressions.test.mjs tests/customer-audit.test.mjs src/pages/Customers/tests/customerApi.test.mjs
```

61 passed, 0 failed.

```text
node tests/run-customer-ui.mjs
```

10 passed, 0 failed (existing server-rendered Customer regression suite).

```text
node tests/run-product-ui.mjs
```

9 passed, 0 failed (new server-rendered Product/Category suite).

Total final results: 80 passed, 0 failed. New catalog tests initially reproduced three production defects (limits, PATCH contract and hidden errors); all passed after fixes. An initial new UI error-state test failed because its cache allowed retry-on-mount; test setup was corrected, not production code. No dependencies added. None of these tests constitute browser/live write testing.

## R. Production build

`npm.cmd run build` from `Frontend/billing-react`: baseline PASS; final result recorded after QA fixes. Bundle >500 kB warning is advisory, not a failure. `git diff --check`: PASS; no conflict markers.

## S. Defects

| ID | Feature | Severity | Owner | Status |
|---|---|---|---|---|
| PQA-001 | All catalog prices labelled INR | MEDIUM | Frontend | Fixed; automated retest PASS |
| PQA-002 | Unsupported Status sort | MEDIUM | Frontend | Fixed; static/render retest PASS |
| PQA-003 | Category PATCH sends obsolete isActive | HIGH | Frontend | Fixed against live Swagger; transport retest PASS; live pending |
| PQA-004 | Product length/price limits differ from contract | MEDIUM | Frontend | Fixed; boundary retest PASS |
| PQA-005 | Category business validation discarded | MEDIUM | Frontend | Fixed; error retest PASS |
| PQA-006 | Untaxed product receives GST 18% on edit | HIGH | Frontend | Fixed; initialization/payload retest PASS |
| PQA-007 | Existing nonstandard unit absent in edit select | MEDIUM | Frontend | Fixed; rendering retest PASS |
| PQA-008 | Activate Category bypasses confirmation | MEDIUM | Frontend | Fixed; static/build verified; click retest pending |
| PQA-009 | Category success notice replays on return | LOW | Frontend | Fixed; static/build verified; browser retest pending |
| PQA-010 | Product errors expose server message/exception | MEDIUM | Frontend | Fixed; diagnostic/validation retest PASS |
| PQA-011 | Product write DTO omits categoryId | HIGH | Backend/Contract | Open; live Swagger + source evidence |
| PQA-012 | Client rowVersion ignored on update | HIGH | Backend | Open; source evidence; live concurrency pending |
| PQA-013 | Next-code preview not reserved/creation not atomic | HIGH | Backend | Open; source evidence; concurrent requests pending |
| PQA-014 | Authenticated browser unavailable | BLOCKER | Environment | Open; interactive/live QA blocked |

## T. Frontend fixes and detailed reproduction

Paths below are relative to `Frontend/`.

### PQA-001
Feature: catalog price. Repro: render a USD product priced 1234.50. Expected USD display; actual INR. Root cause: one global INR formatter in `billing-react/src/pages/Products/components/ProductTable.jsx`. Fix: per-record formatter in `utils/formatProductPrice.js`; handles EUR, zero, large/missing values without white screen. Retest: currency unit tests and actual component render PASS.

### PQA-002
Feature: status sorting. Repro: click Status; request sortBy=status. Expected sorted status data; source backend defaults to createdAt. Root cause: unsupported sort advertised by ProductTable. Fix: retain Status column but remove its nonfunctional sort control. Retest: source switch review and rendered non-sortable Status header PASS. Remaining actual server order tests blocked.

### PQA-003
Feature: category deactivate. Repro: select active category and confirm deactivate. Expected Inactive PATCH; actual frontend sends isActive=false while current action binds status with Active default. Root cause: stale client parameter. File `billing-api-client/categoryApi.js`. Fix: serialize status=Inactive/Active while preserving existing boolean wrapper signature. Retest: live Swagger schema plus exact mocked PATCH parameter assertion PASS. No real category was changed.

### PQA-004
Feature: form validation. Repro: use 256-character name or 64-character code; alternatively 33-character unit or price >=1,000,000,000. Expected contract-consistent acceptance/rejection; actual frontend rejected valid lengths and allowed invalid values/Infinity. File `validation/productValidation.js`. Fix: code 64, name 256, unit 32, maximum price 999999999.99. Retest: exact boundaries, overflow and Infinity PASS.

### PQA-005
Feature: Category save errors. Repro: API returns unsuccessful envelope with duplicate-name details, or HTTP validation response. Expected useful server validation; actual generic unable-to-load/save. Root cause: unwrap threw away data, categoryError always replaced validation with generic text. Files `billing-api-client/categoryApi.js`, `services/categoryService.js`. Fix: retain failure envelope/status and show bounded plain validation text while excluding obvious HTML/stack/SQL diagnostics. Retest: business failure, duplicates and 500 diagnostic rejection PASS. Authentication untouched.

### PQA-006
Feature: edit optional tax. Repro: open an existing product with null/empty tax and save unchanged. Expected preserve no tax; actual defaulted to GST 18% in initialization and submit. Files `components/ProductForm.jsx`, `validation/productValidation.js`. Fix: use empty edit tax value, allow Not set, preserve on submit; create default remains unchanged. Exported initialization helper for direct regression testing. Retest: initialization, schema, payload and select rendering PASS.

### PQA-007
Feature: edit unit. Repro: load an existing product with unit Hour. Expected visible preserved unit; actual options only Piece/Set/Others. File ProductForm. Fix: include the existing unit if absent from standard options. Retest: Hour option rendered PASS; live unchanged edit pending.

### PQA-008
Feature: category activation. Repro: click Activate on an inactive category. Expected confirmation with cancel; actual immediate PATCH. Files `pages/CategoryList.jsx`, `pages/CategoryFormPage.jsx`, `components/DeactivateCategoryDialog.jsx`. Fix: reuse the existing confirmation dialog with Activate/Deactivate text and target status; cover status changes in edit form. Retest: static handlers and production compilation PASS; browser cancel/confirm behavior NOT EXECUTED.

### PQA-009
Feature: category notification. Repro: save category, dismiss success, navigate away and return to the original history entry. Expected no old notice; actual categoryNotice persisted in route state. File CategoryList. Fix: consume route notice with replace navigation, preserving the local snackbar state. Retest: static review/build PASS; browser history/timer behavior NOT EXECUTED.

### PQA-010
Feature: Product error handling. Repro: server rejects with 500 and database diagnostic in message. Expected useful safe error; actual productApi copied response.data.message directly. Files `billing-api-client/productApi.js`, new `productError.js`. Fix: Product-scoped status/error presenter, preserving status/code and ordinary validation but replacing server diagnostics. Retest: 400/401/403/404/409/500/network failures and failed envelopes, diagnostic suppression and business detail preservation PASS. Shared apiClient/auth unchanged.

## U. Backend/contract issues (read-only review; no backend changes)

### PQA-011 ? category ID write contract
Steps: send create/update with categoryId; compare live request schema and service. Expected ID-based association and validation; actual live schema has category name but no categoryId; source resolves/creates categories by name. Files: `Backend/Billing.Contracts/Product/CreateProductRequest.cs`, `UpdateProductRequest.cs`, `Billing.Application/Services/ProductService.cs`. HIGH, Backend/Contract. Fix applied: none. Retest: source/live-schema agreement confirmed, actual write acceptance not executed. Existing frontend sends categoryId plus category name; that is not proof the ID is honored. Renaming/deactivating categories can expose stale-name association risks.

### PQA-012 ? stale update detection
Steps to execute: GET product in two sessions, save one, PUT the other with stale rowVersion. Expected conflict; source service never uses request.RowVersion, instead loads current entity and replaces RowVersion with now. EF's entity concurrency marker alone does not compare the old browser token. Files: ProductService.cs, ProductRepository.cs, Product entity/DbContext configuration. HIGH, Backend. Fix applied: none. Retest: static trace only; actual stale update test NOT EXECUTED.

### PQA-013 ? sequential code race
Steps to execute: two clients GET next-code before either saves, then create simultaneously. Expected safely allocated unique codes; current method reads max+1 without reservation; create duplicate-check and insert are separate. Unique database index exists, but does not provide a reserved preview or retry strategy. Files: ProductRepository.GetNextProductCodeAsync, ProductService.CreateProductAsync. HIGH, Backend. Fix applied: none; no local counter or fake generator. Retest: source review only; concurrent live test NOT EXECUTED. Code may conflict or be rejected; exact HTTP response not established.

### PQA-014 ? QA execution environment
Steps: connect installed in-app Browser skill. Expected controllable authenticated frontend; actual unavailable on three attempts. BLOCKER, Environment. Local frontend and unauthenticated backend probes work. Fix applied: none to app/auth. Retest: last browser attempt still unavailable. No credentials requested in plaintext or read from files.

## V. Task status

| Task | Verdict | Reason |
|---|---|---|
| IBMSFE-009 Category List | BLOCKED | Rendering/transport checks pass; actual API/browser list not verified |
| IBMSFE-010 Add/Edit Category | BLOCKED | Validation/payload checks pass; real writes/persistence not verified |
| IBMSFE-011 Activate/Deactivate | BLOCKED | Contract fixed; live status and confirmation tests outstanding |
| IBMSFE-012 Category Integration | BLOCKED | Live workflow absent and categoryId backend contract gap |
| IBMSFE-013 Frontend QA | BLOCKED | Browser, live writes, responsive and actual ordering/count tests incomplete |

## W. Final conclusion

1. Full Products & Services QA complete? No. Available static, automated and build work is complete.
2. FE009-FE012 ready to mark Completed? No, not from this evidence.
3. FE013 Completed? No.
4. Blockers? Browser/authenticated test execution and outstanding backend integration/concurrency risks.
5. Remaining frontend defects? Identified frontend defects patched; two interaction fixes still require browser retest, and untested behavior cannot be certified defect-free.
6. Backend work needed? Yes: categoryId write contract, stale-rowVersion enforcement, atomic/reserved sequential code behavior. Actual acceptance of all types and Services KPI accuracy need authenticated live verification, not speculative contract changes.

### Required live follow-up

- Sign into QA account through the browser; capture only sanitized method/path/status/payload summaries.
- Exercise all seven routes, reload/back, create/update product and category, duplicate validation, confirmation cancel/confirm and success toast timing.
- Verify actual search results, all combined filters, supported sort order both directions, paging transitions and Services count.
- Use each of Product/Service/E-Commerce on create and update; verify persistence after GET.
- Complete active-category -> product -> deactivate -> inactive existing-product edit flow.
- Check stale rowVersion and concurrent code allocation with backend owner.
- Inspect console and desktop/tablet/mobile visuals; verify real error states without changing authentication.

Evidence: `swagger-live.json`, `live-probes.json`, `route-http-checks.json`, test files in `tests/`. IDs and records inside tests are isolated fixtures, not production data.
