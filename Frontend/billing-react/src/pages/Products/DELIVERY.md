# Products & Services — frontend delivery

Implemented IBMSFE-001 through IBMSFE-004 under `src/pages/Products`, using the existing Material UI dependencies, React Query provider, dashboard layout and Products & Services sidebar link.

## Routes

- `/products`: product catalog
- `/products/new`: Add Product placeholder
- `/products/:id`: View Product placeholder
- `/products/:id/edit`: Edit Product placeholder

Only `src/routes/AppRoutes.jsx` was modified outside the new module. Product forms, backend APIs and other modules are outside this implementation.

## Behavior and integration

The service exposes `getProducts(params, { signal })` and `getCatalogMetadata({ signal })`. Replace the `productService` export with a real API adapter with the same contract when available. Metadata supplies categories and unfiltered summary counts separately from paginated results.

Search trims whitespace, matches the beginning of product-name words or product codes case-insensitively, and waits 350 ms before requesting data. Category and status combine with search. Search, filters, sorting and page-size changes reset pageNumber to 1. Clear Filters clears search/category/status while preserving sort and page size.

The mock service filters and sorts all records before slicing a page. Query parameters are search, category, status, pageNumber, pageSize, sortBy and sortOrder. Results include items, pageNumber, pageSize, totalCount and totalPages. Sortable fields are productCode, name, category, price and status. Currency uses en-IN INR formatting with two decimal places.

React Query handles loading, errors and cancellation. The table shows seven skeleton rows while loading, distinct empty/filtered-empty states, and a retry action for errors. View and Edit use directly visible, keyboard-accessible icon links with tooltips. Table headers expose sort direction. Filters stack at smaller widths and the table scrolls within its container.

The 32 mock records include seven categories, both types/statuses, seven units and all requested tax categories. No real API requests are made by this module.

## Run

No dependencies need installing in the existing checkout.

```powershell
npm.cmd run dev
```

Open the existing app and choose Products & Services, or navigate to `/products`.

## Validation

Service checks passed for combined case-insensitive/trimmed search and filters, all five sort fields in both directions, pagination and page clamping, empty datasets/results, metadata and abort handling. Browser visual verification was not performed.

## Created module files / folder structure
- `src/pages/Products/components/ProductActionsMenu.jsx`
- `src/pages/Products/components/ProductFilters.jsx`
- `src/pages/Products/components/ProductPagination.jsx`
- `src/pages/Products/components/ProductStates.jsx`
- `src/pages/Products/components/ProductSummaryCards.jsx`
- `src/pages/Products/components/ProductTable.jsx`
- `src/pages/Products/data/mockProducts.js`
- `src/pages/Products/ProductList.jsx`
- `src/pages/Products/ProductRoutePlaceholder.jsx`
- `src/pages/Products/services/productService.js`
- `src/pages/Products/styles/products.css`

## Complete source code

### Frontend/billing-react/src/pages/Products/components/ProductActionsMenu.jsx

```jsx
import { Link } from 'react-router-dom';
import { IconButton, Tooltip } from '@mui/material';
import { EditOutlined, VisibilityOutlined } from '@mui/icons-material';

export function ProductActionsMenu({ product }) {
  return <div className="product-row-actions" role="group" aria-label={`Actions for ${product.name}`}>
    <Tooltip title="View Product"><IconButton component={Link} to={`/products/${encodeURIComponent(product.id)}`} size="small" className="product-action-view" aria-label={`View ${product.name}`}><VisibilityOutlined fontSize="small" /></IconButton></Tooltip>
    <Tooltip title="Edit Product"><IconButton component={Link} to={`/products/${encodeURIComponent(product.id)}/edit`} size="small" className="product-action-edit" aria-label={`Edit ${product.name}`}><EditOutlined fontSize="small" /></IconButton></Tooltip>
  </div>;
}
```

### Frontend/billing-react/src/pages/Products/components/ProductFilters.jsx

