export const API_ENDPOINTS = {
  AUTH: {
    REGISTER: '/api/Auth/register',
    LOGIN: '/api/Auth/login',
    FORGOT_PASSWORD: '/api/Auth/forgot-password',
    VERIFY_OTP: '/api/Auth/verify-otp',
    RESET_PASSWORD: '/api/Auth/reset-password',
  },
  CUSTOMERS: {
    BASE: '/api/v1/customers',
    BY_ID: (id) => `/api/v1/customers/${id}`,
    DEACTIVATE: (id) => `/api/v1/customers/${id}/deactivate`,
    DETAILS: (id) => `/api/v1/customers/${id}/details`,
  },
  PRODUCTS: {
    BASE: '/api/v1/products',
    BY_ID: (id) => `/api/v1/products/${id}`,
  },
  CATEGORIES: {
    BASE: '/api/v1/categories',
    BY_ID: (id) => `/api/v1/categories/${encodeURIComponent(id)}`,
    STATUS: (id) => `/api/v1/categories/${encodeURIComponent(id)}/status`,
  },
};

export default API_ENDPOINTS;
