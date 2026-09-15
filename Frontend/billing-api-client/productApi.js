import { apiClient } from './apiClient.js';
import { API_ENDPOINTS } from './endpoints.js';

const ensureSuccess = (response) => {
  if (response?.success === false || response?.isSuccess === false) {
    throw Object.assign(new Error('Product request failed.'), { response: { status: 400, data: response } });
  }
  return response?.data ?? response;
};

export const productApi = {
  getProducts: async (params = {}) => {
    try {
      const response = await apiClient.get(API_ENDPOINTS.PRODUCTS.BASE, { params });
      return ensureSuccess(response);
    } catch (err) {
      throw Object.assign(new Error(err.response?.data?.message || err.message || 'Failed to load products.'), {
        code: err.code,
        status: err.response?.status ?? err.status,
      });
    }
  },

  getProductById: async (id) => {
    try {
      const endpoint = API_ENDPOINTS.PRODUCTS.BY_ID(encodeURIComponent(id));
      const response = await apiClient.get(endpoint);
      const product = ensureSuccess(response);
      if (!product) {
        throw Object.assign(new Error('Product not found.'), { code: 'NOT_FOUND', status: 404 });
      }
      return product;
    } catch (err) {
      throw Object.assign(new Error(err.response?.data?.message || err.message || `Failed to load product with ID ${id}.`), {
        code: err.code,
        status: err.response?.status ?? err.status,
      });
    }
  },

  createProduct: async (data) => {
    try {
      const response = await apiClient.post(API_ENDPOINTS.PRODUCTS.BASE, data);
      return ensureSuccess(response);
    } catch (err) {
      throw Object.assign(new Error(err.response?.data?.message || err.message || 'Failed to create product.'), {
        code: err.code,
        status: err.response?.status ?? err.status,
      });
    }
  },

  updateProduct: async (id, data) => {
    try {
      const endpoint = API_ENDPOINTS.PRODUCTS.BY_ID(encodeURIComponent(id));
      const response = await apiClient.put(endpoint, data);
      return ensureSuccess(response);
    } catch (err) {
      throw Object.assign(new Error(err.response?.data?.message || err.message || 'Failed to update product.'), {
        code: err.code,
        status: err.response?.status ?? err.status,
      });
    }
  },
};

export default productApi;
