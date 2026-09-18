import test from 'node:test';
import assert from 'node:assert/strict';
import { calculatePreview, emptyTax, validateTax } from './taxModel.js';

test('exclusive intra-state GST calculates and splits 18 percent', () => {
  const result = calculatePreview('1000', '18', 'Exclusive', 'GST', 'Intra-State');
  assert.deepEqual(result, { taxable: 1000, tax: 180, total: 1180, lines: [{ label: 'CGST (9%)', amount: 90 }, { label: 'SGST (9%)', amount: 90 }] });
});
test('inclusive GST extracts tax without increasing total', () => {
  const result = calculatePreview('1180', '18', 'Inclusive', 'GST', 'Inter-State');
  assert.equal(result.taxable, 1000); assert.equal(result.tax, 180); assert.equal(result.total, 1180); assert.equal(result.lines[0].label, 'IGST (18%)');
});
test('VAT, custom and component taxes use their own rate', () => {
  for (const type of ['VAT', 'Custom Tax', 'CGST', 'SGST', 'IGST']) {
    const result = calculatePreview('200', '5', 'Exclusive', type, 'Intra-State');
    assert.equal(result.tax, 10); assert.equal(result.lines.length, 1); assert.equal(result.total, 210);
  }
});
test('split rounding preserves the total and rejects invalid preview values', () => {
  const result = calculatePreview('0.05', '18', 'Exclusive', 'GST', 'Intra-State');
  assert.equal(result.lines.reduce((sum, line) => sum + line.amount, 0), result.tax);
  for (const amount of ['', '-1', 'Infinity', 'bad', '1000000000001']) assert.equal(calculatePreview(amount, 18, 'Exclusive', 'GST', 'Intra-State'), null);
  assert.equal(calculatePreview('100', '101', 'Exclusive', 'GST', 'Intra-State'), null);
  assert.equal(calculatePreview('0', '0', 'Inclusive', 'VAT', '' ).total, 0);
});
test('required, numeric and date validation covers create and edit inputs', () => {
  assert.ok(validateTax(emptyTax()).name);
  const valid = { ...emptyTax(), name: 'Standard GST', code: 'GST18', type: 'GST', rate: '18', effectiveFrom: '2026-09-18' };
  assert.deepEqual(validateTax(valid), {});
  for (const rate of ['', 'abc', '-1', '101', 'Infinity']) assert.ok(validateTax({ ...valid, rate }).rate);
  for (const priority of ['', '-1', '1.5', 'abc']) assert.ok(validateTax({ ...valid, priority }).priority);
  assert.ok(validateTax({ ...valid, effectiveTo: '2026-09-17' }).effectiveTo);
  assert.ok(validateTax({ ...valid, effectiveFrom: '2026-02-30' }).effectiveFrom);
  assert.deepEqual(validateTax({ ...valid, effectiveTo: '2026-09-18', rate: '0' }), {});
});
