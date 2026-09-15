import { Skeleton } from '@mui/material';
import { Inventory2Outlined, CheckCircleOutline, PauseCircleOutline, DesignServicesOutlined } from '@mui/icons-material';

const cards = [['total', 'Total Products', Inventory2Outlined], ['active', 'Active Products', CheckCircleOutline], ['inactive', 'Inactive Products', PauseCircleOutline], ['services', 'Services', DesignServicesOutlined]];

export function ProductSummaryCards({ summary }) {
  return <section className="product-summary" aria-label="Catalog summary">{cards.map(([key, label, Icon]) => <div className={`product-summary-card tone-${key}`} key={key}><span className="product-summary-icon"><Icon /></span><div><span>{label}</span><strong>{summary ? summary[key] : <Skeleton width={45} />}</strong><small>{{ total: 'Across your catalog', active: 'Available for billing', inactive: 'Currently unavailable', services: 'Expertise & subscriptions' }[key]}</small></div></div>)}</section>;
}
