import { Link } from 'react-router-dom';
import type { Auction } from '../types';
import { SERVER_URL } from '../config';
import { formatPrice, formatDateTime } from '../utils/format';
import AuctionStatusBadge from './AuctionStatusBadge';
import type { AuctionStatusKind } from './auctionStatus';

export default function AuctionCard({
  id, title, description, startingPrice, endDate,
  isOpen, isActive = true, userName, highestBid, attachments,
}: Auction) {
  const status: AuctionStatusKind = !isActive
    ? 'Deactivated'
    : isOpen ? 'Open' : 'Closed';

  const firstImage = attachments?.find((a) => a.contentType.startsWith('image/'));
  const fileCount = attachments?.length ?? 0;

  return (
    <Link
      to={`/auction/${id}`}
      className="block bg-white rounded-lg shadow-md border border-gray-200 dark:bg-gray-800 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors overflow-hidden"
    >
      {firstImage ? (
        <div className="h-40 overflow-hidden bg-gray-100 dark:bg-gray-700">
          <img src={`${SERVER_URL}${firstImage.url}`} alt={title} className="w-full h-full object-cover" />
        </div>
      ) : (
        <div className="h-40 bg-gray-100 dark:bg-gray-700 flex items-center justify-center">
          <svg className="w-12 h-12 text-gray-300 dark:text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
        </div>
      )}

      <div className="p-5">
        <div className="flex justify-between items-start mb-3">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white line-clamp-1">{title}</h3>
          <AuctionStatusBadge status={status} />
        </div>

        <p className="text-sm font-normal text-gray-500 dark:text-gray-400 mb-4 line-clamp-2">{description}</p>

        <div className="flex items-center justify-between">
          <div>
            <p className="text-xs font-normal text-gray-500 dark:text-gray-400">Current Price</p>
            <p className="text-xl font-bold text-primary-600 dark:text-primary-500">
              {formatPrice(highestBid ?? startingPrice)}
            </p>
          </div>
          <div className="text-right">
            <p className="text-xs font-normal text-gray-500 dark:text-gray-400">Seller</p>
            <p className="text-sm font-medium text-gray-900 dark:text-white">{userName}</p>
          </div>
        </div>

        <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700 flex items-center justify-between">
          <p className="text-xs font-normal text-gray-500 dark:text-gray-400">Ends: {formatDateTime(endDate)}</p>
          {fileCount > 0 && (
            <span className="text-xs text-gray-400 dark:text-gray-500 flex items-center">
              <svg className="w-3.5 h-3.5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
              </svg>
              {fileCount} file{fileCount !== 1 ? 's' : ''}
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}
