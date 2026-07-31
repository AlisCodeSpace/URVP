"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import {
  fetchAuthStatus,
  getAzureAdSignOutUrl,
  type AuthStatus,
} from "@/lib/auth";
import { appBaseUrl } from "@/lib/config";

type AuthContextValue = {
  status: AuthStatus | null;
  loading: boolean;
  refresh: () => Promise<void>;
  signOut: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus | null>(null);
  const [loading, setLoading] = useState(true);

  const refresh = useCallback(async () => {
    try {
      const next = await fetchAuthStatus();
      setStatus(next);
    } catch {
      setStatus({ isAuthenticated: false });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const signOut = useCallback(() => {
    window.location.href = getAzureAdSignOutUrl(`${appBaseUrl}/`);
  }, []);

  const value = useMemo(
    () => ({ status, loading, refresh, signOut }),
    [status, loading, refresh, signOut],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
