import { Footer } from "@/components/layout/Footer";
import { Navbar } from "@/components/layout/Navbar";
import { TourProvider } from "@/components/tour/TourProvider";

export default function SiteLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <TourProvider>
      <Navbar />
      {children}
      <Footer />
    </TourProvider>
  );
}
