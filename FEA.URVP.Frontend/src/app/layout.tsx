import type { Metadata } from "next";
import { Exo } from "next/font/google";
import { Theme } from "@radix-ui/themes";
import "./globals.css";

const exo = Exo({
  variable: "--font-exo",
  subsets: ["latin"],
  weight: ["300", "400", "500", "600", "700", "800"],
  display: "swap",
});

export const metadata: Metadata = {
  title: "URVP | Undergraduate Research Volunteer Program",
  description:
    "Match with faculty research projects at AUB. Undergraduate Research Volunteer Program – AY 2025-26.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={`${exo.variable} h-full antialiased`}>
      <body className={`${exo.className} min-h-full flex flex-col`}>
        <Theme
          accentColor="purple"
          grayColor="slate"
          radius="large"
          scaling="100%"
          appearance="light"
        >
          {children}
        </Theme>
      </body>
    </html>
  );
}
