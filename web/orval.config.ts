import { defineConfig } from "orval";

const input = { target: "../api/openapi/alpinarc-api.json" };

export default defineConfig({
  api: {
    input,
    output: {
      mode: "tags-split",
      target: "lib/api/generated",
      client: "react-query",
      httpClient: "fetch",
      clean: true,
      formatter: "prettier",
      baseUrl: { runtime: "process.env.NEXT_PUBLIC_API_URL" },
      override: {
        fetch: {
          includeHttpResponseReturnType: false,
          forceSuccessResponse: true,
        },
      },
    },
  },
  apiZod: {
    input,
    output: {
      mode: "tags-split",
      target: "lib/api/generated",
      client: "zod",
      fileExtension: ".zod.ts",
      formatter: "prettier",
    },
  },
});
