export const initialDiscountConfiguration = {
  status: 'Active',
  maximumType: 'Percentage',
  maximumValue: 20,
  discountType: 'Percentage',
  applicationLevel: 'Invoice Level',
  allowLineLevel: true,
  allowInvoiceLevel: true,
  enforceMaximum: true,
  allowManualOverride: true,
  requireOverrideReason: true,
  minimumReasonLength: 10,
  roles: [
    { id: 'admin', role: 'Admin', canApply: true, maximum: 30, canOverride: true, requiresReason: true, status: 'Active' },
    { id: 'manager', role: 'Manager', canApply: true, maximum: 20, canOverride: false, requiresReason: false, status: 'Active' },
    { id: 'billing', role: 'Billing User', canApply: true, maximum: 10, canOverride: false, requiresReason: false, status: 'Active' },
    { id: 'sales', role: 'Sales User', canApply: false, maximum: 0, canOverride: false, requiresReason: false, status: 'Active' },
  ],
};
