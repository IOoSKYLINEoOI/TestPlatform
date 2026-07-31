import { Component, type ErrorInfo, type ReactNode } from 'react'

type State = { error?: Error }

export class AppErrorBoundary extends Component<{ children: ReactNode }, State> {
  state: State = {}

  static getDerivedStateFromError(error: Error): State { return { error } }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Unhandled application error', error, info)
  }

  render() {
    if (!this.state.error) return this.props.children
    return <main className="grid min-h-screen place-items-center bg-slate-50 p-6 dark:bg-slate-950"><div className="card w-full max-w-lg text-center"><p className="text-sm font-semibold text-rose-600">Ошибка 500</p><h1 className="mt-2 text-2xl font-semibold">Не удалось отобразить страницу</h1><p className="mt-3 text-sm text-slate-500">Произошла непредвиденная ошибка интерфейса. Перезагрузите страницу или вернитесь на главную.</p><div className="mt-6 flex justify-center gap-3"><button className="button-secondary" onClick={() => window.location.reload()}>Перезагрузить</button><a className="button-primary" href="/">На главную</a></div></div></main>
  }
}
