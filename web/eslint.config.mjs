import { defineConfig, globalIgnores } from "eslint/config";
import nextVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";
import prettier from "eslint-config-prettier/flat";

const eslintConfig = defineConfig([
  ...nextVitals,
  ...nextTs,
  prettier,
  // Base UI is only used inside the generic components in components/ui/;
  // the rest of the app uses those components.
  {
    files: ["**/*.{js,jsx,mjs,ts,tsx}"],
    ignores: ["components/ui/**"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            {
              group: ["@base-ui/react", "@base-ui/react/*"],
              message: "Import the wrappers from @/components/ui instead.",
            },
          ],
        },
      ],
    },
  },
  // Override default ignores of eslint-config-next.
  globalIgnores([
    // Default ignores of eslint-config-next:
    ".next/**",
    "out/**",
    "build/**",
    "next-env.d.ts",
    // Orval output
    "lib/api/generated/**",
  ]),
]);

export default eslintConfig;
