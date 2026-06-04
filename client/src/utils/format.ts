// Small, pure formatters shared across pages and components. Keep these free
// of React/DOM dependencies so they're easy to unit-test.

const CURRENCY = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const DATE = new Intl.DateTimeFormat('en-US', {
  year: 'numeric', month: 'short', day: 'numeric',
  hour: '2-digit', minute: '2-digit',
});

const DATE_SHORT = new Intl.DateTimeFormat('en-US', {
  month: 'short', day: 'numeric', year: 'numeric',
});

/** "$1,234.50" — for any monetary display. */
export function formatPrice(value: number | null | undefined): string {
  return value == null ? '—' : CURRENCY.format(value);
}

/** "Jun 15, 2026, 10:00 AM" — for timestamps with a time. */
export function formatDateTime(iso: string): string {
  return DATE.format(new Date(iso));
}

/** "Jun 15, 2026" — for date-only displays. */
export function formatDate(iso: string): string {
  return DATE_SHORT.format(new Date(iso));
}

/** "1.4 KB" / "12.0 MB" — for file sizes. */
export function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
