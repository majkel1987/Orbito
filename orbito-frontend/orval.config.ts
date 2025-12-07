import { defineConfig } from "orval";

export default defineConfig({
  orbito: {
    input: "http://localhost:5211/swagger/v1/swagger.json",
    output: {
      mode: "tags-split",
      target: "src/core/api/generated/endpoints.ts",
      schemas: "src/core/api/generated/models",
      client: "react-query",
      override: {
        mutator: {
          path: "src/core/api/client.ts",
          name: "customInstance",
        },
      },
    },
  },
});
