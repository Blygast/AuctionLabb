interface Bid {
  id: number;
  amount: number;
  bidDate: string;
  userId: number;
  userName: string;
}

interface BidListProps {
  bids: Bid[];
}

export default function BidList({ bids }: BidListProps) {
  if (bids.length === 0) {
    return (
      <div className="text-center py-8">
        <svg className="mx-auto mb-3 w-10 h-10 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p className="text-gray-500 dark:text-gray-400 text-lg">No bids yet</p>
        <p className="text-sm text-gray-400 dark:text-gray-500 mt-1">Be the first to place a bid!</p>
      </div>
    );
  }

  return (
    <div className="relative overflow-x-auto">
      <table className="w-full text-sm text-left text-gray-500 dark:text-gray-400">
        <thead className="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
          <tr>
            <th scope="col" className="px-6 py-3">Bidder</th>
            <th scope="col" className="px-6 py-3">Amount</th>
            <th scope="col" className="px-6 py-3">Date</th>
          </tr>
        </thead>
        <tbody>
          {bids.map((bid) => (
            <tr key={bid.id} className="bg-white border-b dark:bg-gray-800 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600">
              <td className="px-6 py-4 font-medium text-gray-900 whitespace-nowrap dark:text-white">
                <div className="flex items-center">
                  <div className="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-900 flex items-center justify-center text-primary-700 dark:text-primary-300 text-xs font-medium mr-3">
                    {bid.userName.charAt(0).toUpperCase()}
                  </div>
                  {bid.userName}
                </div>
              </td>
              <td className="px-6 py-4 font-semibold text-primary-600 dark:text-primary-500">${bid.amount.toFixed(2)}</td>
              <td className="px-6 py-4">
                {new Date(bid.bidDate).toLocaleDateString('en-US', {
                  year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit'
                })}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
