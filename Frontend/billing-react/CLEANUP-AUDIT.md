# Frontend cleanup audit ? 2026-09-17

## Safety baseline
Existing developer changes were preserved. Before cleanup, eight tracked files were modified and product-list.css was untracked. No reset, checkout, backend edit, or API-contract edit was performed.

```text
 M Frontend/billing-react/src/pages/Customers/styles/customer-list.css
 M Frontend/billing-react/src/pages/Products/ProductList.jsx
 M Frontend/billing-react/src/pages/Products/components/ProductForm.jsx
 M Frontend/billing-react/src/pages/Products/components/ProductSummaryCards.jsx
 M Frontend/billing-react/src/pages/Products/components/ProductTable.jsx
 M Frontend/billing-react/src/pages/Products/styles/products.css
 M Frontend/billing-react/src/pages/Products/validation/productValidation.js
 M Frontend/billing-react/tests/products.test.mjs
?? Frontend/billing-react/.cleanup-baseline/
?? Frontend/billing-react/src/pages/Products/styles/product-list.css

```
Baseline: 41 Node tests passed; 10 Customer UI rendering tests passed; production build passed (existing chunk-size warning). package.json offers dev/build/preview; no lint or test npm script. Shared frontend packages have no scripts.

## Classification before removal
| Finding | Classification | Decision / evidence |
| --- | --- | --- |
| Products/ProductRoutePlaceholder.jsx | SAFE TO REMOVE | No JS/JSX/MJS references outside its declaration; active AppRoutes uses ProductDetails/CreateProduct/EditProduct. |
| Products/data/mockProducts.js | SAFE TO REMOVE | No runtime, test, configuration or demo code references; Product service uses shared productApi. Historical DELIVERY excerpts only. |
| Three unused imports | SAFE TO REMOVE | Babel scope bindings report no references: CreateInvoice InvoiceBillingLogo; CustomerForm GroupsOutlined; Register Check. React compatibility imports retained. |
| Four repeated product-switch selectors | SAFE TO CONSOLIDATE | Root-level declarations have no intervening competing rules; merge additions into original definitions. |
| Product 96% versus list 100% width | SAFE TO CONSOLIDATE | Explicitly exclude the list from legacy compact width; retain latest list alignment. |
| CustomerList/CustomerDetails | KEEP | Exported from Customers/index.js; historical implementations retained. |
| Customers/LegacyEditCustomer | NEEDS TEAM CONFIRMATION | Unrouted historical screens with distinct behavior/ownership. No deletion merely from absence of routes. |
| useAuthNav compatibility re-export | KEEP | components/index.js exports it; active auth pages import via billing-react alias. |
| API adapters / shared contracts | KEEP | Feature adapters provide mapping/error handling; shared transport and contract consumers/tests remain active. |
| Dashboard/billing mock/sample data | KEEP | Other modules still consume local/demo stores; real Product integration does not prove these obsolete. |
| ModulePlaceholder | KEEP | Explicit active routes for unfinished modules. |
| Product DELIVERY | SAFE TO CONSOLIDATE | Add current status banner; preserve historical implementation notes. |
| node_modules / dist / .vite | GENERATED FILE | 42,226 tracked dependency files (including 32 .vite), two additional top-level .vite files and two dist files; untrack only, preserve disk copies. |
| .env / .env.development | SAFE TO CONSOLIDATE | Only VITE_API_BASE_URL; no token/password/private-key keys detected. Preserve local values, untrack, add example. |
| Customer layered CSS | NEEDS TEAM CONFIRMATION | Multiple historical/theme/responsive overrides; broad flattening without browser computed-style evidence is unsafe. |
| Form button !important | KEEP | Existing MUI specificity overrides; no safe computed-style proof to remove them. |
| Product list scoped overrides | KEEP | Intentional differentiation from shared category/form/detail styles; not blind duplicates. |
| Global index.css element rules | KEEP | Intentional application reset/font/focus styles; module CSS uses named classes. |

## Routes and architecture
No duplicate path declarations found. /invoices/new and /invoices/create are intentional aliases, retained. React Router static Product categories/new routes coexist with :id routes. Customer routes use CustomerListPage, CreateCustomer, CustomerDetailsPage and EditCustomer. Product routes and all bindings remain unchanged.

