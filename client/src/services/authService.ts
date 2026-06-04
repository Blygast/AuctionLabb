import http from './http';
import type { AuthResponse, AuthUser } from '../types';


export const authService = {
  async register(name: string, email: string, password: string): Promise<AuthResponse> {
    const res = await http.post<AuthResponse>('/auth/register', { name, email, password });
    return res.data;
  },

  async login(email: string, password: string): Promise<AuthResponse> {
    const res = await http.post<AuthResponse>('/auth/login', { email, password });
    return res.data;
  },

  async me(): Promise<AuthUser> {
    const res = await http.get<AuthUser>('/auth/me');
    return res.data;
  },

  async updatePassword(currentPassword: string, newPassword: string): Promise<void> {
    await http.put('/auth/password', { currentPassword, newPassword });
  },
};