```jsx
import { Button, IconButton, InputAdornment, MenuItem, TextField } from '@mui/material';
import { Close, Search } from '@mui/icons-material';

export function ProductFilters({ search, onSearch, params, onChange, categories, active, onClear }) {
  return <div className="product-filters">
    <TextField className="product-search" label="Search products" placeholder="Search by product code or product name..." size="small" value={search} onChange={event => onSearch(event.target.value)} InputProps={{
      startAdornment: <InputAdornment position="start"><Search fontSize="small" /></InputAdornment>,
      endAdornment: search ? <InputAdornment position="end"><IconButton size="small" aria-label="Clear search" onClick={() => onSearch('')}><Close fontSize="small" /></IconButton></InputAdornment> : null,
    }} />
    <TextField select label="Category" size="small" value={params.category} onChange={event => onChange({ category: event.target.value })}>
      <MenuItem value="">All Categories</MenuItem>{categories.map(category => <MenuItem key={category} value={category}>{category}</MenuItem>)}
    </TextField>
    <TextField select label="Status" size="small" value={params.status} onChange={event => onChange({ status: event.target.value })}>
      <MenuItem value="">All Status</MenuItem><MenuItem value="Active">Active</MenuItem><MenuItem value="Inactive">Inactive</MenuItem>
    </TextField>
    <Button disabled={!active} onClick={onClear}>Clear Filters</Button>
  </div>;
}
```

### Frontend/billing-react/src/pages/Products/components/ProductPagination.jsx

```jsx
import { MenuItem, Pagination, PaginationItem, TextField } from '@mui/material';

export function ProductPagination({ data, onPage, onPageSize, disabled = false }) {
  const { pageNumber, pageSize, totalCount, totalPages } = data;
  const first = totalCount ? (pageNumber - 1) * pageSize + 1 : 0;
  const last = Math.min(pageNumber * pageSize, totalCount);
  return <footer className="product-pagination">
    <span role="status">Showing {first}–{last} of {totalCount} products</span>
    <TextField disabled={disabled} select size="small" label="Rows per page" value={pageSize} onChange={event => onPageSize(Number(event.target.value))}>{[10, 20, 50].map(size => <MenuItem key={size} value={size}>{size}</MenuItem>)}</TextField>
    <Pagination disabled={disabled} aria-label="Product pages" count={Math.max(1, totalPages)} page={pageNumber} onChange={(_, page) => onPage(page)} shape="rounded" color="primary" renderItem={item => <PaginationItem {...item} slots={{ previous: () => <span>Previous</span>, next: () => <span>Next</span> }} />} />
  </footer>;
}
```

### Frontend/billing-react/src/pages/Products/components/ProductStates.jsx

```jsx
import { Button } from '@mui/material';
import { Inventory2Outlined, ErrorOutline } from '@mui/icons-material';
import { Link } from 'react-router-dom';

export function ProductErrorState({ onRetry }) {
  return <div className="product-state" role="alert"><ErrorOutline /><h2>Unable to load products</h2><p>Something went wrong while loading the product catalog.</p><Button variant="outlined" onClick={onRetry}>Try Again</Button></div>;
}

export function ProductEmptyState({ filtered, onClear }) {
  return <div className="product-state"><Inventory2Outlined /><h2>{filtered ? 'No matching products' : 'No products found'}</h2><p>{filtered ? 'Try changing your search or filters.' : 'Products and services you create will appear here.'}</p>
    {filtered ? <Button variant="outlined" onClick={onClear}>Clear Filters</Button> : <Button component={Link} to="/products/new" variant="contained">+ Add Product</Button>}
  </div>;
}
```

### Frontend/billing-react/src/pages/Products/components/ProductSummaryCards.jsx

```jsx
import { Skeleton } from '@mui/material';
import { Inventory2Outlined, CheckCircleOutline, PauseCircleOutline, DesignServicesOutlined } from '@mui/icons-material';

const cards = [['total', 'Total Products', Inventory2Outlined], ['active', 'Active Products', CheckCircleOutline], ['inactive', 'Inactive Products', PauseCircleOutline], ['services', 'Services', DesignServicesOutlined]];

export function ProductSummaryCards({ summary }) {
  return <section className="product-summary" aria-label="Catalog summary">{cards.map(([key, label, Icon]) => <div className={`product-summary-card tone-${key}`} key={key}><span className="product-summary-icon"><Icon /></span><div><span>{label}</span><strong>{summary ? summary[key] : <Skeleton width={45} />}</strong><small>{{ total: 'Across your catalog', active: 'Available for billing', inactive: 'Currently unavailable', services: 'Expertise & subscriptions' }[key]}</small></div></div>)}</section>;
}
```

### Frontend/billing-react/src/pages/Products/components/ProductTable.jsx

