import { useParams, useNavigate, Link } from 'react-router-dom';
import { auctionService } from '../services/auctionService';
import { useAuctionDetail } from '../hooks/useAuctionDetail';
import { useAuth } from '../context/useAuth';
import { getErrorMessage } from '../utils/errors';
import { formatPrice, formatDate, formatDateTime } from '../utils/format';
import { ROUTES } from '../routes';
import Spinner from '../components/Spinner';
import BidList from '../components/BidList';
import AttachmentViewer from '../components/AttachmentViewer';
import AuctionStatusBadge from '../components/AuctionStatusBadge';
import { BidForm, OwnerBanner, ClosedBanner, SignInPrompt, WinningBidBanner } from '../components/BidForm';
import type { AuctionStatusKind } from '../components/auctionStatus';

export default function AuctionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const auctionId = id ? Number(id) : undefined;
  const { auction, loading, error, refetchAuction, refetchBids } = useAuctionDetail(auctionId);

  const handleCancelBid = async (bidId: number) => {
    try {
      if (!auctionId) return;
      await auctionService.cancelBid(auctionId, bidId);
      refetchAuction(); refetchBids();
    } catch (err: unknown) {
      alert(getErrorMessage(err, 'Failed to cancel bid.'));
    }
  };

  if (loading) {
    return <div className="flex justify-center items-center min-h-[60vh]"><Spinner size="lg" /></div>;
  }

  if (error || !auction) {
    return (
      <div className="text-center py-12">
        <p className="text-red-500 text-lg">{error}</p>
        <button onClick={() => navigate(ROUTES.auctions)} className="mt-4 text-primary-600 hover:underline dark:text-primary-500">
          Back to auctions
        </button>
      </div>
    );
  }

  const isOwner = isAuthenticated && user?.userId === auction.userId;
  const isOpen = auction.isOpen;
  const isClosed = !isOpen;
  const currentPrice = auction.winningBid?.amount ?? auction.highestBid ?? auction.startingPrice;
  const minBid = auction.highestBid ?? auction.startingPrice;
  const userLatestBid = auction.bids.length > 0 && auction.bids[0].userId === user?.userId ? auction.bids[0] : null;
  const status: AuctionStatusKind = !auction.isActive ? 'Deactivated' : isOpen ? 'Open' : 'Closed';

  return (
    <div className="py-6">
      <button onClick={() => navigate(ROUTES.auctions)} className="inline-flex items-center text-sm font-medium text-primary-600 hover:underline dark:text-primary-500 mb-4">
        <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7" /></svg>
        Back to auctions
      </button>

      <div className="bg-white dark:bg-gray-800 relative shadow-md sm:rounded-lg overflow-hidden">
        <div className="p-6">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6">
            <div>
              <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">{auction.title}</h1>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">by {auction.userName}</p>
            </div>
            <div className="flex items-center gap-2 flex-wrap">
              <AuctionStatusBadge status={status} size="md" />
              {isOwner && isOpen && (
                <Link to={ROUTES.auctionEdit(auction.id)} className="text-sm font-medium text-primary-600 hover:underline dark:text-primary-500 inline-flex items-center">
                  <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
                  Edit
                </Link>
              )}
            </div>
          </div>

          <p className="text-base font-normal text-gray-500 dark:text-gray-400 mb-6 leading-relaxed">{auction.description}</p>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <StatTile label={isClosed && auction.isActive ? 'Winning Bid' : 'Current Price'} value={formatPrice(currentPrice)} accent />
            <StatTile label="Starting Price" value={formatPrice(auction.startingPrice)} />
            <StatTile label="Starts" value={formatDate(auction.startDate)} small />
            <StatTile label="Ends" value={formatDateTime(auction.endDate)} small />
          </div>

          {isOwner && <OwnerBanner />}
          {isClosed && <ClosedBanner />}
          {isClosed && auction.isActive && auction.winningBid && <WinningBidBanner bid={auction.winningBid} />}

          {isAuthenticated && !isOwner && isOpen && auctionId && (
            <BidForm auctionId={auctionId} minBid={minBid} onSuccess={() => { refetchAuction(); refetchBids(); }} />
          )}
          {!isAuthenticated && isOpen && <SignInPrompt message="to place a bid on this auction." />}

          {auction.attachments && auction.attachments.length > 0 && (
            <div className="border-t border-gray-200 dark:border-gray-700 pt-6 mt-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Attachments <span className="text-gray-400 dark:text-gray-500 font-normal">({auction.attachments.length})</span>
              </h3>
              <AttachmentViewer attachments={auction.attachments} />
            </div>
          )}

          {isOpen && (
            <div className="border-t border-gray-200 dark:border-gray-700 pt-6 mt-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Bids <span className="text-gray-400 dark:text-gray-500 font-normal">({auction.bids.length})</span>
              </h3>
              {isAuthenticated && userLatestBid && (
                <div className="mb-4 flex items-center justify-between bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg p-3">
                  <span className="text-sm text-yellow-800 dark:text-yellow-300">
                    Your latest bid: <strong>{formatPrice(userLatestBid.amount)}</strong>
                  </span>
                  <button onClick={() => handleCancelBid(userLatestBid.id)}
                    className="text-xs font-medium text-red-600 hover:text-red-800 dark:text-red-400 px-3 py-1 rounded border border-red-200 dark:border-red-800 hover:bg-red-50 dark:hover:bg-red-900/30">
                    Cancel Bid
                  </button>
                </div>
              )}
              <BidList bids={auction.bids} />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

interface StatTileProps {
  label: string;
  value: string;
  accent?: boolean;
  small?: boolean;
}

function StatTile({ label, value, accent, small }: StatTileProps) {
  const baseColor = accent
    ? 'bg-primary-50 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300'
    : 'bg-gray-50 dark:bg-gray-700 text-gray-900 dark:text-white';
  const labelColor = accent
    ? 'text-primary-500 dark:text-primary-400'
    : 'text-gray-500 dark:text-gray-400';
  const valueSize = small ? 'text-sm font-medium' : accent ? 'text-2xl font-bold' : 'text-lg font-semibold';
  return (
    <div className={`rounded-lg p-4 text-center ${baseColor}`}>
      <dt className={`text-xs font-normal uppercase tracking-wider ${labelColor}`}>{label}</dt>
      <dd className={valueSize}>{value}</dd>
    </div>
  );
}
