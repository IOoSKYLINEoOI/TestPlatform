import { useEffect, useState } from 'react'
import { keycloak } from '@/shared/auth/keycloak'

type AuthenticationState = { ready: boolean; authenticated: boolean; error?: string }

export function useAuthentication(): AuthenticationState {
  const [state, setState] = useState<AuthenticationState>({ ready: false, authenticated: false })
  useEffect(() => {
    keycloak.init({ onLoad: 'login-required', pkceMethod: 'S256', checkLoginIframe: false })
      .then((authenticated) => setState({ ready: true, authenticated }))
      .catch(() => setState({ ready: true, authenticated: false, error: 'Не удалось подключиться к сервису авторизации.' }))
  }, [])
  return state
}
