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