```jsx
import { Skeleton, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TableSortLabel, Tooltip } from '@mui/material';
import { ProductActionsMenu } from './ProductActionsMenu';
import { ProductEmptyState, ProductErrorState } from './ProductStates';
import { Inventory2Outlined, DesignServicesOutlined } from '@mui/icons-material';
import { Link } from 'react-router-dom';

const columns = [
  ['productCode', 'Product Code', true], ['name', 'Product Name', true], ['type', 'Type'],
  ['category', 'Category', true], ['unit', 'Unit'], ['price', 'Price', true],
  ['taxCategory', 'Tax Category'], ['status', 'Status', true], ['actions', 'Actions'],
];
const currency = new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', minimumFractionDigits: 2 });

export function ProductTable({ items, loading, error, params, onSort, filtered, onClear, onRetry }) {
  return <TableContainer className="product-table" tabIndex={0} aria-label="Scrollable product catalog" aria-busy={loading}>
    <Table aria-label="Products and services catalog" size="small">
      <TableHead><TableRow>{columns.map(([key, label, sortable]) => <TableCell key={key} align={key === 'price' ? 'right' : 'left'} sortDirection={params.sortBy === key ? params.sortOrder : false}>
        {sortable ? <TableSortLabel active={params.sortBy === key} direction={params.sortBy === key ? params.sortOrder : 'asc'} onClick={() => onSort(key)}>{label}</TableSortLabel> : label}
      </TableCell>)}</TableRow></TableHead>
      <TableBody>{loading ? Array.from({ length: 7 }, (_, row) => <TableRow key={row}>{columns.map(([key]) => <TableCell key={key}><Skeleton height={28} /></TableCell>)}</TableRow>)
        : error ? <TableRow><TableCell colSpan={9}><ProductErrorState onRetry={onRetry} /></TableCell></TableRow>
        : !items.length ? <TableRow><TableCell colSpan={9}><ProductEmptyState filtered={filtered} onClear={onClear} /></TableCell></TableRow>
        : items.map(product => <TableRow hover key={product.id}>
          <TableCell><span className="product-code">{product.productCode}</span></TableCell>
          <TableCell><div className="product-identity"><span className={`product-row-icon ${product.type.toLowerCase()}`} aria-hidden="true">{product.type === 'Service' ? <DesignServicesOutlined fontSize="small" /> : <Inventory2Outlined fontSize="small" />}</span><Tooltip title={product.name}><Link to={`/products/${encodeURIComponent(product.id)}`} className="product-name">{product.name}</Link></Tooltip></div></TableCell>
          <TableCell><span className={`product-badge ${product.type.toLowerCase()}`}>{product.type}</span></TableCell>
          <TableCell><Tooltip title={product.category}><span className="product-category">{product.category}</span></Tooltip></TableCell>
          <TableCell>{product.unit}</TableCell><TableCell align="right" className="product-price">{currency.format(product.price)}</TableCell>
          <TableCell>{product.taxCategory}</TableCell><TableCell><span className={`product-badge ${product.status.toLowerCase()}`}>{product.status}</span></TableCell>
          <TableCell><ProductActionsMenu product={product} /></TableCell>
        </TableRow>)}
      </TableBody>
    </Table>
  </TableContainer>;
}
```

### Frontend/billing-react/src/pages/Products/data/mockProducts.js

