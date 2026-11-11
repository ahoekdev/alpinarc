import { defineConfig } from "orval";

export default defineConfig({
  api: {
    input: {
      target: "../api/openapi/alpinarc-api.json",
    },
    output: {
      mode: "tags-split",
      target: "lib/api/generated",
      client: "fetch",
      clean: true,
      formatter: "prettier",
      override: {
        mutator: {
          path: "lib/api/fetcher.ts",
          name: "apiFetch",
        },
      },
    },
  },
});
