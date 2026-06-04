import { useEffect, useState, useCallback } from 'react';
import { auctionService } from '../services/auctionService';
import type { Auction, AuctionStatus } from '../types';

interface UseAuctionsResult {
  auctions: Auction[];
  loading: boolean;
  refetch: (query?: string) => Promise<void>;
}

export function useAuctions(status: AuctionStatus): UseAuctionsResult {
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [loading, setLoading] = useState(true);

  const refetch = useCallback(
    async (query?: string) => {
      setLoading(true);
      try {
        const data = await auctionService.list({ search: query, status });
        setAuctions(data);
      } catch {
        setAuctions([]);
      } finally {
        setLoading(false);
      }
    },
    [status],
  );

  useEffect(() => {
    // Standard "load on mount and when status changes" pattern. The React 19
    // set-state-in-effect rule over-flags this idiom by tracing setState calls
    // through the refetch callback.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    refetch();
  }, [refetch]);

  return { auctions, loading, refetch };
}
