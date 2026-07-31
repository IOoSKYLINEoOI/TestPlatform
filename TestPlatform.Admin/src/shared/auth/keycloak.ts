import Keycloak from 'keycloak-js'
import { environment } from '@/shared/config/environment'

export const keycloak = new Keycloak({
  url: environment.keycloakUrl,
  realm: environment.keycloakRealm,
  clientId: environment.keycloakClientId,
})

export function isAdmin() {
  const roles = keycloak.tokenParsed?.roles

  return keycloak.hasRealmRole('Admin') || (Array.isArray(roles) && roles.includes('Admin'))
}

