import { Link } from 'react-router-dom';
import { IconButton, Tooltip } from '@mui/material';
import { EditOutlined, VisibilityOutlined } from '@mui/icons-material';

export function ProductActionsMenu({ product }) {
  return <div className="product-row-actions" role="group" aria-label={`Actions for ${product.name}`}>
    <Tooltip title="View Product"><IconButton component={Link} to={`/products/${encodeURIComponent(product.id)}`} size="small" className="product-action-view" aria-label={`View ${product.name}`}><VisibilityOutlined fontSize="small" /></IconButton></Tooltip>
    <Tooltip title="Edit Product"><IconButton component={Link} to={`/products/${encodeURIComponent(product.id)}/edit`} size="small" className="product-action-edit" aria-label={`Edit ${product.name}`}><EditOutlined fontSize="small" /></IconButton></Tooltip>
  </div>;
}
