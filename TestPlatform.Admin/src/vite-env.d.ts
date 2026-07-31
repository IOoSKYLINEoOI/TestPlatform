/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_KEYCLOAK_URL?: string
  readonly VITE_KEYCLOAK_REALM?: string
  readonly VITE_KEYCLOAK_CLIENT_ID?: string
  readonly VITE_KEYCLOAK_ADMIN_URL?: string
  readonly VITE_SWAGGER_URL?: string
  readonly VITE_SEQ_URL?: string
  readonly VITE_MINIO_CONSOLE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
