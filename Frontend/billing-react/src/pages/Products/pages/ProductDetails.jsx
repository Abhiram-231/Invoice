import { Link, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Breadcrumbs, Button, CircularProgress } from '@mui/material';
import { ArrowBack, EditOutlined } from '@mui/icons-material';
import { DashboardErrorState } from '../../../components/dashboard/DashboardStates';
import { productService } from '../services/productService';
import { useCategories } from '../services/categoryService';
import '../styles/product-form.css';
import '../../../styles/Dashboard.css';

export function ProductDetails() {
  const { id } = useParams();
  const query = useQuery({ queryKey: ['products', 'detail', id], queryFn: () => productService.getProductById(id), retry: false });
  const categories = useCategories();
  const product = query.data;
  const categoryName = categories.data?.find(category => String(category.id) === String(product?.categoryId))?.name || product?.category;
  const discountDisplay = product?.discountPercent != null && Number(product.discountPercent) > 0
    ? `${Number(product.discountPercent).toFixed(2)}%`
    : (product?.discountAllowed ? '0.00%' : 'Not allowed');
  const fields = product ? [
    ['Product Code', product.productCode], ['Product Name', product.name], ['Type', product.type],
    ['Category', categoryName || (product.categoryId ? `Category ${product.categoryId}` : 'Not assigned')],
    ['Unit', product.unit], ['Unit Price', `${product.currency || 'INR'} ${Number(product.price).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`],
    ['Tax Category', product.taxCategory], ['HSN / SAC Code', product.hsnSac],
    ['Discount (%)', discountDisplay],
    ['Discount Allowed', product.discountAllowed ? 'Yes' : 'No'], ['Status', product.status],
  ] : [];
  return <main className="product-page">
    <Breadcrumbs aria-label="Breadcrumb"><Link to="/products">Products &amp; Services</Link><span>Product Details</span></Breadcrumbs>
    <header className="product-heading"><div><span className="product-eyebrow">YOUR BILLING CATALOG</span><h1>Product Details</h1><p>{product?.name || 'Product information, pricing and billing settings.'}</p></div><div className="product-row-actions"><Button component={Link} to="/products" variant="outlined" startIcon={<ArrowBack />}>Back to Products</Button>{product && <Button component={Link} to={`/products/${encodeURIComponent(id)}/edit`} variant="contained" startIcon={<EditOutlined />}>Edit Product</Button>}</div></header>
    {query.isPending ? <section className="product-panel product-state" role="status"><CircularProgress size={32} /><p>Loading product details...</p></section>
      : query.isError ? <DashboardErrorState title={query.error?.status === 404 ? 'Product not found' : 'Unable to load product'} message={query.error?.message} onRetry={() => query.refetch()} />
      : <section className="product-form-panel" aria-label="Product details">
        {categories.isError && !product.category && <Alert severity="warning" action={<Button onClick={() => categories.refetch()}>Retry</Button>}>Unable to load the category name.</Alert>}
        <div className="product-form-section"><dl className="product-form-grid" style={{ margin: 0 }}>{fields.map(([label, value]) => <div className="product-form-field" key={label}><dt className="product-field-label">{label}</dt><dd style={{ margin: 0, overflowWrap: 'anywhere' }}>{value || '-'}</dd></div>)}</dl></div>
        <div className="product-form-section"><h2 className="product-section-title">Description</h2><p style={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}>{product.description || 'No description provided.'}</p></div>
      </section>}
  </main>;
}
