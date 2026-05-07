import { useState, useEffect } from 'react';
import api from '../api/axios';
import AuctionCard from '../components/AuctionCard';

interface Attachment { id: number; fileName: string; contentType: string; fileSize: number; uploadedAt: string; url: string; }
interface Auction {
  id: number; title: string; description: string; startingPrice: number;
  endDate: string; isOpen: boolean; isActive: boolean; userName: string;
  highestBid: number | null; attachments: Attachment[];
}

export default function AuctionsPage() {
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('open');
  const [loading, setLoading] = useState(true);

  useEffect(() => { fetchAuctions(); }, [status]);

  const fetchAuctions = async (query?: string, s?: string) => {
    setLoading(true);
    try {
      const params: any = { status: s || status };
      if (query) params.search = query;
      const res = await api.get('/auctions', { params });
      setAuctions(res.data);
    } catch { setAuctions([]); }
    finally { setLoading(false); }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchAuctions(search);
  };

  const clearSearch = () => { setSearch(''); fetchAuctions(undefined, status); };

  const statusTabs = [
    { value: 'open', label: 'Open' },
    { value: 'closed', label: 'Closed' },
    { value: 'all', label: 'All' },
  ];

  return (
    <div className="py-6">
      <div className="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">Auctions</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">Browse and bid on auctions</p>
        </div>
        <div className="flex border-b border-gray-200 dark:border-gray-700">
          {statusTabs.map((tab) => (
            <button key={tab.value} onClick={() => setStatus(tab.value)}
              className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
                status === tab.value
                  ? 'border-primary-600 text-primary-600 dark:text-primary-500 dark:border-primary-500'
                  : 'border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200'
              }`}>
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      <div className="bg-white dark:bg-gray-800 relative shadow-md sm:rounded-lg mb-6">
        <form onSubmit={handleSearch} className="flex flex-col sm:flex-row items-center p-4 gap-3">
          <div className="relative flex-1 w-full">
            <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
              <svg className="w-5 h-5 text-gray-500 dark:text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
              </svg>
            </div>
            <input type="text" value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search auctions by title..."
              className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full pl-10 p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white" />
          </div>
          <button type="submit" className="text-white bg-primary-700 hover:bg-primary-800 focus:ring-4 focus:ring-primary-300 font-medium rounded-lg text-sm px-5 py-2.5 dark:bg-primary-600 dark:hover:bg-primary-700">
            Search
          </button>
          {search && (
            <button type="button" onClick={clearSearch}
              className="text-gray-900 bg-white border border-gray-300 hover:bg-gray-100 font-medium rounded-lg text-sm px-5 py-2.5 dark:bg-gray-800 dark:text-white dark:border-gray-600 dark:hover:bg-gray-700">
              Clear
            </button>
          )}
        </form>
      </div>

      {loading ? (
        <div className="flex justify-center py-12">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600" />
        </div>
      ) : auctions.length === 0 ? (
        <div className="text-center py-12 bg-white dark:bg-gray-800 rounded-lg shadow">
          <h3 className="text-lg font-medium text-gray-900 dark:text-white">No auctions found</h3>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">Try a different search or filter</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
          {auctions.map((a) => <AuctionCard key={a.id} {...a} />)}
        </div>
      )}
    </div>
  );
}
