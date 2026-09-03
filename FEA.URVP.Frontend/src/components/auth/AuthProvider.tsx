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
import { onUnauthorized } from "@/lib/api";
import {
  fetchAuthStatus,
  getAzureAdSignOutUrl,
  type AuthStatus,
} from "@/lib/auth";

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

  // The backend rejected a call as unauthenticated, so the cached status is stale. Dropping it to
  // signed-out clears the session metadata this provider holds and lets the route guards move the
  // user to sign-in. The guards are UX only; the backend already refused the request.
  useEffect(
    () =>
      onUnauthorized(() => {
        setStatus({ isAuthenticated: false });
        setLoading(false);
      }),
    [],
  );

  const signOut = useCallback(() => {
    window.location.href = getAzureAdSignOutUrl();
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
