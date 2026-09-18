import test from 'node:test';
import assert from 'node:assert/strict';
import { productValidationSchema, DEFAULT_PRODUCT_VALUES } from '../src/pages/Products/validation/productValidation.js';
import { productApiService, normalizeProduct } from '../src/pages/Products/services/productService.js';
import { productApi } from '../../billing-api-client/productApi.js';

const valid = { ...DEFAULT_PRODUCT_VALUES, productCode: 'PRD-001', name: 'Keyboard', categoryId: '1', price: 100, hsnSac: '08471301', discountPercentage: 10, discountAllowed: true };

test('validates product and service codes without losing leading zeros', () => {
  for (const type of ['Product', 'Service']) {
    const result = productValidationSchema.validateSync({ ...valid, type });
    assert.equal(result.hsnSac, '08471301');
    assert.equal(result.discountPercentage, 10);
  }
  assert.throws(() => productValidationSchema.validateSync({ ...valid, hsnSac: '12' }), /HSN\/SAC/);
});

test('discount is mandatory only when enabled', () => {
  for (const discountAllowed of [true]) {
    for (const discountPercentage of ['', ' ', null, undefined]) {
      assert.throws(() => productValidationSchema.validateSync({ ...valid, discountAllowed, discountPercentage }), /Discount is required/);
    }
  }
  for (const discountPercentage of [-1, 101, 'invalid']) {
    assert.throws(() => productValidationSchema.validateSync({ ...valid, discountPercentage }), /Discount/);
  }
  for (const discountPercentage of [0, 12.5, 100]) {
    assert.equal(productValidationSchema.validateSync({ ...valid, discountPercentage }).discountPercentage, discountPercentage);
  }
});

test('create and update send HSN/SAC and discount, and reload restores both', async () => {
  const originalCreate = productApi.createProduct;
  const originalUpdate = productApi.updateProduct;
  let stored;
  productApi.createProduct = async data => (stored = { id: 1, ...data });
  productApi.updateProduct = async (id, data) => (stored = { id, ...data });
  try {
    await productApiService.createProduct(valid);
    assert.equal(stored.hsnSacCode, '08471301');
    assert.equal(stored.discountPercent, 10);
    await productApiService.updateProduct(1, { ...valid, hsnSac: '998313', discountPercentage: 15, rowVersion: 'version' });
    const result = normalizeProduct(stored);
    assert.equal(result.hsnSac, '998313');
    assert.equal(result.discountPercentage, 15);
    assert.equal(result.rowVersion, 'version');
    await productApiService.updateProduct(1, { ...valid, hsnSac: '', discountPercentage: 0 });
    assert.equal(stored.hsnSacCode, '');
    assert.equal(stored.discountPercent, 0);
  } finally {
    productApi.createProduct = originalCreate;
    productApi.updateProduct = originalUpdate;
  }
});


test('normalizes backend discount names and preserves zero', () => {
  for (const field of ['discountPercent', 'DiscountPercent', 'discountPercentage']) {
    for (const value of [0, 12.5, 100]) {
      assert.equal(normalizeProduct({ id: 1, [field]: value }).discountPercentage, value);
    }
  }
  assert.equal(normalizeProduct({ id: 1 }).discountPercentage, '');
  assert.equal(normalizeProduct({ id: 1, discountPercent: 0, discountPercentage: 20 }).discountPercentage, 0);
});


test('disabled discounts default to zero and ignore hidden stale values', () => {
  assert.equal(DEFAULT_PRODUCT_VALUES.discountAllowed, false);
  assert.equal(DEFAULT_PRODUCT_VALUES.discountPercentage, 0);
  for (const discountPercentage of [undefined, null, '', 25, -1, 'invalid']) {
    const result = productValidationSchema.validateSync({ ...valid, discountAllowed: false, discountPercentage });
    assert.equal(result.discountPercentage, 0);
  }
});


test('new products can omit their code for server generation', async () => {
  const data = productValidationSchema.validateSync({ ...valid, productCode: '' });
  assert.equal(data.productCode, undefined);
  const originalCreate = productApi.createProduct;
  let sent;
  productApi.createProduct = async payload => { sent = payload; return { id: 9, ...payload, productCode: 'PROD-12345678' }; };
  try {
    const result = await productApiService.createProduct(data);
    assert.equal(Object.hasOwn(sent, 'productCode'), false);
    assert.equal(result.productCode, 'PROD-12345678');
  } finally {
    productApi.createProduct = originalCreate;
  }
});
