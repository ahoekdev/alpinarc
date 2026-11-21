# Web image. Build context: the repo root, so Orval can read api/openapi/.
#   docker build -f web/Dockerfile --build-arg NEXT_PUBLIC_API_URL=http://localhost:5033 -t alpinarc-web .
#
# NEXT_PUBLIC_API_URL is fixed into the client bundle at build time, so changing it
# needs a new build, not a restart.

FROM node:26-slim AS deps
WORKDIR /repo/web
COPY web/package.json web/package-lock.json ./
RUN npm ci

FROM node:26-slim AS build
WORKDIR /repo
COPY api/openapi/ api/openapi/
COPY web/ web/
COPY --from=deps /repo/web/node_modules web/node_modules
WORKDIR /repo/web
# Railway only passes a service variable to the build when it's declared as an ARG.
ARG NEXT_PUBLIC_API_URL
ENV NEXT_TELEMETRY_DISABLED=1
# public/ is empty and so not in git; create it so the runtime stage can copy it.
RUN mkdir -p public \
    && npm run generate:api \
    && npm run build

FROM node:26-slim AS runtime
WORKDIR /app
ENV NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    HOSTNAME=0.0.0.0 \
    PORT=3000
COPY --from=build --chown=node:node /repo/web/.next/standalone ./
COPY --from=build --chown=node:node /repo/web/.next/static ./.next/static
COPY --from=build --chown=node:node /repo/web/public ./public
# The non-root user that the Node images include.
USER node
EXPOSE 3000
CMD ["node", "server.js"]
