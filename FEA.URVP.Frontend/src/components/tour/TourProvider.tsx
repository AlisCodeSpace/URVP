"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  type ReactNode,
} from "react";
import { usePathname, useRouter } from "next/navigation";
import { EVENTS, STATUS, useJoyride, type Step } from "react-joyride";
import { useAuth } from "@/components/auth/AuthProvider";
import { facultyTourSteps, studentTourSteps } from "@/components/tour/tour-steps";
import { UserRole } from "@/lib/auth";
import {
  canTakeTour,
  isCurrentTourPath,
  isTourCompleted,
  isTourStartPath,
  setTourCompleted,
  tourStartHref,
  waitForTourPath,
} from "@/lib/tour";

type TourContextValue = {
  start: () => void;
};

const TourContext = createContext<TourContextValue | null>(null);

const fallbackSteps: Step[] = [
  { target: "body", content: "", skipBeacon: true },
];

export function useTour(): TourContextValue | null {
  return useContext(TourContext);
}

export function TourProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const { status, loading } = useAuth();
  const autoStarted = useRef(false);

  const go = useCallback(
    async (href: string) => {
      if (isCurrentTourPath(href)) return;
      router.push(href);
      await waitForTourPath(href);
    },
    [router],
  );
  const goRef = useRef(go);
  goRef.current = go;

  const steps = useMemo(() => {
    const navigate = (href: string) => goRef.current(href);
    if (status?.role === UserRole.Student) return studentTourSteps(navigate);
    if (status?.role === UserRole.Faculty && status.userId) {
      return facultyTourSteps(navigate, status.userId);
    }
    return [];
  }, [status?.role, status?.userId]);

  const { controls, state, Tour } = useJoyride({
    steps: steps.length > 0 ? steps : fallbackSteps,
    continuous: true,
    scrollToFirstStep: true,
    locale: {
      back: "Back",
      skip: "Skip",
      last: "Done",
      next: "Next",
      nextWithProgress: "Next ({current} of {total})",
    },
    options: {
      skipBeacon: true,
      showProgress: true,
      buttons: ["back", "skip", "primary"],
      overlayClickAction: false,
      blockTargetInteraction: true,
      primaryColor: "#662076",
      textColor: "#1a1020",
      backgroundColor: "#ffffff",
      overlayColor: "rgba(61, 18, 72, 0.55)",
      zIndex: 10000,
      targetWaitTimeout: 8000,
      beforeTimeout: 15000,
      spotlightRadius: 12,
      scrollOffset: 96,
      scrollDuration: 400,
    },
    styles: {
      tooltipTitle: {
        color: "#662076",
      },
    },
    onEvent: (event) => {
      if (event.type !== EVENTS.TOUR_END) return;
      if (!status?.userId || !canTakeTour(status.role)) return;
      setTourCompleted(status.userId, status.role!);
    },
  });

  const controlsRef = useRef(controls);
  controlsRef.current = controls;

  useEffect(() => {
    autoStarted.current = false;
  }, [status?.userId, status?.role]);

  useEffect(() => {
    if (loading || autoStarted.current || steps.length === 0) return;
    if (!status?.isAuthenticated || !status.userId || !canTakeTour(status.role)) {
      return;
    }
    if (state.status === STATUS.RUNNING || state.status === STATUS.WAITING) {
      return;
    }
    if (isTourCompleted(status.userId, status.role!)) return;
    if (!isTourStartPath(pathname, status.role)) return;

    const timer = window.setTimeout(() => {
      if (autoStarted.current) return;
      autoStarted.current = true;
      controlsRef.current.start(0);
    }, 450);
    return () => window.clearTimeout(timer);
  }, [loading, pathname, state.status, status, steps.length]);

  const start = useCallback(() => {
    if (!status?.userId || !canTakeTour(status.role) || steps.length === 0) {
      return;
    }

    const href = tourStartHref(status.role, status.userId);
    if (!href) return;

    setTourCompleted(status.userId, status.role!, false);
    autoStarted.current = true;
    void (async () => {
      await go(href);
      controlsRef.current.start(0);
    })();
  }, [go, status, steps.length]);

  const value = useMemo(() => ({ start }), [start]);

  return (
    <TourContext.Provider value={value}>
      {children}
      {steps.length > 0 ? Tour : null}
    </TourContext.Provider>
  );
}