```js
const records = [
  ['Web Development', 'Service', 'IT Services', 'Project', 25000, 18],
  ['Business Laptop', 'Product', 'Electronics', 'Piece', 55000, 18],
  ['Website Maintenance', 'Service', 'Maintenance', 'Month', 5000, 18],
  ['Wireless Keyboard', 'Product', 'Electronics', 'Piece', 1850, 18],
  ['USB-C Docking Station', 'Product', 'Electronics', 'Unit', 7500, 18],
  ['27-inch Monitor', 'Product', 'Electronics', 'Piece', 18900, 18],
  ['Laser Printer', 'Product', 'Electronics', 'Unit', 22500, 18],
  ['Office Paper A4', 'Product', 'Office Supplies', 'Unit', 350, 12],
  ['Executive Notebook', 'Product', 'Office Supplies', 'Piece', 180, 12],
  ['Desk Organizer', 'Product', 'Office Supplies', 'Piece', 650, 18],
  ['Printed Training Manual', 'Product', 'Office Supplies', 'Piece', 450, 0],
  ['Pantry Tea Pack', 'Product', 'Office Supplies', 'Unit', 280, 5],
  ['Accounting Software License', 'Product', 'Software', 'Year', 12000, 18],
  ['Antivirus License', 'Product', 'Software', 'Year', 1800, 18],
  ['Design Suite License', 'Product', 'Software', 'Year', 32000, 18],
  ['Database Management License', 'Product', 'Software', 'Unit', 45000, 18],
  ['API Integration', 'Service', 'IT Services', 'Project', 35000, 18],
  ['Cloud Migration', 'Service', 'IT Services', 'Project', 85000, 18],
  ['Technical Support', 'Service', 'IT Services', 'Hour', 1200, 18],
  ['Business Process Consulting', 'Service', 'Consulting', 'Day', 15000, 18],
  ['Financial Reporting Advisory', 'Service', 'Consulting', 'Hour', 3500, 18],
  ['Staff Training Workshop', 'Service', 'Consulting', 'Day', 18000, 'Exempt'],
  ['Network Maintenance', 'Service', 'Maintenance', 'Month', 8500, 18],
  ['Printer Maintenance', 'Service', 'Maintenance', 'Year', 6000, 18],
  ['Hardware Inspection', 'Service', 'Maintenance', 'Unit', 950, 18],
  ['Cloud Hosting', 'Service', 'Subscription', 'Month', 2499, 18],
  ['Business Email', 'Service', 'Subscription', 'Month', 299, 18],
  ['Secure Backup Storage', 'Service', 'Subscription', 'Year', 9999, 18],
  ['CRM Subscription', 'Service', 'Subscription', 'Month', 1499, 18],
  ['Office Air Conditioner', 'Product', 'Electronics', 'Unit', 42000, 28],
  ['Accessibility Review', 'Service', 'Consulting', 'Project', 22000, 18],
  ['Enterprise Application Performance Assessment', 'Service', 'IT Services', 'Project', 48000, 18],
];

export const mockProducts = records.map(([name, type, category, unit, price, tax], index) => Object.freeze({
  id: String(index + 1),
  productCode: `PRD-${String(index + 1).padStart(3, '0')}`,
  name, type, category, unit, price,
  taxCategory: tax === 'Exempt' ? tax : `GST ${tax}%`,
  status: index % 5 === 2 ? 'Inactive' : 'Active',
}));
```

### Frontend/billing-react/src/pages/Products/ProductList.jsx

```jsx
import { useEffect, useState } from 'react';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Breadcrumbs, Button } from '@mui/material';
import { Add, Inventory2Outlined } from '@mui/icons-material';
import { productService } from './services/productService';
import { ProductFilters } from './components/ProductFilters';
import { ProductTable } from './components/ProductTable';
import { ProductPagination } from './components/ProductPagination';
import { ProductSummaryCards } from './components/ProductSummaryCards';
import './styles/products.css';

const initialParams = { search: '', category: '', status: '', pageNumber: 1, pageSize: 10, sortBy: 'productCode', sortOrder: 'asc' };

export function ProductList() {
  const [search, setSearch] = useState('');
  const [params, setParams] = useState(initialParams);
  useEffect(() => {
    // Accept even one character; restart the delay on every keystroke.
    const timer = setTimeout(() => setParams(previous => previous.search === search.trim() ? previous : { ...previous, search: search.trim(), pageNumber: 1 }), search.trim() ? 350 : 0);
    return () => clearTimeout(timer);
  }, [search]);
  const catalog = useQuery({ queryKey: ['products', 'metadata'], queryFn: ({ signal }) => productService.getCatalogMetadata({ signal }), staleTime: 60000 });
  const query = useQuery({ queryKey: ['products', 'list', params], queryFn: ({ signal }) => productService.getProducts(params, { signal }), placeholderData: keepPreviousData });
  const change = patch => setParams(previous => ({ ...previous, ...patch, pageNumber: 1 }));
  const clear = () => { setSearch(''); setParams(previous => ({ ...previous, search: '', category: '', status: '', pageNumber: 1 })); };
  const active = Boolean(search || params.search || params.category || params.status);
  const loading = query.isPending || catalog.isPending;
  const updating = search.trim() !== params.search || query.isFetching;
  const error = query.isError || catalog.isError;

  return <main className="product-page">
    <Breadcrumbs aria-label="Breadcrumb"><span>Products &amp; Services</span><span>Product List</span></Breadcrumbs>
    <header className="product-heading"><div><span className="product-eyebrow">YOUR BILLING CATALOG</span><h1>Products &amp; Services</h1><p>Manage products and services used for billing and invoicing.</p></div><Button component={Link} to="/products/new" variant="contained" startIcon={<Add />}>Add Product</Button></header>
    <ProductSummaryCards summary={catalog.data?.summary} />
    <section className="product-panel" aria-label="Product list">
      <div className="product-panel-heading"><div className="product-panel-title"><span className="product-panel-icon"><Inventory2Outlined fontSize="small" /></span><div><h2>Product catalog</h2><p>Everything you bill, organized in one place.</p></div></div><span className="product-result-count" role="status">{loading ? 'Loading catalog…' : error ? 'Catalog unavailable' : updating ? 'Updating results?' : `${query.data?.totalCount ?? 0} ${active ? 'matching ' : ''}items`}</span></div>
      <ProductFilters search={search} onSearch={setSearch} params={params} onChange={change} categories={catalog.data?.categories || []} active={active} onClear={clear} />
      <ProductTable items={query.data?.items || []} loading={loading} error={error} params={params} onSort={sortBy => change({ sortBy, sortOrder: params.sortBy === sortBy && params.sortOrder === 'asc' ? 'desc' : 'asc' })} filtered={Boolean(params.search || params.category || params.status)} onClear={clear} onRetry={() => { query.refetch(); catalog.refetch(); }} />
      {!loading && !error && query.data && <ProductPagination data={query.data} disabled={updating} onPage={pageNumber => setParams(previous => ({ ...previous, pageNumber }))} onPageSize={pageSize => change({ pageSize })} />}
    </section>
  </main>;
}
```

