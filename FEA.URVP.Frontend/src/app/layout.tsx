import type { Metadata, Viewport } from "next";
import { AuthProvider } from "@/components/auth/AuthProvider";
import { AUB_STAMP_SRC } from "@/components/ui/Logo";
import "./globals.css";

export const metadata: Metadata = {
  title: "URVP | Undergraduate Research Volunteer Program",
  description:
    "Match with faculty research. Shape your academic path. Undergraduate Research Volunteer Program – AY 2026–27.",
  icons: {
    icon: [{ url: AUB_STAMP_SRC, type: "image/png" }],
    apple: [{ url: AUB_STAMP_SRC, type: "image/png" }],
    shortcut: AUB_STAMP_SRC,
  },
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  viewportFit: "cover",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="h-full antialiased">
      <body className="min-h-full flex flex-col">
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  );
}
