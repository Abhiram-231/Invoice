import { apiClient } from './apiClient.js';
import { API_ENDPOINTS } from './endpoints.js';

// apiClient already unwraps Axios responses and applies the authenticated context.
const unwrap = response => {
  if (response?.success === false || response?.isSuccess === false) {
    throw Object.assign(new Error('The category request could not be completed.'), {
      response: { status: 400, data: response },
    });
  }
  return response?.data ?? response;
};

export const categoryApi = {
  getCategories: async (params = {}, { signal } = {}) => unwrap(await apiClient.get(API_ENDPOINTS.CATEGORIES.BASE, { params, signal })),
  getCategoryById: async (id, { signal } = {}) => unwrap(await apiClient.get(API_ENDPOINTS.CATEGORIES.BY_ID(id), { signal })),
  createCategory: async data => unwrap(await apiClient.post(API_ENDPOINTS.CATEGORIES.BASE, data)),
  updateCategory: async (id, data) => unwrap(await apiClient.put(API_ENDPOINTS.CATEGORIES.BY_ID(id), data)),
  setCategoryStatus: async (id, isActive) => unwrap(await apiClient.patch(API_ENDPOINTS.CATEGORIES.STATUS(id), null, { params: { status: isActive ? 'Active' : 'Inactive' } })),
};

export default categoryApi;
