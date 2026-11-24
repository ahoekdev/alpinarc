import { defineConfig } from "orval";

export default defineConfig({
  api: {
    input: {
      target: "../api/openapi/alpinarc-api.json",
    },
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
});
