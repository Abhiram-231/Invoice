import { initialDiscountConfiguration } from '../data/discountMockData';
import { initialCharges } from '../data/chargesMockData';

// TODO: Replace mock implementation with Phase 5 backend API once the backend contract is finalized.
let discountConfiguration = structuredClone(initialDiscountConfiguration);
let charges = structuredClone(initialCharges);

export const settingsMockService = {
  getDiscountConfiguration: () => structuredClone(discountConfiguration),
  saveDiscountConfiguration: (configuration) => { discountConfiguration = structuredClone(configuration); return structuredClone(discountConfiguration); },
  getCharges: () => structuredClone(charges),
  createCharge: (charge) => { const record = { ...charge, id: `charge-${Date.now()}` }; charges = [...charges, record]; return structuredClone(record); },
  updateCharge: (charge) => { charges = charges.map((item) => item.id === charge.id ? { ...charge } : item); return structuredClone(charge); },
  setChargeStatus: (id, status) => { charges = charges.map((item) => item.id === id ? { ...item, status } : item); return structuredClone(charges.find((item) => item.id === id)); },
};
