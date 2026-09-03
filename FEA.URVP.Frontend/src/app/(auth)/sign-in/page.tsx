import type { Metadata } from "next";
import { Suspense } from "react";
import { RequireGuest } from "@/components/auth/RequireGuest";
import { SignInView } from "@/components/auth/SignInView";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Sign In | URVP",
  description: "Sign in to the URVP portal with your AUB account.",
};

export default function SignInPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <RequireGuest>
        <SignInView />
      </RequireGuest>
    </Suspense>
  );
}
