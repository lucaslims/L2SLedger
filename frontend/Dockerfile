# ── Stage 1: Dependencies ───────────────────────────────────────
FROM node:20-alpine AS deps
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --ignore-scripts

# ── Stage 2: Build ──────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

COPY --from=deps /app/node_modules ./node_modules
COPY . .

# Build-time env vars: placeholders (runtime injection via env.sh)
ARG VITE_API_BASE_URL=""
ARG VITE_FIREBASE_API_KEY=""
ARG VITE_FIREBASE_AUTH_DOMAIN=""
ARG VITE_FIREBASE_PROJECT_ID=""

RUN npm run build

# ── Stage 3: Production runtime (serve) ────────────────────────
FROM node:20-alpine AS runtime

# OCI image metadata
LABEL org.opencontainers.image.title="L2SLedger Frontend" \
      org.opencontainers.image.description="L2SLedger Frontend SPA (React + Vite)" \
      org.opencontainers.image.source="https://github.com/l2s-ledger/cash-flow" \
      org.opencontainers.image.vendor="L2SLedger"

# Install serve — lightweight static file server with SPA support (~2MB)
# No devDependencies, no build tools, minimal attack surface
RUN npm install -g serve@14 && \
    npm cache clean --force

# Security: create non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app

# Copy only the built static assets — no node_modules, no source code
COPY --from=build --chown=appuser:appgroup /app/dist ./dist

# Copy env.sh entrypoint script for runtime VITE_* injection
COPY --chown=appuser:appgroup docker/env.sh ./env.sh
RUN chmod +x ./env.sh

# serve configuration: SPA mode, custom headers, cache control
COPY --chown=appuser:appgroup docker/serve.json ./dist/serve.json

# Switch to non-root user
USER appuser

# serve listens on port 3000 by default
EXPOSE 3000

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:3000/ || exit 1

# env.sh generates env-config.js then exec's the CMD
ENTRYPOINT ["./env.sh"]
CMD ["serve", "./dist", "-l", "3000", "-s"]
