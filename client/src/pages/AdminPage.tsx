import { useState, useEffect } from 'react';
import api from '../api/axios';

interface AdminUser { id: number; name: string; email: string; role: string; isActive: boolean; auctionCount: number; bidCount: number; }
interface AdminAuction { id: number; title: string; isActive: boolean; isOpen: boolean; startDate: string; endDate: string; startingPrice: number; userName: string; userId: number; bidCount: number; highestBid: number | null; }

export default function AdminPage() {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [auctions, setAuctions] = useState<AdminAuction[]>([]);
  const [tab, setTab] = useState<'users' | 'auctions'>('users');
  const [loading, setLoading] = useState(true);

  useEffect(() => { loadData(); }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [u, a] = await Promise.all([api.get('/admin/users'), api.get('/admin/auctions')]);
      setUsers(u.data); setAuctions(a.data);
    } catch {} finally { setLoading(false); }
  };

  const toggleUser = async (id: number, activate: boolean) => {
    await api.put(`/admin/users/${id}/${activate ? 'activate' : 'deactivate'}`);
    loadData();
  };

  const toggleAuction = async (id: number, activate: boolean) => {
    await api.put(`/auctions/${id}/${activate ? 'activate' : 'deactivate'}`);
    loadData();
  };

  if (loading) {
    return <div className="flex justify-center py-12"><div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600" /></div>;
  }

  return (
    <div className="py-6">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white mb-6">Admin Panel</h1>

      <div className="flex border-b border-gray-200 dark:border-gray-700 mb-6">
        <button onClick={() => setTab('users')} className={`px-4 py-2 text-sm font-medium border-b-2 ${tab === 'users' ? 'border-primary-600 text-primary-600' : 'border-transparent text-gray-500 hover:text-gray-700'}`}>
          Users ({users.length})
        </button>
        <button onClick={() => setTab('auctions')} className={`px-4 py-2 text-sm font-medium border-b-2 ${tab === 'auctions' ? 'border-primary-600 text-primary-600' : 'border-transparent text-gray-500 hover:text-gray-700'}`}>
          Auctions ({auctions.length})
        </button>
      </div>

      {tab === 'users' && (
        <div className="bg-white dark:bg-gray-800 shadow-md sm:rounded-lg overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left text-gray-500 dark:text-gray-400">
              <thead className="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
                <tr>
                  <th className="px-6 py-3">Name</th>
                  <th className="px-6 py-3">Email</th>
                  <th className="px-6 py-3">Role</th>
                  <th className="px-6 py-3">Status</th>
                  <th className="px-6 py-3">Auctions</th>
                  <th className="px-6 py-3">Bids</th>
                  <th className="px-6 py-3">Action</th>
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id} className="bg-white border-b dark:bg-gray-800 dark:border-gray-700">
                    <td className="px-6 py-4 font-medium text-gray-900 dark:text-white">{u.name}</td>
                    <td className="px-6 py-4">{u.email}</td>
                    <td className="px-6 py-4">
                      <span className={`px-2 py-0.5 rounded text-xs font-medium ${u.role === 'Admin' ? 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300' : 'bg-gray-100 text-gray-800 dark:bg-gray-600 dark:text-gray-300'}`}>
                        {u.role}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`px-2 py-0.5 rounded text-xs font-medium ${u.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300' : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'}`}>
                        {u.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-6 py-4">{u.auctionCount}</td>
                    <td className="px-6 py-4">{u.bidCount}</td>
                    <td className="px-6 py-4">
                      {u.role !== 'Admin' && (
                        <button onClick={() => toggleUser(u.id, !u.isActive)}
                          className={`text-xs font-medium px-3 py-1.5 rounded-lg ${u.isActive ? 'text-red-600 bg-red-50 hover:bg-red-100 dark:text-red-400 dark:bg-red-900/30 dark:hover:bg-red-900/50' : 'text-green-600 bg-green-50 hover:bg-green-100 dark:text-green-400 dark:bg-green-900/30 dark:hover:bg-green-900/50'}`}>
                          {u.isActive ? 'Deactivate' : 'Activate'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {tab === 'auctions' && (
        <div className="bg-white dark:bg-gray-800 shadow-md sm:rounded-lg overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left text-gray-500 dark:text-gray-400">
              <thead className="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
                <tr>
                  <th className="px-6 py-3">Title</th>
                  <th className="px-6 py-3">Seller</th>
                  <th className="px-6 py-3">Status</th>
                  <th className="px-6 py-3">Price</th>
                  <th className="px-6 py-3">Bids</th>
                  <th className="px-6 py-3">Action</th>
                </tr>
              </thead>
              <tbody>
                {auctions.map((a) => (
                  <tr key={a.id} className="bg-white border-b dark:bg-gray-800 dark:border-gray-700">
                    <td className="px-6 py-4 font-medium text-gray-900 dark:text-white max-w-[200px] truncate">{a.title}</td>
                    <td className="px-6 py-4">{a.userName}</td>
                    <td className="px-6 py-4">
                      <div className="flex flex-col gap-1">
                        <span className={`px-2 py-0.5 rounded text-xs font-medium w-fit ${a.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300' : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'}`}>
                          {a.isActive ? 'Active' : 'Deactivated'}
                        </span>
                        <span className={`px-2 py-0.5 rounded text-xs font-medium w-fit ${a.isOpen ? 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300' : 'bg-gray-100 text-gray-800 dark:bg-gray-600 dark:text-gray-300'}`}>
                          {a.isOpen ? 'Open' : 'Ended'}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 font-semibold text-primary-600 dark:text-primary-500">
                      ${(a.highestBid ?? a.startingPrice).toFixed(2)}
                    </td>
                    <td className="px-6 py-4">{a.bidCount}</td>
                    <td className="px-6 py-4">
                      <button onClick={() => toggleAuction(a.id, !a.isActive)}
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
      )}
    </div>
  );
}
