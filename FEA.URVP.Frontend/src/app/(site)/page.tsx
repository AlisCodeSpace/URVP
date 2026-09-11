import { FeatureNav } from "@/components/home/FeatureNav";
import { Hero } from "@/components/home/Hero";
import { Intro } from "@/components/home/Intro";
import { RollingBanners } from "@/components/home/RollingBanners";
import { Testimonials } from "@/components/home/Testimonials";

export default function Home() {
  return (
    <main className="flex-1">
      <Hero />
      <Intro />
      <FeatureNav />
      <Testimonials />
      <RollingBanners />
    </main>
  );
}
