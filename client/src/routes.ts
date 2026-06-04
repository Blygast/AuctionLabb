/**
 * Centralized route paths. Components import from here instead of hardcoding
 * strings, so a future rename is a one-line change.
 *
 * Use the `match` helper for any path that should be active on sub-routes
 * (e.g. /auction/:id should highlight the "Auctions" link).
 */
export const ROUTES = {
  auctions: '/',
  login: '/login',
  register: '/register',
  create: '/create',
  profile: '/profile',
  admin: '/admin',
  auctionDetail: (id: number | string = ':id') => `/auction/${id}`,
  auctionEdit: (id: number | string = ':id') => `/auction/${id}/edit`,
} as const;

/** True if `pathname` equals or starts with `path` (followed by '/' or end). */
export function isPathActive(pathname: string, path: string): boolean {
  if (path === '/') return pathname === '/';
  return pathname === path || pathname.startsWith(`${path}/`);
}
