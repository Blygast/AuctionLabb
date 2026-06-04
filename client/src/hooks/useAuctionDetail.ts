import { useEffect, useState, useCallback } from 'react';
import { auctionService } from '../services/auctionService';
import type { Auction } from '../types';

interface UseAuctionDetailResult {
  auction: Auction | null;
  loading: boolean;
  error: string;
  refetchAuction: () => Promise<void>;
  refetchBids: () => Promise<void>;
}

export function useAuctionDetail(id: number | undefined): UseAuctionDetailResult {
  const [auction, setAuction] = useState<Auction | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const refetchAuction = useCallback(async () => {
    if (!id) return;
    try {
      const data = await auctionService.get(id);
      setAuction(data);
    } catch {
      setError('Auction not found.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  const refetchBids = useCallback(async () => {
    if (!id) return;
    try {
      const bids = await auctionService.getBids(id);
      setAuction((prev) => (prev ? { ...prev, bids } : prev));
    } catch {
      // Bids are non-critical; keep the previously-loaded list.
    }
  }, [id]);

  useEffect(() => {
    // See useAuctions for why this lint rule is suppressed.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    refetchAuction();
    refetchBids();
  }, [refetchAuction, refetchBids]);

  return { auction, loading, error, refetchAuction, refetchBids };
}
