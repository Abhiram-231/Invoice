import React from 'react';
import test from 'node:test';
import assert from 'node:assert/strict';
import { renderToStaticMarkup } from 'react-dom/server';
import { StaticRouter } from 'react-router-dom/server';
import { Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ProductTable } from '../src/pages/Products/components/ProductTable';
import { ProductPagination } from '../src/pages/Products/components/ProductPagination';
import { ProductForm, getProductInitialValues } from '../src/pages/Products/components/ProductForm';
import { ProductDetails } from '../src/pages/Products/pages/ProductDetails';
import { CategoryList } from '../src/pages/Products/pages/CategoryList';
import { CategoryFormPage } from '../src/pages/Products/pages/CategoryFormPage';

const categories = [{ id: 1, name: 'Active category', status: 'Active', description: 'Available', productCount: 2 }, { id: 2, name: 'Inactive category', status: 'Inactive', description: '', productCount: 1 }];
const product = { id: 7, productCode: 'PRD-7', name: 'Long product name '.repeat(20), type: 'Product', category: 'Active category', categoryId: 1, unit: 'Piece', price: 1234.5, currency: 'USD', taxCategory: '', status: 'Inactive', hsnSac: '001234', discountAllowed: false, discountPercentage: 0 };
const client = () => new QueryClient({ defaultOptions: { queries: { retry: false, retryOnMount: false, gcTime: Infinity, staleTime: Infinity } } });
const render = (element, cache = client(), location = '/products') => renderToStaticMarkup(<StaticRouter location={location}><QueryClientProvider client={cache}>{element}</QueryClientProvider></StaticRouter>);
const table = props => <ProductTable items={[product]} loading={false} error={null} params={{ sortBy: 'price', sortOrder: 'desc' }} onSort={() => {}} onRetry={() => {}} onClear={() => {}} {...props} />;

test('PQA table: nine columns, actual currency, long names, inactive badge and numeric action URLs', () => {
  const html = render(table());
  for (const label of ['Product Code', 'Product Name', 'Type', 'Category', 'Unit', 'Price', 'Tax Category', 'Status', 'Actions']) assert.ok(html.includes(label));
  assert.match(html, /\$1,234\.50/); assert.doesNotMatch(html, /₹1,234/);
  assert.match(html, /href="\/products\/7"/); assert.match(html, /href="\/products\/7\/edit"/);
  assert.match(html, /Inactive/); assert.ok(html.includes(product.name));
  const statusHeader = html.match(/<th\b[^>]*>(?:(?!<\/th>).)*Status(?:(?!<\/th>).)*<\/th>/s)?.[0];
  assert.ok(statusHeader); assert.doesNotMatch(statusHeader, /role="button"/);
  assert.match(html, /aria-sort="descending"/);
});

test('PQA table states: skeleton, real empty, filtered empty and API failure never render fixture rows', () => {
  assert.match(render(table({ loading: true })), /MuiSkeleton/);
  assert.match(render(table({ items: [] })), /No products found/);
  assert.match(render(table({ items: [], filtered: true })), /No matching products/);
  const failed = render(table({ error: new Error('Access denied') }));
  assert.match(failed, /Unable to load products/); assert.doesNotMatch(failed, /PRD-7/);
});

test('PQA pagination: first, middle, last and empty bounds', () => {
  for (const [pageNumber, totalCount, first, last] of [[1, 21, 1, 10], [2, 21, 11, 20], [3, 21, 21, 21], [1, 0, 0, 0]]) {
    const html = render(<ProductPagination data={{ pageNumber, pageSize: 10, totalCount, totalPages: Math.ceil(totalCount / 10) }} onPage={() => {}} onPageSize={() => {}} />);
    assert.match(html, new RegExp(`Showing ${first}.*${last} of ${totalCount} products`));
  }
});

test('PQA product form: create excludes inactive category; edit retains its disabled association and legacy unit', () => {
  const cache = client(); cache.setQueryData(['categories', 'list'], categories);
  const create = render(<ProductForm mode="create" onSubmit={() => {}} />, cache);
  assert.match(create, /Active category/); assert.doesNotMatch(create, /Inactive category/);
  assert.match(create, /E-Commerce/);
  const edit = render(<ProductForm mode="edit" initialValues={{ ...product, categoryId: 2, unit: 'Hour', taxCategory: null }} onSubmit={() => {}} />, cache);
  assert.match(edit, /<option value="2" disabled="">Inactive category \(Inactive\)<\/option>/);
  assert.match(edit, /<option value="Hour">Hour<\/option>/);
  assert.match(edit, /<option value="">Not set<\/option>/);
});

test('PQA edit initialization never adds tax to an untaxed product and preserves optional values', () => {
  assert.equal(getProductInitialValues(null).taxCategory, 'GST 18%');
  for (const taxCategory of [null, undefined, '']) {
    const values = getProductInitialValues({ ...product, taxCategory, description: null, hsnSacCode: '001234', discountPercent: 0 });
    assert.equal(values.taxCategory, ''); assert.equal(values.description, '');
    assert.equal(values.hsnSac, '001234'); assert.equal(values.discountPercentage, 0);
    assert.equal(values.categoryId, '1');
  }
});

test('PQA categories: real cached rows, zero/known counts, edit and both status actions', () => {
  const cache = client(); cache.setQueryData(['categories', 'list'], categories);
  const html = render(<CategoryList />, cache, '/products/categories');
  assert.match(html, /Product Categories/); assert.match(html, /Available/);
  assert.match(html, /href="\/products\/categories\/1\/edit"/);
  assert.match(html, /aria-label="Deactivate Active category"/);
  assert.match(html, /aria-label="Activate Inactive category"/);
  assert.match(html, /2 categories/);
});

test('PQA categories: loading, empty and API-error states', () => {
  assert.match(render(<CategoryList />), /Loading categories/);
  const empty = client(); empty.setQueryData(['categories', 'list'], []);
  assert.match(render(<CategoryList />, empty), /No categories yet/);
  const failed = client(); failed.getQueryCache().build(failed, { queryKey: ['categories', 'list'] }).setState({ status: 'error', fetchStatus: 'idle', error: { status: 403 } });
  const html = render(<CategoryList />, failed);
  assert.match(html, /do not have permission/); assert.doesNotMatch(html, /No categories yet/);
});

test('PQA category forms: add and edit route render correct heading and populated values', () => {
  const cache = client(); cache.setQueryData(['categories', 'list'], categories); cache.setQueryData(['categories', 'detail', '1'], categories[0]);
  const route = <Routes><Route path="/products/categories/new" element={<CategoryFormPage />} /><Route path="/products/categories/:categoryId/edit" element={<CategoryFormPage />} /></Routes>;
  assert.match(render(route, cache, '/products/categories/new'), /Save Category/);
  const edit = render(route, cache, '/products/categories/1/edit');
  assert.match(edit, /Edit Category/); assert.match(edit, /value="Active category"/); assert.match(edit, /Available/);
});

test('PQA details: current scope renders HSN, currency, discount, description fallback and category', () => {
  const cache = client(); cache.setQueryData(['products', 'detail', '7'], product); cache.setQueryData(['categories', 'list'], categories);
  const html = render(<Routes><Route path="/products/:id" element={<ProductDetails />} /></Routes>, cache, '/products/7');
  for (const value of ['PRD-7', 'USD 1,234.50', '001234', 'Active category', 'No description provided.', 'Discount Allowed']) assert.ok(html.includes(value), value);
});
