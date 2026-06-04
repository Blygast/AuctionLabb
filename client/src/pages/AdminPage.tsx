import { useState } from 'react';
import { adminService } from '../services/adminService';
import { useAdminData } from '../hooks/useAdminData';
import Spinner from '../components/Spinner';
import UserTable from '../components/UserTable';
import AuctionTable from '../components/AuctionTable';

type Tab = 'users' | 'auctions';

const tabClass = (active: boolean) =>
  `px-4 py-2 text-sm font-medium border-b-2 ${active ? 'border-primary-600 text-primary-600' : 'border-transparent text-gray-500 hover:text-gray-700'}`;

export default function AdminPage() {
  const [tab, setTab] = useState<Tab>('users');
  const { users, auctions, loading, refetch } = useAdminData();

  const toggle = async (id: number, activate: boolean, kind: 'user' | 'auction') => {
    if (kind === 'user') {
      if (activate) await adminService.activateUser(id);
      else await adminService.deactivateUser(id);
    } else {
      if (activate) await adminService.activateAuction(id);
      else await adminService.deactivateAuction(id);
    }
    refetch();
  };

  if (loading) {
    return <div className="flex justify-center py-12"><Spinner size="md" /></div>;
  }

  return (
    <div className="py-6">
      <h1 className="text-2xl font-semibold text-gray-900 dark:text-white mb-6">Admin Panel</h1>

      <div className="flex border-b border-gray-200 dark:border-gray-700 mb-6">
        <button onClick={() => setTab('users')} className={tabClass(tab === 'users')}>
          Users ({users.length})
        </button>
        <button onClick={() => setTab('auctions')} className={tabClass(tab === 'auctions')}>
          Auctions ({auctions.length})
        </button>
      </div>

      {tab === 'users'
        ? <UserTable users={users} onToggle={(id, activate) => toggle(id, activate, 'user')} />
        : <AuctionTable auctions={auctions} onToggle={(id, activate) => toggle(id, activate, 'auction')} />
      }
    </div>
  );
}
