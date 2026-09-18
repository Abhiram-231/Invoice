import test from 'node:test';
import assert from 'node:assert/strict';
import { formatAuditChanges } from '../src/pages/Customers/components/customerAuditUtils.js';

test('whole snapshots show only the changed field', () => {
  assert.equal(formatAuditChanges({ oldValue: '{"name":"Old","email":"same","status":"Active"}', newValue: '{"status":"Active","email":"same","name":"New"}' }), 'name: Old to New');
});
test('nested snapshots retain changed, added and removed values', () => {
  assert.equal(formatAuditChanges({ oldValue: { address: { city: 'Old', pin: '123456' }, phone: '123' }, newValue: { address: { city: 'New', pin: '123456' }, active: false } }), 'address.city: Old to New; phone: 123 to \u2192; active: \u2192 to false');
});
test('unchanged arrays and reordered keys produce no false changes', () => {
  assert.equal(formatAuditChanges({ oldValue: { items: [{ a: 0, b: false }] }, newValue: { items: [{ b: false, a: 0 }] } }), 'No field changes');
});
test('plain field changes and changes-only events stay readable', () => {
  assert.equal(formatAuditChanges({ oldValue: 'Old', newValue: 'New' }), 'Old to New');
  assert.equal(formatAuditChanges({ changes: 'Customer created' }), 'Customer created');
});


test('missing values use a right arrow and preserve question marks in user text', () => {
  assert.equal(formatAuditChanges({}), '\u2192');
  assert.equal(formatAuditChanges({ oldValue: null, newValue: 'Why?' }), '\u2192 to Why?');
});
