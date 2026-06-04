import { Navigate, useLocation } from 'react-router-dom';
import type { ReactNode } from 'react';
import { useAuth } from '../context/useAuth';
import FullPageSpinner from './FullPageSpinner';

export default function AdminRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated, isAdmin, loading } = useAuth();
  const location = useLocation();

  if (loading) return <FullPageSpinner label="Checking your session..." />;
  if (!isAuthenticated) return <Navigate to="/login" state={{ from: location }} replace />;
  if (!isAdmin) return <Navigate to="/" replace />;

  return <>{children}</>;
}
