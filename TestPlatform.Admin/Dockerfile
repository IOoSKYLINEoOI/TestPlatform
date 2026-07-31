FROM node:22-alpine AS build

WORKDIR /app

RUN corepack enable

COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
ARG VITE_KEYCLOAK_URL=http://localhost:8080
ARG VITE_KEYCLOAK_REALM=test-platform
ARG VITE_KEYCLOAK_CLIENT_ID=public-client
ARG VITE_KEYCLOAK_ADMIN_URL
ARG VITE_SWAGGER_URL=http://localhost:5062/swagger/index.html
ARG VITE_SEQ_URL=http://localhost:8081
ARG VITE_MINIO_CONSOLE_URL=http://localhost:9101
ENV VITE_KEYCLOAK_URL=$VITE_KEYCLOAK_URL \
    VITE_KEYCLOAK_REALM=$VITE_KEYCLOAK_REALM \
    VITE_KEYCLOAK_CLIENT_ID=$VITE_KEYCLOAK_CLIENT_ID \
    VITE_KEYCLOAK_ADMIN_URL=$VITE_KEYCLOAK_ADMIN_URL \
    VITE_SWAGGER_URL=$VITE_SWAGGER_URL \
    VITE_SEQ_URL=$VITE_SEQ_URL \
    VITE_MINIO_CONSOLE_URL=$VITE_MINIO_CONSOLE_URL
RUN pnpm build

FROM nginx:1.27-alpine

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
