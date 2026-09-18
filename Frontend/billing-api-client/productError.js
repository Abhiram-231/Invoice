// Product-only error presentation; shared authentication behavior is unchanged.
export function productRequestError(error, fallback) {
  const status = error?.response?.status ?? error?.status;
  let message = fallback;
  if (status === 401) message = 'Please sign in to access products.';
  else if (status === 403) message = 'You do not have permission to perform this product action.';
  else if (status === 404) message = 'Product or requested resource not found. Refresh and try again.';
  else if (status >= 500) message = 'The product service is unavailable. Please try again.';
  else if (!status && (error?.code === 'ERR_NETWORK' || error?.code === 'ECONNABORTED' || error?.message === 'Network Error')) message = 'Unable to reach the product service. Check your connection and try again.';
  else if ([400, 409, 422].includes(status)) {
    const data = error?.response?.data;
    const messages = [data?.errors && Object.values(data.errors).flat(), data?.message, data?.title].flat().filter(value =>
      typeof value === 'string' && value.trim() && value.length <= 240 &&
      !/[<>]|stack\s*trace|exception|\bat\s+\S+\(|\bselect\b.+\bfrom\b/i.test(value));
    message = messages.length ? [...new Set(messages)].join(' ') : status === 409 ? 'This product has changed or its code already exists. Refresh and try again.' : 'Check the product values and try again.';
  }
  return Object.assign(new Error(message), { code: error?.code, status });
}
