import { numberingApi } from 'billing-api-client';
import {
  DEFAULT_PRESETS_BY_DOC_TYPE,
  DEFAULT_NUMBERING_CONFIG,
} from '../validation/numberingValidation';

const STORAGE_KEY = 'ibms_numbering_settings_v1';

const getStoredSettings = () => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
};

const saveStoredSettings = (documentType, config) => {
  try {
    const existing = getStoredSettings() || {};
    existing[documentType] = config;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(existing));
  } catch (e) {
    console.warn('Unable to persist numbering settings locally', e);
  }
};

export const numberingService = {
  /**
   * Loads numbering configuration for a given document type.
   */
  async getSettings(documentType = 'Invoice') {
    try {
      const response = await numberingApi.getNumberingSettings({ documentType });
      if (response && typeof response === 'object' && response.documentType) {
        return { data: response, isFallback: false };
      }
    } catch (err) {
      // If endpoint is not found (404/501) or network unavailable, gracefully fall back
      const isMissingEndpoint =
        err.status === 404 ||
        err.status === 501 ||
        err.code === 'ERR_NETWORK' ||
        err.message?.includes('Network Error');

      if (!isMissingEndpoint) {
        console.warn('Backend error fetching numbering settings:', err.message);
      }
    }

    // Check local storage or defaults
    const stored = getStoredSettings();
    const config =
      stored?.[documentType] ||
      DEFAULT_PRESETS_BY_DOC_TYPE[documentType] ||
      DEFAULT_NUMBERING_CONFIG;

    return { data: { ...config, documentType }, isFallback: true };
  },

  /**
   * Updates numbering configuration for a given document type.
   */
  async updateSettings(payload) {
    const documentType = payload.documentType || 'Invoice';
    try {
      const response = await numberingApi.updateNumberingSettings(payload);
      // Also sync to local storage cache
      saveStoredSettings(documentType, payload);
      return { success: true, data: response, isFallback: false };
    } catch (err) {
      const isMissingEndpoint =
        err.status === 404 ||
        err.status === 501 ||
        err.code === 'ERR_NETWORK' ||
        err.message?.includes('Network Error');

      if (isMissingEndpoint) {
        // Persist locally so user sees their changes remain saved
        saveStoredSettings(documentType, payload);
        return {
          success: true,
          data: payload,
          isFallback: true,
          message: 'Settings saved locally. (Backend endpoint pending deployment)',
        };
      }

      throw err;
    }
  },
};

export default numberingService;
