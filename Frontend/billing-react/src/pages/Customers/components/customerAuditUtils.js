const parse = value => {
  if (typeof value !== 'string') return value;
  try { return JSON.parse(value); } catch { return value; }
};
const object = value => value !== null && typeof value === 'object';
const display = value => value == null || value === '' ? '\u2192' : object(value) ? JSON.stringify(value) : String(value);

// Audit endpoints can return whole snapshots; show only their actual differences.
export function formatAuditChanges(row) {
  const before = parse(row.oldValue);
  const after = parse(row.newValue);
  if (object(before) && object(after)) {
    const changes = [];
    const compare = (oldValue, newValue, path) => {
      if (object(oldValue) && object(newValue) && Array.isArray(oldValue) === Array.isArray(newValue)) {
        for (const key of new Set([...Object.keys(oldValue), ...Object.keys(newValue)])) {
          compare(oldValue[key], newValue[key], path ? `${path}.${key}` : key);
        }
      } else if (!Object.is(oldValue, newValue)) {
        changes.push(`${path}: ${display(oldValue)} to ${display(newValue)}`);
      }
    };
    compare(before, after, '');
    return changes.length ? changes.join('; ') : 'No field changes';
  }
  if (row.oldValue != null || row.newValue != null) return `${display(before)} to ${display(after)}`;
  return display(parse(row.changes));
}
