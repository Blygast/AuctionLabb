import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ErrorBoundary } from './components/ErrorBoundary';
import { ROUTES } from './routes';
import Navbar from './components/Navbar';
import Sidebar from './components/Sidebar';
import PrivateRoute from './components/PrivateRoute';
import AdminRoute from './components/AdminRoute';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import AuctionsPage from './pages/AuctionsPage';
import AuctionDetailPage from './pages/AuctionDetailPage';
import CreateAuctionPage from './pages/CreateAuctionPage';
import ProfilePage from './pages/ProfilePage';
import AdminPage from './pages/AdminPage';
import EditAuctionPage from './pages/EditAuctionPage';

function App() {
  return (
    <ErrorBoundary>
      <AuthProvider>
        <BrowserRouter>
          <div className="bg-gray-50 dark:bg-gray-900 min-h-screen">
            <Navbar />
            <Sidebar />
            <div className="p-4 lg:ml-64 pt-20">
              <div className="max-w-7xl mx-auto">
                <Routes>
                  <Route path={ROUTES.auctions} element={<AuctionsPage />} />
                  <Route path={ROUTES.login} element={<LoginPage />} />
                  <Route path={ROUTES.register} element={<RegisterPage />} />
                  <Route path={ROUTES.auctionDetail()} element={<AuctionDetailPage />} />
                  <Route path={ROUTES.create} element={<PrivateRoute><CreateAuctionPage /></PrivateRoute>} />
                  <Route path={ROUTES.profile} element={<PrivateRoute><ProfilePage /></PrivateRoute>} />
                  <Route path={ROUTES.admin} element={<AdminRoute><AdminPage /></AdminRoute>} />
                  <Route path={ROUTES.auctionEdit()} element={<PrivateRoute><EditAuctionPage /></PrivateRoute>} />
                </Routes>
              </div>
            </div>
          </div>
        </BrowserRouter>
      </AuthProvider>
    </ErrorBoundary>
  );
}

export default App;
