"use client";

/**
 * SignalR placeholder. v1 UI works with manual refresh only.
 * Do not block notification delivery on a live connection.
 */
export function useRealTimeNotifications(_options: { enabled?: boolean } = {}) {
  return { connected: false };
}
