import { defineConfig } from "orval";
import { apiUrl } from "./lib/env";

// orval doesn't load env files itself
process.loadEnvFile(".env.development.local");

export default defineConfig({
  api: {
    // Served by the running API (apps/api: npm run start:dev)
    input: `${apiUrl()}/api/docs-json`,
    output: {
      target: "lib/api/generated.ts",
      client: "fetch",
      baseUrl: {
        runtime: "apiUrl()",
        imports: [{ name: "apiUrl", importPath: "../env" }],
      },
    },
  },
});
