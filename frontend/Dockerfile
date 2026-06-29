# syntax=docker/dockerfile:1.7

############################
# Base
############################
FROM oven/bun:1-alpine AS base

WORKDIR /app

ENV NEXT_TELEMETRY_DISABLED=1

############################
# Dependencies
############################
FROM base AS deps

# Copy your package.json and the new bun.lock file
COPY package.json bun.lock* ./

# Use Bun's cache mount instead of npm's
RUN --mount=type=cache,target=/root/.bun/install/cache \
    bun install --frozen-lockfile

############################
# Builder
############################
FROM base AS builder

ARG BUILD_VERSION=1.0.0
ARG NEXT_PUBLIC_API_URL
ARG NEXT_PUBLIC_KEYCLOAK_URL
ARG NEXT_PUBLIC_KEYCLOAK_REALM
ARG NEXT_PUBLIC_KEYCLOAK_CLIENT_ID

COPY --from=deps /app/node_modules ./node_modules
COPY . .

ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL:-http://localhost:5020}
ENV NEXT_PUBLIC_KEYCLOAK_URL=${NEXT_PUBLIC_KEYCLOAK_URL:-http://localhost:8080}
ENV NEXT_PUBLIC_KEYCLOAK_REALM=${NEXT_PUBLIC_KEYCLOAK_REALM:-obss}
ENV NEXT_PUBLIC_KEYCLOAK_CLIENT_ID=${NEXT_PUBLIC_KEYCLOAK_CLIENT_ID:-obss-frontend}

# Next.js standalone build requires the engine to know it is building a production target
ENV NODE_ENV=production

RUN bun run build

############################
# Runtime (Stays on slim Node.js for Next.js standalone output compatibility)
############################
FROM node:22-alpine AS runner

WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV HOSTNAME=0.0.0.0
ENV PORT=3000

LABEL \
    org.opencontainers.image.title="OBSS Frontend" \
    org.opencontainers.image.description="Tier-1 Telecom OSS/BSS Platform Frontend" \
    org.opencontainers.image.version="${BUILD_VERSION}" \
    org.opencontainers.image.vendor="OBSS"

COPY --from=builder --chown=node:node /app/.next/standalone ./
COPY --from=builder --chown=node:node /app/.next/static ./.next/static
COPY --from=builder --chown=node:node /app/public ./public

USER node

EXPOSE 3000

CMD ["node", "server.js"]
