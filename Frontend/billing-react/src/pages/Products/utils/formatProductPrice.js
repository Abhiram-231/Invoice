export function formatProductPrice(price, currency = 'INR') {
  if (price == null || price === '' || !Number.isFinite(Number(price))) return '-';
  const value = Number(price);
  try {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: currency || 'INR', minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
  } catch {
    return `${currency} ${value.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
