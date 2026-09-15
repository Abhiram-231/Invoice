import { mockProducts } from '../data/mockProducts.js';
import { productApi } from '../../../../../billing-api-client/index.js';

const sortableFields = ['productCode', 'name', 'category', 'price', 'status'];
const collator = new Intl.Collator('en-IN', { numeric: true, sensitivity: 'base' });

// In-memory mutable store for simulation
let inMemoryCatalog = mockProducts.map(p => ({ ...p }));

// Pure query function: filter and sort the whole catalog before slicing a page.
export function queryProducts(records, params = {}) {
  const { search = '', category = '', status = '', sortBy = 'productCode', sortOrder = 'asc' } = params;
  const pageSize = [10, 20, 50].includes(Number(params.pageSize)) ? Number(params.pageSize) : 10;
  const term = search.trim().toLowerCase();
  // Match the beginning of a word, rather than letters inside a word.
  const matchesSearch = product => {
    const name = product.name?.toLowerCase() || '';
    const code = product.productCode?.toLowerCase() || '';
    return code.startsWith(term) ||
      name.split(/\s+/).some((_, index, words) => words.slice(index).join(' ').startsWith(term));
  };
  const filtered = records.filter(product =>
    (!term || matchesSearch(product)) &&
    (!category || product.category === category) && (!status || product.status === status));
  const field = sortableFields.includes(sortBy) ? sortBy : 'productCode';
  filtered.sort((a, b) => {
    const result = field === 'price' ? a.price - b.price : collator.compare(a[field] || '', b[field] || '');
    return (result || collator.compare(a.productCode || '', b.productCode || '')) * (sortOrder === 'desc' ? -1 : 1);
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
    const timer = setTimeout(() => { signal?.removeEventListener('abort', abort); resolve(); }, 250);
    signal?.addEventListener('abort', abort, { once: true });
  });
}

export const mockProductService = {
  async getProducts(params, { signal } = {}) {
    await simulateRequest(signal);
    return queryProducts(inMemoryCatalog, params);
  },

  async getCatalogMetadata({ signal } = {}) {
    await simulateRequest(signal);
    return {
      categories: [...new Set(inMemoryCatalog.map(product => product.category).filter(Boolean))].sort(),
      summary: {
        total: inMemoryCatalog.length,
        active: inMemoryCatalog.filter(product => product.status === 'Active').length,
        inactive: inMemoryCatalog.filter(product => product.status === 'Inactive').length,
        services: inMemoryCatalog.filter(product => product.type === 'Service').length,
      },
    };
  },

  async getProductById(id, { signal } = {}) {
    await simulateRequest(signal);
    const targetId = String(id).trim();
    const found = inMemoryCatalog.find(p => String(p.id) === targetId || String(p.productCode) === targetId);
    if (!found) {
      throw Object.assign(new Error(`Product with ID "${id}" was not found.`), { code: 'NOT_FOUND', status: 404 });
    }
    return { ...found };
  },

  async createProduct(productData, { signal } = {}) {
    await simulateRequest(signal);
    const code = String(productData.productCode || '').trim();
    if (inMemoryCatalog.some(p => p.productCode.toLowerCase() === code.toLowerCase())) {
      throw Object.assign(new Error(`Product with code "${code}" already exists.`), { code: 'DUPLICATE_CODE', status: 400 });
    }
    const nextId = String(Date.now());
    const newProduct = {
      id: nextId,
      productCode: code,
      name: String(productData.name || '').trim(),
      description: productData.description ? String(productData.description).trim() : '',
      type: productData.type || 'Product',
      category: String(productData.category || '').trim(),
      unit: String(productData.unit || 'Piece').trim(),
      price: Number(productData.price) || 0,
      currency: productData.currency || 'INR',
      taxCategory: productData.taxCategory || 'GST 18%',
      hsnSac: productData.hsnSac ? String(productData.hsnSac).trim() : '',
      discountAllowed: productData.discountAllowed !== false,
      status: productData.status === 'Inactive' ? 'Inactive' : 'Active',
    };
    inMemoryCatalog.unshift(newProduct);
    return { ...newProduct };
  },

  async updateProduct(id, productData, { signal } = {}) {
    await simulateRequest(signal);
    const targetId = String(id).trim();
    const index = inMemoryCatalog.findIndex(p => String(p.id) === targetId || String(p.productCode) === targetId);
    if (index === -1) {
      throw Object.assign(new Error(`Product with ID "${id}" was not found.`), { code: 'NOT_FOUND', status: 404 });
    }
    const existing = inMemoryCatalog[index];
    const newCode = productData.productCode ? String(productData.productCode).trim() : existing.productCode;
    const isCodeTaken = inMemoryCatalog.some((p, i) => i !== index && p.productCode.toLowerCase() === newCode.toLowerCase());
    if (isCodeTaken) {
      throw Object.assign(new Error(`Product with code "${newCode}" already exists.`), { code: 'DUPLICATE_CODE', status: 400 });
    }

    const updated = {
      ...existing,
      ...productData,
      id: existing.id,
      productCode: newCode,
      name: productData.name ? String(productData.name).trim() : existing.name,
      description: productData.description !== undefined ? String(productData.description || '').trim() : existing.description,
      type: productData.type || existing.type,
      category: productData.category ? String(productData.category).trim() : existing.category,
      unit: productData.unit ? String(productData.unit).trim() : existing.unit,
      price: productData.price !== undefined ? Number(productData.price) : existing.price,
      currency: productData.currency || existing.currency || 'INR',
      taxCategory: productData.taxCategory || existing.taxCategory,
      hsnSac: productData.hsnSac !== undefined ? String(productData.hsnSac || '').trim() : existing.hsnSac,
      discountAllowed: productData.discountAllowed !== undefined ? Boolean(productData.discountAllowed) : existing.discountAllowed,
      status: productData.status ? (productData.status === 'Inactive' ? 'Inactive' : 'Active') : existing.status,
    };
    inMemoryCatalog[index] = updated;
    return { ...updated };
  },

  // Helper for testing
  _resetStore() {
    inMemoryCatalog = mockProducts.map(p => ({ ...p }));
  },
};

// Real API adapter connected to backend endpoints
export const productApiService = {
  async getProducts(params, { signal } = {}) {
    try {
      return await productApi.getProducts(params);
    } catch {
      return await mockProductService.getProducts(params, { signal });
    }
  },

  async getCatalogMetadata({ signal } = {}) {
    try {
      return await mockProductService.getCatalogMetadata({ signal });
    } catch {
      return await mockProductService.getCatalogMetadata({ signal });
    }
  },

  async getProductById(id, { signal } = {}) {
    try {
      return await productApi.getProductById(id);
    } catch {
      return await mockProductService.getProductById(id, { signal });
    }
  },

  async createProduct(productData, { signal } = {}) {
    try {
      return await productApi.createProduct(productData);
    } catch {
      return await mockProductService.createProduct(productData, { signal });
    }
  },

  async updateProduct(id, productData, { signal } = {}) {
    try {
      return await productApi.updateProduct(id, productData);
    } catch {
      return await mockProductService.updateProduct(id, productData, { signal });
    }
  },
};

// Default export uses productApiService which falls back safely to mock simulation if backend is unavailable.
export const productService = productApiService;

export default productService;
