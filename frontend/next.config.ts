import type { NextConfig } from "next";
import os from "os";

// Dynamically fetch the active local network IP address
const getLocalIp = (): string => {
  const interfaces = os.networkInterfaces();
  for (const interfaceName in interfaces) {
    const addresses = interfaces[interfaceName];
    if (addresses) {
      for (const addr of addresses) {
        if (addr.family === "IPv4" && !addr.internal) {
          return addr.address;
        }
      }
    }
  }
  return "127.0.0.1";
};

const localIp = getLocalIp();

const nextConfig: NextConfig = {
  output: "standalone",
  
  // FIX: This must reside at the root level of NextConfig
  allowedDevOrigins: [localIp, "localhost", "127.0.0.1", "0.0.0.0"],

  async redirects() {
    return [
      {
        source: "/services",
        destination: "/service-inventory",
        permanent: true,
      },
      {
        source: "/services/:id",
        destination: "/service-inventory/:id",
        permanent: true,
      },
    ]
  },
};

export default nextConfig;