UI ? feature hooks/services ? shared billing-api-client remains in place. Customer feature API layers also retain their existing transport/error adapters. Endpoint URLs, request payloads, response mapping, headers, tenant/auth behavior and contracts are unchanged.

## Remaining risks / deferred work
No full browser interaction or computed-style comparison performed. Existing Customer UI tests are server-rendered, not browser end-to-end tests. Live backend mutation tests intentionally not performed. Keep historical Customer screens, barrels, CSS themes and API layers until owners approve broader consolidation. Existing bundle exceeds Vite's chunk warning threshold; no code splitting introduced solely for cleanup.

## File changes
- Removed src/pages/Products/ProductRoutePlaceholder.jsx and src/pages/Products/data/mockProducts.js with zero code consumers.
- src/pages/CreateInvoice/CreateInvoice.jsx: unused InvoiceBillingLogo import removed.
- src/pages/Customers/components/CustomerForm.jsx: unused GroupsOutlined import removed.
- src/pages/Register/Register.jsx: unused Check import removed.
- src/pages/Products/styles/product-form.css: consolidated .product-switch-field, .product-switch-info, .product-switch-title, .product-switch-desc declarations. Existing animation and reduced-motion handling retained.
- src/pages/Products/styles/products.css: legacy 96% panel rule excludes .product-list-page; removed obsolete shared summary width selector. Product List remains 100% and left/right aligned.
- src/pages/Products/DELIVERY.md: current integration banner above historical notes.
- ../.gitignore: frontend-wide generated-output/local-environment exclusions, example exception.
- .env.example: preserved current non-secret API-base configuration for onboarding; local .env/.env.development remain on disk and ignored.
- CLEANUP-AUDIT.md and cleanup-inventory.json: baseline, classification, verification and retained CSS inventory.

## Environment setup
Copy .env.example to .env when setting up a new checkout. Existing local .env and .env.development were preserved with their original values. No API URL or authentication configuration was changed. Vite client environment variables are public configuration, not a place for credentials.

## Exact verification commands
Run from Frontend/billing-react unless specified otherwise:

```powershell
node --test tests/products.test.mjs tests/customers.test.mjs tests/customer-regressions.test.mjs src/pages/Customers/tests/customerApi.test.mjs
node tests/run-customer-ui.mjs
npm.cmd run build -- --outDir .cleanup-baseline
npm.cmd run build -- --outDir .cleanup-final
```

Baseline: Node 41/41 (Customer 36, Product 5); UI 10/10; build PASS.
Final: Node 41/41; UI/build results recorded after completion below.
The build uses temporary output directories so tracked/local dist content is not overwritten. Sandbox escalation is needed for esbuild's parent-directory access on this Windows installation; this is an environment permission constraint, not a source failure.
Static parse/import audit: 115 scripts, 17 stylesheets; no unresolved static local/alias imports or real merge markers. 71 repeated-selector occurrences recorded and retained in cleanup-inventory.json. No new lint dependency installed; no existing lint command is configured.

## Final verification result
- Overall: PASS WITH WARNINGS.
- Customer automated regression: PASS (36 Node tests + 10 server-rendered UI tests).
- Product automated regression: PASS (5 Node tests).
- Final production build: PASS; existing large-chunk warning remains.
- Exact duplicate source hash groups: none.
- Browser smoke / live backend interaction: NOT EXECUTED (no browser automation tool exposed in this session). No real customer/product data mutated.
- Source functionality preserved by focused diff and passing checks; browser behavior is not independently certified.
- Cleanup intentionally skips uncertain historical screens, unused-export guesses, API consolidation, broad CSS flattening and MUI priority changes.

## Final Git hygiene
42,232 index-only deletions: 42,226 node_modules files (including 32 nested Vite cache files), two top-level .vite files, two dist files, and two local environment files. All local directories/configuration remain present. Zero generated dependency/build/cache files remain tracked under Frontend. New ignore/example/audit files and source cleanup remain unstaged for review; no commit or push performed. No backend or shared API/contract diffs.