### Frontend/billing-react/src/pages/Products/ProductRoutePlaceholder.jsx

```jsx
import { Link, useParams } from 'react-router-dom';
import { Breadcrumbs, Button } from '@mui/material';
import { ArrowBack, Inventory2Outlined } from '@mui/icons-material';
import './styles/products.css';

export function ProductRoutePlaceholder({ mode }) {
  const { id } = useParams();
  const title = mode === 'new' ? 'Add Product' : mode === 'edit' ? 'Edit Product' : 'Product Details';
  return <main className="product-page"><Breadcrumbs aria-label="Breadcrumb"><Link to="/products">Products &amp; Services</Link><span>{title}</span></Breadcrumbs>
    <section className="product-panel product-state"><Inventory2Outlined /><h1>{title}</h1><p>{id ? `Product reference: ${id}` : 'Create a new product or service.'}</p><p>This screen is not available yet. You can continue browsing the product catalog.</p><Button component={Link} to="/products" variant="outlined" startIcon={<ArrowBack />}>Back to Products</Button></section>
  </main>;
}
```

### Frontend/billing-react/src/pages/Products/services/productService.js

```js
import { mockProducts } from '../data/mockProducts.js';

const sortableFields = ['productCode', 'name', 'category', 'price', 'status'];
const collator = new Intl.Collator('en-IN', { numeric: true, sensitivity: 'base' });

// Pure query function: filter and sort the whole catalog before slicing a page.
export function queryProducts(records, params = {}) {
  const { search = '', category = '', status = '', sortBy = 'productCode', sortOrder = 'asc' } = params;
  const pageSize = [10, 20, 50].includes(Number(params.pageSize)) ? Number(params.pageSize) : 10;
  const term = search.trim().toLowerCase();
  // Match the beginning of a word, rather than letters inside a word.
  // For example, "lap" matches "Business Laptop", but "apt" does not.
  const matchesSearch = product => {
    const name = product.name.toLowerCase();
    return product.productCode.toLowerCase().startsWith(term) ||
      name.split(/\s+/).some((_, index, words) => words.slice(index).join(' ').startsWith(term));
  };
  const filtered = records.filter(product =>
    (!term || matchesSearch(product)) &&
    (!category || product.category === category) && (!status || product.status === status));
  const field = sortableFields.includes(sortBy) ? sortBy : 'productCode';
  filtered.sort((a, b) => {
    const result = field === 'price' ? a.price - b.price : collator.compare(a[field], b[field]);
    return (result || collator.compare(a.productCode, b.productCode)) * (sortOrder === 'desc' ? -1 : 1);
  });
  const totalCount = filtered.length;
  const totalPages = Math.ceil(totalCount / pageSize);
  const requestedPage = Number(params.pageNumber);
  const pageNumber = Math.min(Math.max(1, Number.isFinite(requestedPage) ? Math.floor(requestedPage) : 1), Math.max(1, totalPages));
  return {
    items: filtered.slice((pageNumber - 1) * pageSize, pageNumber * pageSize).map(item => ({ ...item })),
    pageNumber, pageSize, totalCount, totalPages,
  };
}

async function simulateRequest(signal) {
  signal?.throwIfAborted();
  await new Promise((resolve, reject) => {
    const abort = () => { clearTimeout(timer); reject(signal.reason); };
    const timer = setTimeout(() => { signal?.removeEventListener('abort', abort); resolve(); }, 300);
    signal?.addEventListener('abort', abort, { once: true });
  });
}

export const mockProductService = {
  async getProducts(params, { signal } = {}) {
    await simulateRequest(signal);
    return queryProducts(mockProducts, params);
  },
  async getCatalogMetadata({ signal } = {}) {
    await simulateRequest(signal);
    return {
      categories: [...new Set(mockProducts.map(product => product.category))].sort(),
      summary: {
        total: mockProducts.length,
        active: mockProducts.filter(product => product.status === 'Active').length,
        inactive: mockProducts.filter(product => product.status === 'Inactive').length,
        services: mockProducts.filter(product => product.type === 'Service').length,
      },
    };
  },
};

// Swap this adapter for productApiService when the backend is available.
export const productService = mockProductService;
```

