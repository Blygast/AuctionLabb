import type { Bid } from '../types';
import { formatPrice } from '../utils/format';
import { useState, type FormEvent } from 'react';
import { auctionService } from '../services/auctionService';
import { getErrorMessage } from '../utils/errors';

interface BidFormProps {
  auctionId: number;
  minBid: number;
  onSuccess: () => void;
}

export function BidForm({ auctionId, minBid, onSuccess }: BidFormProps) {
  const [amount, setAmount] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const submit = async (e: FormEvent) => {
    e.preventDefault(); setError(''); setSuccess(''); setSubmitting(true);
    try {
      await auctionService.placeBid(auctionId, parseFloat(amount));
      setSuccess('Bid placed successfully!'); setAmount('');
      onSuccess();
    } catch (err: unknown) {
      setError(getErrorMessage(err, 'Failed to place bid.'));
    } finally { setSubmitting(false); }
  };

  return (
    <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-6 mb-6">
      <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Place a Bid</h3>
      {error && (
        <div className="flex items-center p-4 mb-4 text-sm text-red-800 border border-red-300 rounded-lg bg-red-50 dark:bg-gray-800 dark:text-red-400 dark:border-red-800">{error}</div>
      )}
      {success && (
        <div className="flex items-center p-4 mb-4 text-sm text-green-800 border border-green-300 rounded-lg bg-green-50 dark:bg-gray-800 dark:text-green-400 dark:border-green-800">{success}</div>
      )}
      <form onSubmit={submit} className="flex flex-col sm:flex-row gap-3">
        <div className="flex-1">
          <label className="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">
            Minimum bid: <span className="font-semibold text-gray-900 dark:text-white">{formatPrice(minBid)}</span>
          </label>
          <input type="number" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} required min={minBid + 0.01}
            placeholder={`More than ${formatPrice(minBid)}`}
            className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white" />
        </div>
        <button type="submit" disabled={submitting}
          className="text-white bg-primary-700 hover:bg-primary-800 focus:ring-4 focus:ring-primary-300 font-medium rounded-lg text-sm px-5 py-2.5 dark:bg-primary-600 dark:hover:bg-primary-700 disabled:bg-primary-400 self-end">
          {submitting ? 'Placing...' : 'Place Bid'}
        </button>
      </form>
    </div>
  );
}

const infoBanner = (message: React.ReactNode, color: string) => (
  <div className={`flex items-center p-4 mb-4 text-sm border rounded-lg ${color}`}>
    <svg className="shrink-0 inline w-4 h-4 me-3" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" /></svg>
    {message}
  </div>
);

export function OwnerBanner() {
  return infoBanner("This is your auction. You cannot place bids on your own auctions.",
    'text-yellow-800 border-yellow-300 bg-yellow-50 dark:bg-gray-800 dark:text-yellow-300 dark:border-yellow-800');
}

export function ClosedBanner() {
  return infoBanner("This auction has ended. Bidding is closed.",
    'text-gray-800 border-gray-300 bg-gray-50 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600');
}

interface SignInBannerProps {
  message: string;
}

export function SignInPrompt({ message }: SignInBannerProps) {
  return infoBanner(
    <><a href="/login" className="underline font-medium">Sign in</a>&nbsp;{message}</>,
    'text-blue-800 border-blue-300 bg-blue-50 dark:bg-gray-800 dark:text-blue-400 dark:border-blue-800');
}

interface WinningBidBannerProps {
  bid: Bid;
}

export function WinningBidBanner({ bid }: WinningBidBannerProps) {
  return (
    <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-4 mb-4">
      <h4 className="text-sm font-semibold text-green-800 dark:text-green-300 mb-2">Winning Bid</h4>
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <div className="w-8 h-8 rounded-full bg-green-100 dark:bg-green-800 flex items-center justify-center text-green-700 dark:text-green-300 text-xs font-medium mr-3">
            {bid.userName.charAt(0).toUpperCase()}
          </div>
          <span className="text-sm font-medium text-gray-900 dark:text-white">{bid.userName}</span>
        </div>
        <span className="text-lg font-bold text-green-700 dark:text-green-400">{formatPrice(bid.amount)}</span>
      </div>
    </div>
  );
}
