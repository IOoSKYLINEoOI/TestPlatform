import { keycloak } from '@/shared/auth/keycloak'

export function LoadingScreen({ label }: { label: string }) {
  return <div className="grid min-h-screen place-items-center bg-slate-50"><div className="text-center"><div className="mx-auto size-8 animate-spin rounded-full border-2 border-indigo-600 border-t-transparent" /><p className="mt-4 text-sm text-slate-600">{label}</p></div></div>
}

export function AccessDeniedScreen() {
  return <div className="grid min-h-screen place-items-center bg-slate-50 p-6"><div className="max-w-md rounded-2xl border border-slate-200 bg-white p-8 text-center shadow-sm"><div className="mx-auto grid size-12 place-items-center rounded-full bg-rose-50 text-xl text-rose-600">!</div><h1 className="mt-5 text-xl font-semibold">Недостаточно прав</h1><p className="mt-2 text-sm leading-6 text-slate-600">Для административной панели требуется роль Admin.</p><button className="button-primary mt-6" onClick={() => keycloak.logout({ redirectUri: window.location.origin })}>Выйти</button></div></div>
}
