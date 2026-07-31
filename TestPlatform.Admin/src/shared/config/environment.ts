const keycloakUrl = import.meta.env.VITE_KEYCLOAK_URL || 'http://localhost:8080'
const keycloakRealm = import.meta.env.VITE_KEYCLOAK_REALM || 'test-platform'

export const environment = {
  keycloakUrl,
  keycloakRealm,
  keycloakClientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'public-client',
  keycloakAdminUrl: import.meta.env.VITE_KEYCLOAK_ADMIN_URL || `${keycloakUrl}/admin/${keycloakRealm}/console/`,
  swaggerUrl: import.meta.env.VITE_SWAGGER_URL || 'http://localhost:5062/swagger/index.html',
  seqUrl: import.meta.env.VITE_SEQ_URL || 'http://localhost:8081',
  minioConsoleUrl: import.meta.env.VITE_MINIO_CONSOLE_URL || 'http://localhost:9101',
} as const