### Frontend/billing-react/src/pages/Products/styles/products.css

```css
.product-page { padding: 28px 32px 40px; min-width: 0; max-width: 1800px; margin: 0 auto; color: #2f2523; background: #fffaf2; }
.product-page .MuiBreadcrumbs-root { font-size: 12px; color: #756763; margin-bottom: 18px; }
.product-page a { color: #4a2c2a; }
.product-heading { display: flex; align-items: center; justify-content: space-between; gap: 20px; margin-bottom: 28px; padding: 6px 0 2px; }
.product-eyebrow { display: block; font-size: 10px; letter-spacing: 1.8px; font-weight: 700; color: #8d6e63; margin-bottom: 10px; }
.product-heading h1 { font-size: clamp(24px, 2.3vw, 30px); font-weight: 700; letter-spacing: -.7px; margin: 0 0 7px; }
.product-heading p { color: #756763; font-size: 14px; margin: 0; }
.product-page .MuiButton-containedPrimary { background-color: #4a2c2a; color: white; }
.product-page .MuiButton-containedPrimary:hover { background-color: #5d4037; }
.product-heading .MuiButton-root { white-space: nowrap; padding: 10px 20px; }
.product-summary { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 16px; margin-bottom: 24px; }
.product-summary-card { display: flex; align-items: flex-start; gap: 14px; border: 1px solid #ecdcc4; background: #fff3df; border-radius: 14px; padding: 22px 18px; box-shadow: 0 4px 14px #69513908; border-top: 3px solid #d9b67f; }
.product-summary-card.tone-active { background: #edf7ef; border-color: #d4e7d8; border-top-color: #9ac5a5; }
.product-summary-card.tone-inactive { background: #fff0ed; border-color: #f0d9d2; border-top-color: #dfb1a5; }
.product-summary-card.tone-services { background: #f1effb; border-color: #e1dbf0; border-top-color: #b9acd9; }
.product-summary-card.tone-active .product-summary-icon { background: #dceee0; color: #386449; }
.product-summary-card.tone-inactive .product-summary-icon { background: #f9dfd8; color: #8b5145; }
.product-summary-card.tone-services .product-summary-icon { background: #e5dff5; color: #665184; }
.product-summary-card.tone-total strong { color: #70522d; }
.product-summary-card.tone-active strong { color: #386449; }
.product-summary-card.tone-inactive strong { color: #8b5145; }
.product-summary-card.tone-services strong { color: #665184; }
.product-summary-card small { display: block; color: #756763; font-size: 11px; margin-top: 5px; line-height: 1.5; }
.product-summary-icon { display: grid; place-items: center; background: #f5e5c9; color: #70522d; border-radius: 12px; width: 42px; height: 42px; flex-shrink: 0; }
.product-summary-card div > span { color: #756763; font-size: 12px; }
.product-summary-card strong { display: block; font-size: 30px; letter-spacing: -.8px; line-height: 1.3; margin-top: 4px; font-variant-numeric: tabular-nums; }
.product-panel { background: #fffefd; border: 1px solid #eee2d2; border-radius: 14px; overflow: hidden; box-shadow: 0 6px 22px #69513907; }
.product-panel-heading { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 22px; border-bottom: 1px solid #f0e9e4; }
.product-panel-title { display: flex; align-items: center; gap: 12px; min-width: 0; }
.product-panel-icon { display: grid; place-items: center; width: 38px; height: 38px; flex-shrink: 0; background: #f5efea; color: #8d6e63; border-radius: 10px; }
.product-panel-title p { margin: 4px 0 0; color: #756763; font-size: 12px; }
.product-panel-heading .product-result-count { background: #f5efea; color: #5d4037; border: 1px solid #e8ddd7; border-radius: 20px; padding: 5px 10px; font-size: 11px; font-weight: 600; white-space: nowrap; }
.product-panel-heading h2 { font-size: 16px; margin: 0; }
.product-panel-heading > span { color: #756763; font-size: 12px; }
.product-filters { display: grid; grid-template-columns: minmax(230px, 1fr) 190px 150px auto; gap: 14px; padding: 24px 22px; }
.product-page .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline { border-color: #8d6e63; }
.product-table { max-width: 100%; overflow-x: auto; }
.product-table table { min-width: 1080px; }
.product-table .MuiTableCell-root { padding: 14px 16px; font-size: 12px; border-color: #eee6e1; white-space: nowrap; }
.product-table .MuiTableCell-head { background: #faf1e4; color: #5d4037; padding-top: 16px; padding-bottom: 16px; }
.product-table .MuiTableRow-hover { transition: background-color 150ms ease; }
.product-table .MuiTableRow-hover:hover { background: #fff8ec; }
.product-table .MuiTableRow-hover:hover > td:first-child { box-shadow: inset 3px 0 #b08968; }
.product-identity { display: flex; align-items: center; gap: 10px; }
.product-row-actions { display: flex; align-items: center; gap: 8px; }
.product-row-actions .MuiIconButton-root { width: 34px; height: 34px; border-radius: 9px; border: 1px solid transparent; }
.product-row-actions .product-action-view { background: #edf2f8; color: #486580; }
.product-row-actions .product-action-view:hover { background: #dfe9f4; border-color: #c4d5e6; }
.product-row-actions .product-action-edit { background: #faf0df; color: #79572e; }
.product-row-actions .product-action-edit:hover { background: #f3e4ca; border-color: #e4cda5; }
.product-row-icon { width: 34px; height: 34px; flex-shrink: 0; display: grid; place-items: center; border-radius: 9px; background: #f5efea; color: #8d6e63; }
.product-row-icon.service { background: #edf0f4; color: #69798d; }
.product-identity .product-name { text-decoration: none; }
.product-identity .product-name:hover { text-decoration: underline; text-underline-offset: 3px; }
.product-code { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; color: #5d4037; font-weight: 600; letter-spacing: .3px; }
.product-name, .product-category { display: block; max-width: 230px; overflow: hidden; text-overflow: ellipsis; }
.product-name { font-weight: 600; font-size: 13px; }
.product-category { max-width: 150px; color: #756763; }
.product-price { font-variant-numeric: tabular-nums; font-weight: 600 !important; }
.product-badge { display: inline-flex; align-items: center; gap: 6px; border-radius: 6px; padding: 4px 9px; font-size: 11px; font-weight: 600; }
.product-badge.product { background: #f5efea; color: #5d4037; }
.product-badge.service { background: #edf0f4; color: #536278; }
.product-badge.active { background: #edf5ef; color: #386449; }
.product-badge.inactive { background: #f8eeee; color: #945454; }
.product-badge.active::before, .product-badge.inactive::before { content: ''; width: 5px; height: 5px; border-radius: 50%; background: currentColor; }
.product-pagination { display: flex; flex-wrap: wrap; align-items: center; gap: 20px; padding: 22px; background: #fffbf5; border-top: 1px solid #eee6e1; }
.product-pagination > span { margin-right: auto; font-size: 12px; color: #756763; }
.product-pagination .MuiTextField-root { width: 115px; }
.product-pagination .Mui-selected { background-color: #4a2c2a !important; color: white; }
.product-state { text-align: center; padding: 52px 20px; white-space: normal; }
.product-state > svg { font-size: 36px; color: #8d6e63; }
.product-state h2 { font-size: 18px; margin: 14px 0 8px; }
.product-state p { color: #756763; font-size: 14px; margin-bottom: 20px; }
.product-page :focus-visible { outline: 2px solid #8d6e63; outline-offset: 3px; }
@media (max-width: 1100px) { .product-filters { grid-template-columns: 1fr 1fr; } .product-summary { gap: 10px; } .product-summary-card { padding: 14px 10px; gap: 8px; } }
@media (max-width: 600px) { .product-page { padding: 20px 12px; } .product-heading { align-items: flex-start; flex-direction: column; } .product-summary { grid-template-columns: 1fr 1fr; } .product-summary-card { flex-direction: column; } .product-filters { grid-template-columns: 1fr; padding: 20px 14px; } .product-panel-heading { padding: 18px 14px; flex-wrap: wrap; } .product-pagination { gap: 18px 12px; padding: 20px 12px; } .product-pagination nav { width: 100%; } .product-pagination .MuiPagination-ul { justify-content: center; } }
@media (prefers-reduced-motion: reduce) { .product-page *, .product-page *::before, .product-page *::after { transition: none !important; } }
```

