import { Featured } from "@/components/home/Featured";
import { FeatureNav } from "@/components/home/FeatureNav";
import { Hero } from "@/components/home/Hero";
import { Intro } from "@/components/home/Intro";
import { Programs } from "@/components/home/Programs";
import { RollingBanners } from "@/components/home/RollingBanners";
import { Workshops } from "@/components/home/Workshops";

export default function Home() {
  return (
    <main className="flex-1">
      <Hero />
      <Intro />
      <FeatureNav />
      <Programs />
      <RollingBanners />
      <Workshops />
      <Featured />
    </main>
  );
}
