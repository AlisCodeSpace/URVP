import type { Metadata } from "next";
import { SignInView } from "@/components/auth/SignInView";

export const metadata: Metadata = {
  title: "Sign In | URVP",
  description: "Sign in to the URVP portal with your AUB account.",
};

export default function SignInPage() {
  return <SignInView />;
}
