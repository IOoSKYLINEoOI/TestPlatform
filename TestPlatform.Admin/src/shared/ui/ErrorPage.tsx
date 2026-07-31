import { Link } from 'react-router-dom'

export function ErrorPage({ code, title, description }: { code: 403 | 404 | 500; title: string; description: string }) {
  return <section className="page-shell"><div className="card mx-auto mt-16 max-w-xl py-12 text-center"><p className="text-sm font-semibold text-indigo-600">Ошибка {code}</p><h1 className="mt-2 text-2xl font-semibold">{title}</h1><p className="mx-auto mt-3 max-w-md text-sm text-slate-500">{description}</p><Link className="button-primary mt-6" to="/">На главную</Link></div></section>
}
