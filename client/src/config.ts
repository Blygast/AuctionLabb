/**
 * Build-time configuration. All values are baked in at build time, so
 * nothing here is a runtime secret.
 */
const apiUrl = import.meta.env.VITE_API_URL ?? 'https://localhost:5001';

export const SERVER_URL = apiUrl;
