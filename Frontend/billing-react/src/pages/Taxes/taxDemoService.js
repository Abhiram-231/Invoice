// Explicit demo records: never sent to the billing API or persisted as real taxes.
const seedTaxes = [
  { id: 'demo-gst', name: 'Standard GST', code: 'GST18', type: 'GST', rate: 18, calculation: 'Exclusive', priority: 1, effectiveFrom: '2026-04-01', effectiveTo: '', status: 'Active' },
  { id: 'demo-cgst', name: 'Central GST', code: 'CGST9', type: 'CGST', rate: 9, calculation: 'Exclusive', priority: 2, effectiveFrom: '2026-04-01', effectiveTo: '', status: 'Active' },
  { id: 'demo-sgst', name: 'State GST', code: 'SGST9', type: 'SGST', rate: 9, calculation: 'Exclusive', priority: 3, effectiveFrom: '2026-04-01', effectiveTo: '', status: 'Active' },
  { id: 'demo-igst', name: 'Integrated GST', code: 'IGST18', type: 'IGST', rate: 18, calculation: 'Exclusive', priority: 1, effectiveFrom: '2026-04-01', effectiveTo: '', status: 'Active' },
  { id: 'demo-vat', name: 'VAT Inclusive', code: 'VAT5', type: 'VAT', rate: 5, calculation: 'Inclusive', priority: 1, effectiveFrom: '2026-01-01', effectiveTo: '', status: 'Active' },
  { id: 'demo-custom', name: 'Custom Tax Example', code: 'CUSTOM2', type: 'Custom Tax', rate: 2, calculation: 'Exclusive', priority: 4, effectiveFrom: '2026-01-01', effectiveTo: '2026-12-31', status: 'Inactive' },
];
let records = seedTaxes.map(tax => ({ ...tax }));
export const taxDemoService = {
  async list() { return records.map(tax => ({ ...tax })); },
  async save(values, id) {
    const record = { ...values, name: values.name.trim(), code: values.code.trim(), rate: Number(values.rate), priority: Number(values.priority), id: id || `demo-${crypto.randomUUID()}` };
    records = id ? records.map(tax => tax.id === id ? record : tax) : [...records, record];
    return { ...record };
  },
};
