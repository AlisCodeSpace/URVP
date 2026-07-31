import type { Metadata } from "next";
import { RequireGuest } from "@/components/auth/RequireGuest";
import { SignInView } from "@/components/auth/SignInView";
import { getAzureAdSignInUrl } from "@/lib/auth";

export const metadata: Metadata = {
  title: "Sign In | URVP",
  description: "Sign in to the URVP portal with your AUB account.",
};

type SignInPageProps = {
  searchParams: Promise<{ error?: string }>;
};

export default async function SignInPage({ searchParams }: SignInPageProps) {
  const params = await searchParams;

  return (
    <RequireGuest>
      <SignInView
        signInUrl={getAzureAdSignInUrl()}
        error={params.error}
      />
    </RequireGuest>
  );
}
