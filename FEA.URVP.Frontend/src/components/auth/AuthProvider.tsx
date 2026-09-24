"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  type ReactNode,
} from "react";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
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
  const session = useCancellableQuery(
    async () => {
      try {
        return await fetchAuthStatus();
      } catch {
        return { isAuthenticated: false } satisfies AuthStatus;
      }
    },
    [],
    { fallbackError: "Could not read the session." },
  );
  const { setData, reload } = session;

  // The backend rejected a call as unauthenticated, so the cached status is stale. Dropping it to
  // signed-out clears the session metadata this provider holds and lets the route guards move the
  // user to sign-in. The guards are UX only; the backend already refused the request.
  useEffect(
    () =>
      onUnauthorized(() => {
        setData({ isAuthenticated: false });
      }),
    [setData],
  );

  const signOut = useCallback(() => {
    window.location.href = getAzureAdSignOutUrl();
  }, []);

  const refresh = useCallback(() => reload({ silent: true }), [reload]);

  const value = useMemo(
    () => ({
      status: session.data,
      loading: session.loading,
      refresh,
      signOut,
    }),
    [refresh, session.data, session.loading, signOut],
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