### Frontend/billing-react/src/routes/AppRoutes.jsx

```jsx
import { Routes, Route, Navigate } from 'react-router-dom';

import { Landing } from '../pages/Landing/Landing';
import { Login } from '../pages/Login/Login';
import { Register } from '../pages/Register/Register';
import { ForgotPassword } from '../pages/ForgotPassword/ForgotPassword';
import { VerifyOtp } from '../pages/VerifyOtp/VerifyOtp';
import { ResetPassword } from '../pages/ResetPassword/ResetPassword';

import { Dashboard } from '../pages/Dashboard/Dashboard';
import { CreateInvoice } from '../pages/CreateInvoice/CreateInvoice';
import { AppLayout } from '../layouts/AppLayout';
import { ModulePlaceholder } from '../pages/ModulePlaceholder/ModulePlaceholder';

import { Payments } from '../pages/Payments/Payments';
import { Invoices } from '../pages/Invoices/Invoices';
import { Taxes } from '../pages/Taxes/Taxes';
import { ProductList } from '../pages/Products/ProductList';
import { ProductRoutePlaceholder } from '../pages/Products/ProductRoutePlaceholder';

// ==============================
// CUSTOMER MODULE
// ==============================

// Manikanta - Customer List
import { CustomerListPage } from '../pages/Customers/pages/CustomerListPage';

// Jayakrishna - Create & Edit Customer
import { CreateCustomer } from '../pages/Customers/pages/CreateCustomer';
import { EditCustomer } from '../pages/Customers/pages/EditCustomer';

// Sumanth - Customer Details
import { CustomerDetailsPage } from '../pages/Customers/pages/CustomerDetailsPage';

export const AppRoutes = () => (
  <Routes>
    {/* Public Routes */}
    <Route path="/" element={<Landing />} />
    <Route path="/login" element={<Login />} />
    <Route path="/register" element={<Register />} />
    <Route path="/forgot-password" element={<ForgotPassword />} />
    <Route path="/verify-otp" element={<VerifyOtp />} />
    <Route path="/reset-password" element={<ResetPassword />} />

    {/* Application Routes */}
    <Route element={<AppLayout />}>
      <Route path="/dashboard" element={<Dashboard />} />

      {/* Invoices */}
      <Route path="/invoices" element={<Invoices />} />
      <Route path="/invoices/new" element={<CreateInvoice />} />
      <Route path="/invoices/create" element={<CreateInvoice />} />

      {/* Payments */}
      <Route path="/payments" element={<Payments />} />

      {/* ==============================
          CUSTOMER MODULE
      ============================== */}

      {/* Manikanta - Customer List */}
      <Route
        path="/customers"
        element={<CustomerListPage />}
      />

      {/* Jayakrishna - Create Customer */}
      <Route
        path="/customers/create"
        element={<CreateCustomer />}
      />

      {/* Jayakrishna - Edit Customer */}
      <Route
        path="/customers/:customerId/edit"
        element={<EditCustomer />}
      />

      {/* Sumanth - Customer Details */}
      <Route
        path="/customers/:customerId"
        element={<CustomerDetailsPage />}
      />

      {/* Other Modules */}
      <Route path="/products" element={<ProductList />} />
      <Route path="/products/new" element={<ProductRoutePlaceholder mode="new" />} />
      <Route path="/products/:id" element={<ProductRoutePlaceholder mode="view" />} />
      <Route path="/products/:id/edit" element={<ProductRoutePlaceholder mode="edit" />} />
      <Route path="/credit-notes" element={<ModulePlaceholder />} />
      <Route path="/recurring-billing" element={<ModulePlaceholder />} />
      <Route path="/expenses" element={<ModulePlaceholder />} />
      <Route path="/taxes/*" element={<Taxes />} />
      <Route path="/reports" element={<ModulePlaceholder />} />
      <Route path="/audit-activity" element={<ModulePlaceholder />} />
      <Route path="/templates-branding" element={<ModulePlaceholder />} />
      <Route path="/invoice-numbering" element={<ModulePlaceholder />} />
      <Route path="/integration-settings" element={<ModulePlaceholder />} />
      <Route path="/settings" element={<ModulePlaceholder />} />
      <Route path="/support" element={<ModulePlaceholder />} />
    </Route>

    {/* Unknown Route */}
    <Route path="*" element={<Navigate to="/" replace />} />
  </Routes>
);
```


## Production build result

Production build passed using npm.cmd run build. Vite reported a bundle-size warning for the main JavaScript chunk (928.40 kB).

This DELIVERY.md document is also a newly created file; it contains the complete implementation source above.

