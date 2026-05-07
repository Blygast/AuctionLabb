import { useState, useEffect, type FormEvent } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';
import BidList from '../components/BidList';
import AttachmentViewer from '../components/AttachmentViewer';

interface Attachment { id: number; fileName: string; contentType: string; fileSize: number; uploadedAt: string; url: string; }
interface Bid { id: number; amount: number; bidDate: string; userId: number; userName: string; }
interface Auction {
  id: number; title: string; description: string; startingPrice: number;
  startDate: string; endDate: string; isOpen: boolean; isActive: boolean;
  userId: number; userName: string; highestBid: number | null;
  attachments: Attachment[]; bids: Bid[]; winningBid: Bid | null;
}

export default function AuctionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const [auction, setAuction] = useState<Auction | null>(null);
  const [bidAmount, setBidAmount] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [bidError, setBidError] = useState('');
  const [bidSuccess, setBidSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => { if (id) { fetchAuction(); fetchBids(); } }, [id]);

  const fetchAuction = async () => {
    try { const res = await api.get(`/auctions/${id}`); setAuction(res.data); }
    catch { setError('Auction not found.'); }
    finally { setLoading(false); }
  };

  const fetchBids = async () => {
    try { const res = await api.get(`/auctions/${id}/bids`); if (auction) auction.bids = res.data; }
    catch {}
  };

  const handlePlaceBid = async (e: FormEvent) => {
    e.preventDefault(); setBidError(''); setBidSuccess(''); setSubmitting(true);
    try {
      await api.post(`/auctions/${id}/bids`, { amount: parseFloat(bidAmount) });
      setBidSuccess('Bid placed successfully!'); setBidAmount('');
      fetchAuction(); fetchBids();
    } catch (err: any) { setBidError(err.response?.data || 'Failed to place bid.'); }
    finally { setSubmitting(false); }
  };

  const handleCancelBid = async (bidId: number) => {
    try {
      await api.delete(`/auctions/${id}/bids/${bidId}`);
      fetchAuction(); fetchBids();
    } catch (err: any) { alert(err.response?.data || 'Failed to cancel bid.'); }
  };

  if (loading) return <div className="flex justify-center items-center min-h-[60vh]"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600" /></div>;

  if (error || !auction) return (
    <div className="text-center py-12">
      <p className="text-red-500 text-lg">{error}</p>
      <button onClick={() => navigate('/')} className="mt-4 text-primary-600 hover:underline dark:text-primary-500">Back to auctions</button>
    </div>
  );

  const isOwner = isAuthenticated && user?.userId === auction.userId;
  const currentPrice = auction.winningBid?.amount ?? auction.highestBid ?? auction.startingPrice;
  const minBid = auction.highestBid ?? auction.startingPrice;
  const isOpen = auction.isOpen;
  const isClosed = !isOpen;

  const userLatestBid = auction.bids.length > 0 && auction.bids[0].userId === user?.userId ? auction.bids[0] : null;

  return (
    <div className="py-6">
      <button onClick={() => navigate('/')} className="inline-flex items-center text-sm font-medium text-primary-600 hover:underline dark:text-primary-500 mb-4">
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
              {isOpen ? (
                <span className="bg-green-100 text-green-800 text-sm font-medium inline-flex items-center px-3 py-1 rounded-full dark:bg-green-900 dark:text-green-300">
                  <span className="w-2 h-2 mr-1.5 rounded-full bg-green-500" /> Open
                </span>
              ) : (
                <span className="bg-gray-100 text-gray-800 text-sm font-medium inline-flex items-center px-3 py-1 rounded-full dark:bg-gray-600 dark:text-gray-300">
                  Closed
                </span>
              )}
              {!auction.isActive && (
                <span className="bg-red-100 text-red-800 text-sm font-medium inline-flex items-center px-3 py-1 rounded-full dark:bg-red-900 dark:text-red-300">
                  Deactivated
                </span>
              )}
              {isOwner && isOpen && (
                <Link to={`/auction/${auction.id}/edit`}
                  className="text-sm font-medium text-primary-600 hover:underline dark:text-primary-500 inline-flex items-center">
                  <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
                  Edit
                </Link>
              )}
            </div>
          </div>

          <p className="text-base font-normal text-gray-500 dark:text-gray-400 mb-6 leading-relaxed">{auction.description}</p>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <div className="bg-primary-50 dark:bg-primary-900/30 rounded-lg p-4 text-center">
              <dt className="text-xs font-normal text-primary-500 dark:text-primary-400 uppercase tracking-wider">
                {isClosed ? 'Winning Bid' : 'Current Price'}
              </dt>
              <dd className="text-2xl font-bold text-primary-700 dark:text-primary-300">${currentPrice.toFixed(2)}</dd>
            </div>
            <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4 text-center">
              <dt className="text-xs font-normal text-gray-500 dark:text-gray-400 uppercase tracking-wider">Starting Price</dt>
              <dd className="text-lg font-semibold text-gray-900 dark:text-white">${auction.startingPrice.toFixed(2)}</dd>
            </div>
            <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4 text-center">
              <dt className="text-xs font-normal text-gray-500 dark:text-gray-400 uppercase tracking-wider">Starts</dt>
              <dd className="text-sm font-medium text-gray-900 dark:text-white">
                {new Date(auction.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
              </dd>
            </div>
            <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4 text-center">
              <dt className="text-xs font-normal text-gray-500 dark:text-gray-400 uppercase tracking-wider">Ends</dt>
              <dd className="text-sm font-medium text-gray-900 dark:text-white">
                {new Date(auction.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
              </dd>
            </div>
          </div>

          {isOwner && (
            <div className="flex items-center p-4 mb-4 text-sm text-yellow-800 border border-yellow-300 rounded-lg bg-yellow-50 dark:bg-gray-800 dark:text-yellow-300 dark:border-yellow-800">
              <svg className="shrink-0 inline w-4 h-4 me-3" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" /></svg>
              This is your auction. You cannot place bids on your own auctions.
            </div>
          )}

          {isClosed && (
            <div className="flex items-center p-4 mb-4 text-sm text-gray-800 border border-gray-300 rounded-lg bg-gray-50 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600">
              <svg className="shrink-0 inline w-4 h-4 me-3" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" /></svg>
              This auction has ended. Bidding is closed.
            </div>
          )}

          {isClosed && auction.winningBid && (
            <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-4 mb-4">
              <h4 className="text-sm font-semibold text-green-800 dark:text-green-300 mb-2">Winning Bid</h4>
              <div className="flex items-center justify-between">
                <div className="flex items-center">
                  <div className="w-8 h-8 rounded-full bg-green-100 dark:bg-green-800 flex items-center justify-center text-green-700 dark:text-green-300 text-xs font-medium mr-3">
                    {auction.winningBid.userName.charAt(0).toUpperCase()}
                  </div>
                  <span className="text-sm font-medium text-gray-900 dark:text-white">{auction.winningBid.userName}</span>
                </div>
                <span className="text-lg font-bold text-green-700 dark:text-green-400">${auction.winningBid.amount.toFixed(2)}</span>
              </div>
            </div>
          )}

          {isAuthenticated && !isOwner && isOpen && (
            <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-6 mb-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Place a Bid</h3>
              {bidError && (
                <div className="flex items-center p-4 mb-4 text-sm text-red-800 border border-red-300 rounded-lg bg-red-50 dark:bg-gray-800 dark:text-red-400 dark:border-red-800">{bidError}</div>
              )}
              {bidSuccess && (
                <div className="flex items-center p-4 mb-4 text-sm text-green-800 border border-green-300 rounded-lg bg-green-50 dark:bg-gray-800 dark:text-green-400 dark:border-green-800">{bidSuccess}</div>
              )}
              <form onSubmit={handlePlaceBid} className="flex flex-col sm:flex-row gap-3">
                <div className="flex-1">
                  <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">
                    Minimum bid: <span className="font-semibold text-gray-900 dark:text-white">${minBid.toFixed(2)}</span>
                  </label>
                  <input type="number" step="0.01" value={bidAmount} onChange={(e) => setBidAmount(e.target.value)} required min={minBid + 0.01}
                    placeholder={`More than $${minBid.toFixed(2)}`}
                    className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white" />
                </div>
                <button type="submit" disabled={submitting}
                  className="text-white bg-primary-700 hover:bg-primary-800 focus:ring-4 focus:ring-primary-300 font-medium rounded-lg text-sm px-5 py-2.5 dark:bg-primary-600 dark:hover:bg-primary-700 disabled:bg-primary-400 self-end">
                  {submitting ? 'Placing...' : 'Place Bid'}
                </button>
              </form>
            </div>
          )}

          {!isAuthenticated && isOpen && (
            <div className="flex items-center p-4 mb-4 text-sm text-blue-800 border border-blue-300 rounded-lg bg-blue-50 dark:bg-gray-800 dark:text-blue-400 dark:border-blue-800">
              <svg className="shrink-0 inline w-4 h-4 me-3" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" /></svg>
              <a href="/login" className="underline font-medium">Sign in</a>&nbsp;to place a bid on this auction.
            </div>
          )}

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
                    Your latest bid: <strong>${userLatestBid.amount.toFixed(2)}</strong>
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
