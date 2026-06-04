import type { AdminAuction } from '../types';
import { formatPrice } from '../utils/format';

interface Props {
  auctions: AdminAuction[];
  onToggle: (id: number, activate: boolean) => void;
}

const cellBase = 'px-6 py-4';
const headerBase = 'px-6 py-3';

export default function AuctionTable({ auctions, onToggle }: Props) {
  return (
    <div className="bg-white dark:bg-gray-800 shadow-md sm:rounded-lg overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-sm text-left text-gray-500 dark:text-gray-400">
          <thead className="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
            <tr>
              <th className={headerBase}>Title</th>
              <th className={headerBase}>Seller</th>
              <th className={headerBase}>Status</th>
              <th className={headerBase}>Price</th>
              <th className={headerBase}>Bids</th>
              <th className={headerBase}>Action</th>
            </tr>
          </thead>
          <tbody>
            {auctions.map((a) => (
              <tr key={a.id} className="bg-white border-b dark:bg-gray-800 dark:border-gray-700">
                <td className={`${cellBase} font-medium text-gray-900 dark:text-white max-w-[200px] truncate`}>{a.title}</td>
                <td className={cellBase}>{a.userName}</td>
                <td className={cellBase}>
                  <div className="flex flex-col gap-1">
                    <span className={`px-2 py-0.5 rounded text-xs font-medium w-fit ${a.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300' : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'}`}>
                      {a.isActive ? 'Active' : 'Deactivated'}
                    </span>
                    <span className={`px-2 py-0.5 rounded text-xs font-medium w-fit ${a.isOpen ? 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300' : 'bg-gray-100 text-gray-800 dark:bg-gray-600 dark:text-gray-300'}`}>
                      {a.isOpen ? 'Open' : 'Ended'}
                    </span>
                  </div>
                </td>
                <td className={`${cellBase} font-semibold text-primary-600 dark:text-primary-500`}>
                  {formatPrice(a.highestBid ?? a.startingPrice)}
                </td>
                <td className={cellBase}>{a.bidCount}</td>
                <td className={cellBase}>
                  <button onClick={() => onToggle(a.id, !a.isActive)}
                    className={`text-xs font-medium px-3 py-1.5 rounded-lg ${a.isActive ? 'text-red-600 bg-red-50 hover:bg-red-100 dark:text-red-400 dark:bg-red-900/30' : 'text-green-600 bg-green-50 hover:bg-green-100 dark:text-green-400 dark:bg-green-900/30'}`}>
                    {a.isActive ? 'Deactivate' : 'Activate'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
