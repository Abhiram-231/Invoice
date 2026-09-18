import { apiClient } from '../../../billing-api-client/apiClient.js';

const endpoint = '/api/v1/settings/taxes';
export const TAX_CONTRACT_BLOCKER = 'Tax saving is unavailable until the backend publishes its tax settings request and response contract.';

// No tax endpoint or DTO is published in the current backend/OpenAPI document.
// Keep transport on the shared authenticated client; do not guess a PUT payload.
export const taxApi = {
  get: () => apiClient.get(endpoint),
  put: confirmedPayload => apiClient.put(endpoint, confirmedPayload),
};

export const taxService = {
  async list() {
    await taxApi.get();
    throw new Error('Tax settings responded, but its response contract is not yet configured.');
  },
  async save() {
    throw new Error(TAX_CONTRACT_BLOCKER);
  },
};

export function taxError(error) {
  const status = error?.response?.status;
  if (status === 400) return 'The tax settings request was rejected. Check the entered values.';
  if (status === 401) return 'Your session has expired. Please sign in again.';
  if (status === 403) return 'You do not have permission to manage tax settings.';
  if (status === 404) return 'The tax settings endpoint is not available on this backend yet.';
  if (status === 409) return 'Tax settings have changed. Reload before trying again.';
  if (status >= 500) return 'The server could not load tax settings. Please retry.';
  if (error?.message === TAX_CONTRACT_BLOCKER || error?.message?.startsWith('Tax settings responded,')) return error.message;
  return 'Unable to connect to tax settings. Check your connection and retry.';
}
