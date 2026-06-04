import { useEffect, useState, useCallback } from 'react';
import { adminService } from '../services/adminService';
import type { AdminAuction, AdminUser } from '../types';

interface UseAdminDataResult {
  users: AdminUser[];
  auctions: AdminAuction[];
  loading: boolean;
  refetch: () => Promise<void>;
}

export function useAdminData(): UseAdminDataResult {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [auctions, setAuctions] = useState<AdminAuction[]>([]);
  const [loading, setLoading] = useState(true);

  const refetch = useCallback(async () => {
    setLoading(true);
    try {
      const [u, a] = await Promise.all([adminService.listUsers(), adminService.listAuctions()]);
      setUsers(u);
      setAuctions(a);
    } catch {
      // Initial load failure — show empty tables; the user can retry by toggling a tab.
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    // See useAuctions for why this lint rule is suppressed.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    refetch();
  }, [refetch]);

  return { users, auctions, loading, refetch };
}
