import http from './http';
import type { AdminAuction, AdminUser } from '../types';


export const adminService = {
  async listUsers(): Promise<AdminUser[]> {
    const res = await http.get<AdminUser[]>('/admin/users');
    return res.data;
  },

  async listAuctions(): Promise<AdminAuction[]> {
    const res = await http.get<AdminAuction[]>('/admin/auctions');
    return res.data;
  },

  async activateUser(id: number): Promise<void> {
    await http.put(`/admin/users/${id}/activate`);
  },

  async deactivateUser(id: number): Promise<void> {
    await http.put(`/admin/users/${id}/deactivate`);
  },

  async activateAuction(id: number): Promise<void> {
    await http.put(`/auctions/${id}/activate`);
  },

  async deactivateAuction(id: number): Promise<void> {
    await http.put(`/auctions/${id}/deactivate`);
  },
};
