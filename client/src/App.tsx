import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
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
    <AuthProvider>
      <BrowserRouter>
        <div className="bg-gray-50 dark:bg-gray-900 min-h-screen">
          <Navbar />
          <Sidebar />
          <div className="p-4 lg:ml-64 pt-20">
            <div className="max-w-7xl mx-auto">
              <Routes>
                <Route path="/" element={<AuctionsPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/auction/:id" element={<AuctionDetailPage />} />
                <Route
                  path="/create"
                  element={
                    <PrivateRoute>
                      <CreateAuctionPage />
                    </PrivateRoute>
                  }
                />
                <Route
                  path="/profile"
                  element={
                    <PrivateRoute>
                      <ProfilePage />
                    </PrivateRoute>
                  }
                />
                <Route
                  path="/admin"
                  element={
                    <AdminRoute>
                      <AdminPage />
                    </AdminRoute>
                  }
                />
                <Route
                  path="/auction/:id/edit"
                  element={
                    <PrivateRoute>
                      <EditAuctionPage />
                    </PrivateRoute>
                  }
                />
              </Routes>
            </div>
          </div>
        </div>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
