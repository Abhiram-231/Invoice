import { Button } from '@mui/material';
import { Inventory2Outlined, ErrorOutline } from '@mui/icons-material';
import { Link } from 'react-router-dom';

export function ProductErrorState({ onRetry }) {
  return <div className="product-state" role="alert"><ErrorOutline /><h2>Unable to load products</h2><p>Something went wrong while loading the product catalog.</p><Button variant="outlined" onClick={onRetry}>Try Again</Button></div>;
}

export function ProductEmptyState({ filtered, onClear }) {
  return <div className="product-state"><Inventory2Outlined /><h2>{filtered ? 'No matching products' : 'No products found'}</h2><p>{filtered ? 'Try changing your search or filters.' : 'Products and services you create will appear here.'}</p>
    {filtered ? <Button variant="outlined" onClick={onClear}>Clear Filters</Button> : <Button component={Link} to="/products/new" variant="contained">+ Add Product</Button>}
  </div>;
}
