import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Button, CircularProgress, Switch } from '@mui/material';
import {
  Inventory2Outlined,
  ReceiptLongOutlined,
  TuneOutlined,
  ArrowBack,
  SaveOutlined,
} from '@mui/icons-material';
import {
  productValidationSchema,
  DEFAULT_PRODUCT_VALUES,
  PRODUCT_TYPES,
  CURRENCIES,
  TAX_CATEGORIES,
  STANDARD_UNITS,
} from '../validation/productValidation';
import '../styles/product-form.css';

const DEFAULT_CATEGORIES = [
  'Consulting',
  'Electronics',
  'IT Services',
  'Maintenance',
  'Office Supplies',
  'Software',
  'Subscription',
];

const getCurrencySymbol = (currency) => {
  switch (currency) {
    case 'USD':
      return '$';
    case 'EUR':
      return '€';
    case 'GBP':
      return '£';
    case 'INR':
    default:
      return '₹';
  }
};

export function ProductForm({
  initialValues = null,
  onSubmit,
  isSubmitting = false,
  submitError = '',
  onCancel,
  mode = 'create',
  categories = DEFAULT_CATEGORIES,
}) {
  const getSanitizedInitialValues = (values) => {
    if (!values) return DEFAULT_PRODUCT_VALUES;
    return {
      productCode: values.productCode || '',
      name: values.name || '',
      description: values.description || '',
      type: values.type || 'Product',
      category: values.category || '',
      unit: values.unit || 'Piece',
      price: values.price !== undefined && values.price !== null ? values.price : '',
      currency: values.currency || 'INR',
      taxCategory: values.taxCategory || 'GST 18%',
      hsnSac: values.hsnSac || '',
      discountAllowed: values.discountAllowed !== false,
      status: values.status || 'Active',
    };
  };

  const {
    register,
    handleSubmit,
    control,
    watch,
    reset,
    formState: { errors },
  } = useForm({
    resolver: yupResolver(productValidationSchema),
    defaultValues: getSanitizedInitialValues(initialValues),
    mode: 'onTouched',
  });

  // Re-populate when initialValues change (edit mode async loading)
  useEffect(() => {
    if (initialValues) {
      reset(getSanitizedInitialValues(initialValues));
    }
  }, [initialValues, reset]);

  const selectedCurrency = watch('currency') || 'INR';
  const currencySymbol = getCurrencySymbol(selectedCurrency);

  // Merge unique categories
  const categoryOptions = Array.from(
    new Set([...(categories || []), initialValues?.category].filter(Boolean))
  ).sort();

  const handleValidSubmit = (data) => {
    if (isSubmitting) return;
    const payload = {
      ...data,
      productCode: data.productCode?.trim(),
      name: data.name?.trim(),
      description: data.description?.trim() || '',
      category: data.category?.trim(),
      unit: data.unit?.trim(),
      price: Number(data.price) || 0,
      currency: data.currency || 'INR',
      taxCategory: data.taxCategory || 'GST 18%',
      hsnSac: data.hsnSac?.trim() || '',
      discountAllowed: Boolean(data.discountAllowed),
      status: data.status || 'Active',
    };
    onSubmit(payload);
  };

  return (
    <div className="product-form-container">
      {submitError && (
        <div className="product-alert product-alert-error" role="alert">
          <span>⚠️ {submitError}</span>
        </div>
      )}

      <form onSubmit={handleSubmit(handleValidSubmit)} className="product-form-panel" noValidate>
        {/* SECTION 1: Product Information */}
        <div className="product-form-section">
          <div className="product-section-header">
            <span className="product-section-icon" aria-hidden="true">
              <Inventory2Outlined fontSize="small" />
            </span>
            <div>
              <h3 className="product-section-title">Product Information</h3>
              <p className="product-section-subtitle">
                Core identification details for your product or service
              </p>
            </div>
          </div>

          <div className="product-form-grid">
            {/* Product Code */}
            <div className="product-form-field">
              <label htmlFor="productCode" className="product-field-label">
                Product Code <span className="product-field-required">*</span>
              </label>
              <input
                id="productCode"
                type="text"
                placeholder="e.g. PRD-101"
                className={`product-input ${errors.productCode ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.productCode)}
                aria-describedby={errors.productCode ? 'productCode-err' : undefined}
                {...register('productCode')}
              />
              {errors.productCode && (
                <span id="productCode-err" className="product-field-error" role="alert">
                  {errors.productCode.message}
                </span>
              )}
            </div>

            {/* Product Name */}
            <div className="product-form-field">
              <label htmlFor="productName" className="product-field-label">
                Product Name <span className="product-field-required">*</span>
              </label>
              <input
                id="productName"
                type="text"
                placeholder="e.g. Wireless Keyboard"
                className={`product-input ${errors.name ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.name)}
                aria-describedby={errors.name ? 'productName-err' : undefined}
                {...register('name')}
              />
              {errors.name && (
                <span id="productName-err" className="product-field-error" role="alert">
                  {errors.name.message}
                </span>
              )}
            </div>

            {/* Product Type */}
            <div className="product-form-field">
              <label htmlFor="productType" className="product-field-label">
                Product Type <span className="product-field-required">*</span>
              </label>
              <select
                id="productType"
                className={`product-select ${errors.type ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.type)}
                {...register('type')}
              >
                {PRODUCT_TYPES.map((t) => (
                  <option key={t} value={t}>
                    {t}
                  </option>
                ))}
              </select>
              {errors.type && (
                <span className="product-field-error" role="alert">
                  {errors.type.message}
                </span>
              )}
            </div>

            {/* Category */}
            <div className="product-form-field">
              <label htmlFor="productCategory" className="product-field-label">
                Category <span className="product-field-required">*</span>
              </label>
              <select
                id="productCategory"
                className={`product-select ${errors.category ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.category)}
                aria-describedby={errors.category ? 'productCategory-err' : undefined}
                {...register('category')}
              >
                <option value="">Select Category</option>
                {categoryOptions.map((cat) => (
                  <option key={cat} value={cat}>
                    {cat}
                  </option>
                ))}
              </select>
              {errors.category && (
                <span id="productCategory-err" className="product-field-error" role="alert">
                  {errors.category.message}
                </span>
              )}
            </div>

            {/* Description */}
            <div className="product-form-field product-form-full">
              <label htmlFor="productDescription" className="product-field-label">
                Description
              </label>
              <textarea
                id="productDescription"
                rows={3}
                placeholder="Add product specifications, warranty details, or billing notes..."
                className={`product-textarea ${errors.description ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.description)}
                aria-describedby={errors.description ? 'productDescription-err' : undefined}
                {...register('description')}
              />
              {errors.description && (
                <span id="productDescription-err" className="product-field-error" role="alert">
                  {errors.description.message}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* SECTION 2: Pricing & Tax */}
        <div className="product-form-section">
          <div className="product-section-header">
            <span className="product-section-icon" aria-hidden="true">
              <ReceiptLongOutlined fontSize="small" />
            </span>
            <div>
              <h3 className="product-section-title">Pricing &amp; Tax</h3>
              <p className="product-section-subtitle">
                Set base pricing, unit of measurement, and GST tax classifications
              </p>
            </div>
          </div>

          <div className="product-form-grid">
            {/* Unit */}
            <div className="product-form-field">
              <label htmlFor="productUnit" className="product-field-label">
                Unit of Measurement <span className="product-field-required">*</span>
              </label>
              <select
                id="productUnit"
                className={`product-select ${errors.unit ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.unit)}
                {...register('unit')}
              >
                {STANDARD_UNITS.map((u) => (
                  <option key={u} value={u}>
                    {u}
                  </option>
                ))}
              </select>
              {errors.unit && (
                <span className="product-field-error" role="alert">
                  {errors.unit.message}
                </span>
              )}
            </div>

            {/* Price */}
            <div className="product-form-field">
              <label htmlFor="productPrice" className="product-field-label">
                Unit Price ({currencySymbol}) <span className="product-field-required">*</span>
              </label>
              <div className="product-input-group">
                <span className="product-input-prefix">{currencySymbol}</span>
                <input
                  id="productPrice"
                  type="number"
                  step="any"
                  min="0"
                  placeholder="0.00"
                  className={`product-input has-prefix ${errors.price ? 'has-error' : ''}`}
                  aria-invalid={Boolean(errors.price)}
                  aria-describedby={errors.price ? 'productPrice-err' : undefined}
                  {...register('price')}
                />
              </div>
              {errors.price && (
                <span id="productPrice-err" className="product-field-error" role="alert">
                  {errors.price.message}
                </span>
              )}
            </div>

            {/* Currency */}
            <div className="product-form-field">
              <label htmlFor="productCurrency" className="product-field-label">
                Billing Currency
              </label>
              <select
                id="productCurrency"
                className={`product-select ${errors.currency ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.currency)}
                {...register('currency')}
              >
                {CURRENCIES.map((c) => (
                  <option key={c} value={c}>
                    {c} ({getCurrencySymbol(c)})
                  </option>
                ))}
              </select>
              {errors.currency && (
                <span className="product-field-error" role="alert">
                  {errors.currency.message}
                </span>
              )}
            </div>

            {/* Tax Category */}
            <div className="product-form-field">
              <label htmlFor="productTaxCategory" className="product-field-label">
                Tax Category
              </label>
              <select
                id="productTaxCategory"
                className={`product-select ${errors.taxCategory ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.taxCategory)}
                {...register('taxCategory')}
              >
                {TAX_CATEGORIES.map((tax) => (
                  <option key={tax} value={tax}>
                    {tax}
                  </option>
                ))}
              </select>
              {errors.taxCategory && (
                <span className="product-field-error" role="alert">
                  {errors.taxCategory.message}
                </span>
              )}
            </div>

            {/* HSN/SAC */}
            <div className="product-form-field">
              <label htmlFor="productHsnSac" className="product-field-label">
                HSN / SAC Code
              </label>
              <input
                id="productHsnSac"
                type="text"
                placeholder="e.g. 84713010 or 998313"
                className={`product-input ${errors.hsnSac ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.hsnSac)}
                aria-describedby={errors.hsnSac ? 'productHsnSac-err' : undefined}
                {...register('hsnSac')}
              />
              {errors.hsnSac && (
                <span id="productHsnSac-err" className="product-field-error" role="alert">
                  {errors.hsnSac.message}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* SECTION 3: Settings */}
        <div className="product-form-section">
          <div className="product-section-header">
            <span className="product-section-icon" aria-hidden="true">
              <TuneOutlined fontSize="small" />
            </span>
            <div>
              <h3 className="product-section-title">Settings</h3>
              <p className="product-section-subtitle">
                Availability, discounts, and item catalog status
              </p>
            </div>
          </div>

          <div className="product-form-grid">
            {/* Discount Allowed */}
            <div className="product-form-field">
              <div className="product-switch-field">
                <div className="product-switch-info">
                  <span className="product-switch-title">Discount Allowed</span>
                  <span className="product-switch-desc">
                    Permit discounts on this item during invoice creation
                  </span>
                </div>
                <Controller
                  name="discountAllowed"
                  control={control}
                  render={({ field }) => (
                    <Switch
                      checked={Boolean(field.value)}
                      onChange={(e) => field.onChange(e.target.checked)}
                      color="primary"
                    />
                  )}
                />
              </div>
            </div>

            {/* Status */}
            <div className="product-form-field">
              <label htmlFor="productStatus" className="product-field-label">
                Status
              </label>
              <select
                id="productStatus"
                className={`product-select ${errors.status ? 'has-error' : ''}`}
                aria-invalid={Boolean(errors.status)}
                {...register('status')}
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
              {errors.status && (
                <span className="product-field-error" role="alert">
                  {errors.status.message}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Form Action Controls */}
        <div className="product-form-actions">
          <Button
            type="button"
            variant="outlined"
            onClick={onCancel}
            disabled={isSubmitting}
            className="product-btn-cancel"
            startIcon={<ArrowBack />}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isSubmitting}
            className="product-btn-submit"
            startIcon={
              isSubmitting ? (
                <CircularProgress size={16} color="inherit" />
              ) : (
                <SaveOutlined />
              )
            }
          >
            {isSubmitting
              ? mode === 'edit'
                ? 'Saving Changes...'
                : 'Creating Product...'
              : mode === 'edit'
              ? 'Update Product'
              : 'Save Product'}
          </Button>
        </div>
      </form>
    </div>
  );
}

export default ProductForm;
