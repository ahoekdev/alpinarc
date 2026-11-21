import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Bundles a minimal server (server.js) into .next/standalone for the Docker image.
  output: "standalone",
  cacheComponents: true,
  partialPrefetching: true,
};

export default nextConfig;
