import { Card, Skeleton } from '@mui/material';
import { DashboardErrorState } from '../../../components/dashboard/DashboardStates';
import { StatusBadge as DashboardStatusBadge } from '../../../components/dashboard/DashboardSections';
const StatusBadge = ({ value }) => value == null || value === '' ? <span>—</span> : <DashboardStatusBadge value={String(value)} />;
const formatCurrency = (value, currency) => {
  if (value == null || value === '' || !Number.isFinite(Number(value)) || !currency?.trim()) return '—';
  try { return new Intl.NumberFormat('en-IN', { style: 'currency', currency: currency.trim().toUpperCase() }).format(Number(value)); } catch { return '—'; }
};

export { StatusBadge, formatCurrency };
export const displayDate = (value, time = false) => {
  if (!value) return '—';
  let target = value;
  if (typeof value === 'string') {
    const trimmed = value.trim();
    if (!trimmed) return '—';
    if (trimmed.length === 10) {
      target = `${trimmed}T00:00:00`;
    } else if (!trimmed.endsWith('Z') && !/[+-]\d{2}(:\d{2})?$/.test(trimmed)) {
      target = `${trimmed.replace(' ', 'T')}Z`;
    } else {
      target = trimmed;
    }
  }
  const dateObj = target instanceof Date ? target : new Date(target);
  if (Number.isNaN(dateObj.getTime())) return '—';
  return new Intl.DateTimeFormat('en-IN', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    ...(time ? { hour: '2-digit', minute: '2-digit' } : {})
  }).format(dateObj);
};
export function CustomerState({ query }) {
  if (query.isPending) return <div role="status" aria-label="Loading customer data" className="customer-detail-skeleton"><Skeleton height={80} /><div className="customer-summary">{[0, 1, 2].map(i => <Skeleton key={i} variant="rounded" height={92} />)}</div><Skeleton variant="rounded" height={220} /></div>;
  if (query.isError) return <DashboardErrorState title={["NOT_FOUND", "INVALID_ID"].includes(query.error.code) ? "Customer not found" : "Unable to load customer data"} message={query.error.message} onRetry={!['NOT_FOUND', 'INVALID_ID'].includes(query.error.code) ? () => query.refetch() : undefined} />;
  return null;
}
export function InformationCard({ title, fields }) {
  return <Card className="customer-card" component="section"><h2>{title}</h2><dl className="customer-fields">{fields.map(([label, value]) => <div key={label}><dt>{label}</dt><dd>{value === null || value === undefined || value === '' ? '—' : value}</dd></div>)}</dl></Card>;
}
export function FinancialSummary({ items, currency }) {
  return <div className="customer-summary">{items.map(([label, amount, highlight, count]) => <Card key={label} className={`customer-summary-card ${highlight ? 'customer-summary-highlight' : ''}`}><span>{label}</span><strong>{count ? (amount ?? '—') : formatCurrency(amount, currency)}</strong></Card>)}</div>;
}
