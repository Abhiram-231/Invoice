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
