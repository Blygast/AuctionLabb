import type { AuctionStatusKind } from './auctionStatus';
import { AUCTION_STATUS_BADGE, AUCTION_STATUS_DOT } from './auctionStatus';

interface Props {
  status: AuctionStatusKind;
  size?: 'sm' | 'md';
}

/**
 * Small pill used by AuctionCard and AuctionDetailPage to show whether
 * an auction is open, closed, or deactivated.
 */
export default function AuctionStatusBadge({ status, size = 'sm' }: Props) {
  const sizing = size === 'sm'
    ? 'text-xs px-2.5 py-0.5'
    : 'text-sm px-3 py-1';

  return (
    <span className={`font-medium inline-flex items-center rounded-full ${sizing} ${AUCTION_STATUS_BADGE[status]}`}>
      <span className={`w-2 h-2 mr-1.5 rounded-full ${AUCTION_STATUS_DOT[status]}`} />
      {status}
    </span>
  );
}
