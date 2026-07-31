import type { Metadata } from "next";
import { Suspense } from "react";
import { AuthCallbackView } from "@/components/auth/AuthCallbackView";

export const metadata: Metadata = {
  title: "Signing in | URVP",
  description: "Completing AUB single sign-on.",
};

export default function AuthCallbackPage() {
  return (
    <Suspense
      fallback={
        <main className="flex min-h-dvh flex-1 items-center justify-center px-6">
          <p className="text-muted">Completing sign-in…</p>
        </main>
      }
    >
      <AuthCallbackView />
    </Suspense>
  );
}
