import { useState, useEffect, type ReactNode } from 'react';
import { authService } from '../services/authService';
import { AuthContext, type AuthContextType } from './authContextValue';
import type { AuthUser } from '../types';

export function AuthProvider({ children }: { children: ReactNode }) {
  // Initialize loading from token presence: a stored token means we'll fetch /me
  // in the effect, so the app should show a spinner until that resolves.
  const [loading, setLoading] = useState<boolean>(() => Boolean(localStorage.getItem('token')));
  const [user, setUser] = useState<AuthUser | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) return;

    let cancelled = false;
    authService
      .me()
      .then((u) => {
        if (!cancelled) setUser(u);
      })
      .catch(() => {
        if (!cancelled) localStorage.removeItem('token');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  const login = async (email: string, password: string) => {
    const res = await authService.login(email, password);
    localStorage.setItem('token', res.token);
    setUser({ userId: res.userId, name: res.name, email: res.email, role: res.role || 'User' });
  };

  const register = async (name: string, email: string, password: string) => {
    const res = await authService.register(name, email, password);
    localStorage.setItem('token', res.token);
    setUser({ userId: res.userId, name: res.name, email: res.email, role: res.role || 'User' });
  };

  const logout = () => {
    localStorage.removeItem('token');
    setUser(null);
  };

  const value: AuthContextType = {
    user, isAuthenticated: !!user, isAdmin: user?.role === 'Admin',
    loading, login, register, logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
