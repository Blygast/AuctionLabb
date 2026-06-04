import type { AdminUser } from '../types';

interface Props {
  users: AdminUser[];
  onToggle: (id: number, activate: boolean) => void;
}

const cellBase = 'px-6 py-4';
const headerBase = 'px-6 py-3';

export default function UserTable({ users, onToggle }: Props) {
  return (
    <div className="bg-white dark:bg-gray-800 shadow-md sm:rounded-lg overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-sm text-left text-gray-500 dark:text-gray-400">
          <thead className="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
            <tr>
              <th className={headerBase}>Name</th>
              <th className={headerBase}>Email</th>
              <th className={headerBase}>Role</th>
              <th className={headerBase}>Status</th>
              <th className={headerBase}>Auctions</th>
              <th className={headerBase}>Bids</th>
              <th className={headerBase}>Action</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} className="bg-white border-b dark:bg-gray-800 dark:border-gray-700">
                <td className={`${cellBase} font-medium text-gray-900 dark:text-white`}>{u.name}</td>
                <td className={cellBase}>{u.email}</td>
                <td className={cellBase}>
                  <span className={`px-2 py-0.5 rounded text-xs font-medium ${u.role === 'Admin' ? 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300' : 'bg-gray-100 text-gray-800 dark:bg-gray-600 dark:text-gray-300'}`}>
                    {u.role}
                  </span>
                </td>
                <td className={cellBase}>
                  <span className={`px-2 py-0.5 rounded text-xs font-medium ${u.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300' : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'}`}>
                    {u.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className={cellBase}>{u.auctionCount}</td>
                <td className={cellBase}>{u.bidCount}</td>
                <td className={cellBase}>
                  {u.role !== 'Admin' && (
                    <button onClick={() => onToggle(u.id, !u.isActive)}
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
  );
}
