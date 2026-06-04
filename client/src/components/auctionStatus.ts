/** Tailwind class for the "Open" / "Closed" / "Deactivated" status pill. */
export const AUCTION_STATUS_BADGE = {
  Open:       'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
  Closed:     'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
  Deactivated:'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300',
} as const;

/** Tailwind class for the small status dot inside the pill. */
export const AUCTION_STATUS_DOT = {
  Open:       'bg-green-500',
  Closed:     'bg-red-500',
  Deactivated:'bg-gray-500',
} as const;

export type AuctionStatusKind = keyof typeof AUCTION_STATUS_BADGE;
