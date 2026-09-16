import React, { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Breadcrumbs, Button, CircularProgress } from '@mui/material';
import { ArrowBack, Inventory2Outlined } from '@mui/icons-material';
import { productService } from '../services/productService';
import { ProductForm } from '../components/ProductForm';
import '../styles/product-form.css';

export function EditProduct() {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');

  const productQuery = useQuery({
    queryKey: ['products', 'detail', id],
    queryFn: ({ signal }) => productService.getProductById(id, { signal }),
    enabled: Boolean(id),
    retry: 1,
  });

  const handleSubmit = async (formData) => {
    if (isSubmitting || !id) return;
    setIsSubmitting(true);
    setSubmitError('');

    try {
      await productService.updateProduct(id, formData);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['products'] }),
        queryClient.invalidateQueries({ queryKey: ['products', 'detail', id] }),
      ]);
      navigate('/products', {
        state: { productNotice: `Product "${formData.name}" updated successfully.` },
      });
    } catch (err) {
      setSubmitError(err.message || 'Failed to update product. Please check your entries.');
      setIsSubmitting(false);
    }
  };

  if (productQuery.isLoading) {
    return (
      <main className="product-page">
        <Breadcrumbs aria-label="Breadcrumb">
          <Link to="/products">Products &amp; Services</Link>
          <span>Edit Product</span>
        </Breadcrumbs>

        <section className="product-panel product-state" style={{ padding: '64px 20px' }}>
          <CircularProgress size={36} style={{ color: '#8d6e63' }} />
          <h2>Loading product data...</h2>
          <p>Please wait while we retrieve the product information.</p>
        </section>
      </main>
    );
  }

  if (productQuery.isError || !productQuery.data) {
    return (
      <main className="product-page">
        <Breadcrumbs aria-label="Breadcrumb">
          <Link to="/products">Products &amp; Services</Link>
          <span>Edit Product</span>
        </Breadcrumbs>

        <section className="product-panel product-state" style={{ padding: '64px 20px' }}>
          <Inventory2Outlined />
          <h2>Unable to find product</h2>
          <p>{productQuery.error?.message || `No product found matching reference: "${id}".`}</p>
          <Button
            component={Link}
            to="/products"
            variant="outlined"
            startIcon={<ArrowBack />}
            className="product-btn-cancel"
          >
            Back to Products
          </Button>
        </section>
      </main>
    );
  }

  return (
    <main className="product-page">
      <Breadcrumbs aria-label="Breadcrumb">
        <Link to="/products">Products &amp; Services</Link>
        <span>Edit Product</span>
      </Breadcrumbs>

      <header className="product-heading">
        <div>
          <span className="product-eyebrow">YOUR BILLING CATALOG</span>
          <h1>Edit Product</h1>
          <p>Update product pricing, tax and billing details.</p>
        </div>
        <Button
          component={Link}
          to="/products"
          variant="outlined"
          startIcon={<ArrowBack />}
        >
          Back to Products
        </Button>
      </header>

      <ProductForm
        mode="edit"
        initialValues={productQuery.data}
        onSubmit={handleSubmit}
        isSubmitting={isSubmitting}
        submitError={submitError}
        onCancel={() => navigate('/products')}
      />
    </main>
  );
}

export default EditProduct;
