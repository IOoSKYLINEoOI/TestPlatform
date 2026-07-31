import { isAdmin } from '@/shared/auth/keycloak'
import { AccessDeniedScreen, LoadingScreen } from '@/features/auth'
import { useAuthentication } from '@/features/auth'
import { AppRoutes } from '@/app/routes/AppRoutes'
import { AppErrorBoundary } from '@/shared/ui'

export function App() {
  const { ready, authenticated, error } = useAuthentication()
  if (!ready) return <LoadingScreen label="Проверяем доступ…" />
  if (error) return <LoadingScreen label={error} />
  if (!authenticated) return <LoadingScreen label="Перенаправляем на страницу входа…" />
  if (!isAdmin()) return <AccessDeniedScreen />
  return <AppErrorBoundary><AppRoutes /></AppErrorBoundary>
}
