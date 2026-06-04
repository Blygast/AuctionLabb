import axios from 'axios';
import { SERVER_URL } from '../config';

/**
 * Pre-configured axios instance.
 *  - All requests get the JWT (if any) attached automatically.
 *  - `baseURL` is the server root + `/api`; services hit paths like `/auctions`.
 */
const http = axios.create({
  baseURL: `${SERVER_URL}/api`,
  headers: { 'Content-Type': 'application/json' },
});

http.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export default http;
