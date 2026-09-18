export const initialCharges = [
  { id: 'shipping', name: 'Shipping Charge', code: 'SHIP', type: 'Shipping', calculationType: 'Fixed', value: 100, applicationLevel: 'Invoice Level', taxable: true, description: 'Standard delivery charge.', status: 'Active' },
  { id: 'handling', name: 'Handling Fee', code: 'HAND', type: 'Handling', calculationType: 'Fixed', value: 25, applicationLevel: 'Invoice Level', taxable: true, description: 'Order handling fee.', status: 'Active' },
  { id: 'convenience', name: 'Convenience Charge', code: 'CONV', type: 'Convenience Charge', calculationType: 'Percentage', value: 2, applicationLevel: 'Invoice Level', taxable: false, description: 'Online payment convenience charge.', status: 'Active' },
  { id: 'late', name: 'Late Fee', code: 'LATE', type: 'Late Fee', calculationType: 'Percentage', value: 1.5, applicationLevel: 'Invoice Level', taxable: false, description: 'Applied to overdue invoices.', status: 'Inactive' },
  { id: 'packaging', name: 'Packaging Fee', code: 'PACK', type: 'Custom Charge', calculationType: 'Fixed', value: 50, applicationLevel: 'Invoice Level', taxable: true, description: 'Protective packaging fee.', status: 'Active' },
];
