import { Footer } from "@/components/layout/Footer";
import { Navbar } from "@/components/layout/Navbar";
import { NotFoundView } from "@/components/ui/NotFoundView";

export default function RootNotFound() {
  return (
    <>
      <Navbar />
      <NotFoundView />
      <Footer />
    </>
  );
}
