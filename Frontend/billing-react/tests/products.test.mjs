import test, { beforeEach } from 'node:test';
import assert from 'node:assert/strict';
import {
  productValidationSchema,
  DEFAULT_PRODUCT_VALUES,
} from '../src/pages/Products/validation/productValidation.js';
import {
  mockProductService,
  queryProducts,
} from '../src/pages/Products/services/productService.js';

beforeEach(() => {
  mockProductService._resetStore();
});

test('productValidationSchema passes on complete valid product', () => {
  const validProduct = {
    productCode: 'PRD-999',
    name: 'Enterprise Cloud Server',
    description: 'High-performance cloud dedicated compute node',
    type: 'Service',
    category: 'IT Services',
    unit: 'Month',
    price: 15000,
    currency: 'INR',
    taxCategory: 'GST 18%',
    hsnSac: '998313',
    discountAllowed: true,
    status: 'Active',
  };

  const validated = productValidationSchema.validateSync(validProduct);
  assert.equal(validated.productCode, 'PRD-999');
  assert.equal(validated.name, 'Enterprise Cloud Server');
  assert.equal(validated.price, 15000);
  assert.equal(validated.hsnSac, '998313');
  assert.equal(validated.discountAllowed, true);
  assert.equal(validated.status, 'Active');
});

test('productValidationSchema rejects missing product code', () => {
  assert.throws(
    () => {
      productValidationSchema.validateSync({
        name: 'Product Without Code',
        category: 'Electronics',
        unit: 'Piece',
        price: 500,
      });
    },
    (err) => err.message.includes('Product code is required')
  );
});

test('productValidationSchema rejects invalid product code format', () => {
  assert.throws(
    () => {
      productValidationSchema.validateSync({
        productCode: 'PRD@#$123',
        name: 'Bad Code Product',
        category: 'Electronics',
        unit: 'Piece',
        price: 500,
      });
    },
    (err) => err.message.includes('Product code must contain only')
  );
});

test('productValidationSchema rejects missing product name', () => {
  assert.throws(
    () => {
      productValidationSchema.validateSync({
        productCode: 'PRD-001',
        name: '',
        category: 'Electronics',
        unit: 'Piece',
        price: 500,
      });
    },
    (err) => err.message.includes('Product name is required')
  );
});

test('productValidationSchema rejects missing or negative price', () => {
  assert.throws(
    () => {
      productValidationSchema.validateSync({
        productCode: 'PRD-001',
        name: 'Negative Price Product',
        category: 'Electronics',
        unit: 'Piece',
        price: -50,
      });
    },
    (err) => err.message.includes('Price cannot be negative')
  );

  assert.throws(
    () => {
      productValidationSchema.validateSync({
        productCode: 'PRD-001',
        name: 'Empty Price Product',
        category: 'Electronics',
        unit: 'Piece',
        price: '',
      });
    },
    (err) => err.message.includes('Price must be a valid number')
  );
});

test('productValidationSchema validates HSN/SAC formatting when provided', () => {
  // Invalid HSN (2 digits - must be 4 to 8 digits)
  assert.throws(
    () => {
      productValidationSchema.validateSync({
        productCode: 'PRD-001',
        name: 'Invalid HSN Product',
        category: 'Electronics',
        unit: 'Piece',
        price: 100,
        hsnSac: '12',
      });
    },
    (err) => err.message.includes('HSN/SAC must be 4 to 8 digits')
  );

  // Valid 4, 6, and 8 digits should pass
  assert.doesNotThrow(() => {
    productValidationSchema.validateSync({
      productCode: 'PRD-001',
      name: 'Valid HSN Product',
      category: 'Electronics',
      unit: 'Piece',
      price: 100,
      hsnSac: '8471',
    });
  });

  assert.doesNotThrow(() => {
    productValidationSchema.validateSync({
      productCode: 'PRD-001',
      name: 'Valid SAC Product',
      category: 'IT Services',
      unit: 'Hour',
      price: 1000,
      hsnSac: '998313',
    });
  });

  assert.doesNotThrow(() => {
    productValidationSchema.validateSync({
      productCode: 'PRD-001',
      name: 'Valid 8 Digit HSN',
      category: 'Electronics',
      unit: 'Piece',
      price: 100,
      hsnSac: '84713010',
    });
  });
});

test('DEFAULT_PRODUCT_VALUES contains clean defaults', () => {
  assert.equal(DEFAULT_PRODUCT_VALUES.type, 'Product');
  assert.equal(DEFAULT_PRODUCT_VALUES.currency, 'INR');
  assert.equal(DEFAULT_PRODUCT_VALUES.taxCategory, 'GST 18%');
  assert.equal(DEFAULT_PRODUCT_VALUES.status, 'Active');
  assert.equal(DEFAULT_PRODUCT_VALUES.discountAllowed, true);
});

test('mockProductService creates a new product and reflects it in catalog', async () => {
  const newProductData = {
    productCode: 'PRD-999',
    name: 'Ergonomic Desk Chair',
    description: 'Adjustable lumbar support chair',
    type: 'Product',
    category: 'Office Supplies',
    unit: 'Unit',
    price: 12500,
    currency: 'INR',
    taxCategory: 'GST 18%',
    hsnSac: '94013000',
    discountAllowed: true,
    status: 'Active',
  };

  const created = await mockProductService.createProduct(newProductData);
  assert.ok(created.id);
  assert.equal(created.productCode, 'PRD-999');
  assert.equal(created.name, 'Ergonomic Desk Chair');
  assert.equal(created.price, 12500);

  // Verify created product is immediately searchable and listable
  const list = await mockProductService.getProducts({ search: 'Ergonomic' });
  assert.equal(list.totalCount, 1);
  assert.equal(list.items[0].productCode, 'PRD-999');

  // Verify duplicate code creation throws an error
  await assert.rejects(
    () => mockProductService.createProduct(newProductData),
    (err) => err.message.includes('already exists')
  );
});

test('mockProductService retrieves product by ID and handles not found', async () => {
  const product = await mockProductService.getProductById('1');
  assert.equal(product.id, '1');
  assert.equal(product.name, 'Web Development');

  await assert.rejects(
    () => mockProductService.getProductById('non-existent-999'),
    (err) => err.status === 404
  );
});

test('mockProductService updates product and rejects duplicate codes', async () => {
  const updateData = {
    name: 'Advanced Web Development',
    price: 32000,
    status: 'Inactive',
  };

  const updated = await mockProductService.updateProduct('1', updateData);
  assert.equal(updated.name, 'Advanced Web Development');
  assert.equal(updated.price, 32000);
  assert.equal(updated.status, 'Inactive');

  // Verify updated data persisted
  const fetched = await mockProductService.getProductById('1');
  assert.equal(fetched.name, 'Advanced Web Development');
  assert.equal(fetched.price, 32000);

  // Verify trying to change code to an already taken code fails
  await assert.rejects(
    () => mockProductService.updateProduct('1', { productCode: 'PRD-002' }),
    (err) => err.message.includes('already exists')
  );
});
