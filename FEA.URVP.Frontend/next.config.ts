import type { NextConfig } from "next";

const apiOrigin = (
  process.env.API_URL ||
  process.env.NEXT_PUBLIC_API_URL ||
  ""
).replace(/\/$/, "");

const nextConfig: NextConfig = {
  async rewrites() {
    if (!/^https?:\/\//i.test(apiOrigin)) {
      return [];
    }

    return [
      {
        source: "/api/:path*",
        destination: `${apiOrigin}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
