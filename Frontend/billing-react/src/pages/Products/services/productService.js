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
