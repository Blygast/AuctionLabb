import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Sidebar() {
  const location = useLocation();
  const { isAuthenticated, isAdmin } = useAuth();

  const isActive = (path: string) => location.pathname === path;

  const linkClass = (path: string) =>
    `flex items-center p-2 text-base font-normal text-gray-900 rounded-lg dark:text-white hover:bg-gray-100 dark:hover:bg-gray-700 ${
      isActive(path) ? 'bg-gray-100 dark:bg-gray-700' : ''
    }`;

  const sectionHeader = (label: string, extra?: React.ReactNode) => (
    <div className="pt-4 pb-1 px-3">
      <div className="flex items-center p-2 text-xs font-normal text-gray-500 dark:text-gray-400">
        <span className="ml-3 uppercase tracking-wider font-semibold">{label}</span>
        {extra}
      </div>
    </div>
  );

  return (
    <>
      <aside
        id="sidebar"
        className="fixed top-0 left-0 z-20 flex flex-col flex-shrink-0 hidden w-64 h-full pt-16 font-normal duration-75 lg:flex transition-width"
        aria-label="Sidebar"
      >
        <div className="relative flex flex-col flex-1 min-h-0 pt-0 bg-white border-r border-gray-200 dark:bg-gray-800 dark:border-gray-700">
          <div className="flex flex-col flex-1 pt-5 pb-4 overflow-y-auto">
            <div className="flex-1 px-3 bg-white dark:bg-gray-800">

              {sectionHeader('Auction')}

              <ul className="space-y-1">
                <li>
                  <Link to="/" className={linkClass('/')}>
                    <svg className="w-6 h-6 text-gray-500 transition duration-75 dark:text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                      <path d="M2 10a8 8 0 018-8v8h8a8 8 0 11-16 0z" /><path d="M12 2.252A8.014 8.014 0 0117.748 8H12V2.252z" />
                    </svg>
                    <span className="ml-3">Auctions</span>
                  </Link>
                </li>
                {isAuthenticated && (
                  <li>
                    <Link to="/create" className={linkClass('/create')}>
                      <svg className="w-6 h-6 text-gray-500 transition duration-75 dark:text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd" />
                      </svg>
                      <span className="ml-3">Create Auction</span>
                    </Link>
                  </li>
                )}
              </ul>

              {sectionHeader(
                'Account',
                isAuthenticated ? (
                  <span className="ml-auto mr-3 flex items-center text-green-500 text-xs">
                    <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                    Signed in
                  </span>
                ) : undefined
              )}

              <ul className="space-y-1">
                {!isAuthenticated ? (
                  <>
                    <li>
                      <Link to="/login" className={linkClass('/login')}>
                        <svg className="w-6 h-6 text-gray-500 transition duration-75 dark:text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
                        </svg>
                        <span className="ml-3">Sign In</span>
                      </Link>
                    </li>
                    <li>
                      <Link to="/register" className={linkClass('/register')}>
                        <svg className="w-6 h-6 text-gray-500 transition duration-75 dark:text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                          <path d="M8 9a3 3 0 100-6 3 3 0 000 6zM8 11a6 6 0 016 6H2a6 6 0 016-6zM16 7a1 1 0 10-2 0v1h-1a1 1 0 100 2h1v1a1 1 0 102 0v-1h1a1 1 0 100-2h-1V7z" />
                        </svg>
                        <span className="ml-3">Register</span>
                      </Link>
                    </li>
                  </>
                ) : (
                  <li>
                    <Link to="/profile" className={linkClass('/profile')}>
                      <svg className="w-6 h-6 text-gray-500 transition duration-75 dark:text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-6-3a2 2 0 11-4 0 2 2 0 014 0zm-2 4a5 5 0 00-4.546 2.916A5.986 5.986 0 0010 16a5.986 5.986 0 004.546-2.084A5 5 0 0010 11z" clipRule="evenodd" />
                      </svg>
                      <span className="ml-3">My Profile</span>
                    </Link>
                  </li>
                )}
              </ul>

              {isAdmin && (
                <>
                  {sectionHeader('Admin')}
                  <ul className="space-y-1">
                    <li>
                      <Link to="/admin" className={linkClass('/admin')}>
                        <svg className="w-6 h-6 text-gray-500 transition duration-75 dark:text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M11.49 3.17c-.38-1.56-2.6-1.56-2.98 0a1.532 1.532 0 01-2.286.948c-1.372-.836-2.942.734-2.106 2.106.54.886.061 2.042-.947 2.287-1.561.379-1.561 2.6 0 2.978a1.532 1.532 0 01.947 2.287c-.836 1.372.734 2.942 2.106 2.106a1.532 1.532 0 012.287.947c.379 1.561 2.6 1.561 2.978 0a1.533 1.533 0 012.287-.947c1.372.836 2.942-.734 2.106-2.106a1.533 1.533 0 01.947-2.287c1.561-.379 1.561-2.6 0-2.978a1.532 1.532 0 01-.947-2.287c.836-1.372-.734-2.942-2.106-2.106a1.532 1.532 0 01-2.287-.947zM10 13a3 3 0 100-6 3 3 0 000 6z" clipRule="evenodd" />
                        </svg>
                        <span className="ml-3">Admin Panel</span>
                      </Link>
                    </li>
                  </ul>
                </>
              )}

            </div>
          </div>
        </div>
      </aside>
      <div
        className="fixed inset-0 z-10 hidden bg-gray-900/50 dark:bg-gray-900/90"
        id="sidebarBackdrop"
        onClick={() => {
          document.getElementById('sidebar')?.classList.add('hidden');
          document.getElementById('sidebarBackdrop')?.classList.add('hidden');
        }}
      />
    </>
  );
}
