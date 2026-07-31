import { ChevronLeft, ChevronRight, ClipboardCheck, ClipboardList, History, Search } from 'lucide-react'
import { FormEvent, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { attemptsApi } from '@/entities/attempt'
import type { AttemptSource, AttemptSourceType } from '@/entities/attempt'
import { isAbortError } from '@/shared/api/httpClient'
import { ErrorToast } from '@/shared/ui'

export function AttemptsOverviewPage() {
  const [sources, setSources] = useState<AttemptSource[]>([])
  const [search, setSearch] = useState('')
  const [query, setQuery] = useState('')
  const [type, setType] = useState<'all' | AttemptSourceType>('all')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const pageSize = 12

  useEffect(() => {
    const controller = new AbortController()
    setLoading(true)
    setError(undefined)
    attemptsApi.getSources({ search: query, type: type === 'all' ? undefined : type, page, pageSize }, controller.signal)
      .then((result) => { setTotal(result.totalCount); setSources(result.items) })
      .catch((cause) => { if (!isAbortError(cause)) setError(cause instanceof Error ? cause.message : 'Не удалось загрузить тесты и экзамены.') })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [page, query, type])

  function submit(event: FormEvent) { event.preventDefault(); setPage(1); setQuery(search) }
  const pageCount = Math.max(1, Math.ceil(total / pageSize))

  return <section className="page-shell">
    <ErrorToast message={error} onClose={() => setError(undefined)} />
    <div><h1 className="page-title">Попытки</h1><p className="page-description">Выберите тест или экзамен, чтобы открыть историю прохождений.</p></div>
    <div className="mt-8 flex flex-wrap gap-3">
      <form className="relative min-w-64 flex-1" onSubmit={submit}><Search className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} /><input className="input input-with-icon" onChange={(event) => setSearch(event.target.value)} placeholder="Поиск по названию" value={search} /></form>
      <div className="flex rounded-lg border border-slate-200 bg-white p-1 dark:border-slate-800 dark:bg-slate-900">{([['all', 'Все'], ['test', 'Тесты'], ['exam', 'Экзамены']] as const).map(([value, label]) => <button className={`rounded-md px-4 py-2 text-sm font-medium ${type === value ? 'bg-indigo-600 text-white' : 'text-slate-600 hover:bg-slate-50 dark:text-slate-300 dark:hover:bg-slate-800'}`} key={value} onClick={() => { setType(value); setPage(1) }} type="button">{label}</button>)}</div>
    </div>
    {loading ? <div className="card mt-6 py-12 text-center text-sm text-slate-500">Загрузка…</div> : sources.length === 0 ? <div className="card mt-6 py-12 text-center text-sm text-slate-500">Тесты и экзамены не найдены</div> : <>
      <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{sources.map((source) => <SourceCard key={`${source.type}-${source.id}`} source={source} />)}</div>
      <div className="mt-6 flex items-center justify-between"><p className="text-sm text-slate-500">Страница {page} из {pageCount} · всего {total}</p><div className="flex gap-2"><button className="button-secondary px-3" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}><ChevronLeft size={17} /></button><button className="button-secondary px-3" disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)}><ChevronRight size={17} /></button></div></div>
    </>}
  </section>
}

function SourceCard({ source }: { source: AttemptSource }) {
  const Icon = source.type === 'test' ? ClipboardCheck : ClipboardList
  return <Link className="card group transition hover:-translate-y-0.5 hover:border-indigo-200 hover:shadow-md" to={`/${source.type === 'test' ? 'tests' : 'exams'}/${source.id}/attempts`}><div className="flex items-start justify-between gap-4"><span className="grid size-10 place-items-center rounded-lg bg-indigo-50 text-indigo-600 dark:bg-indigo-950"><Icon size={20} /></span><History className="text-slate-300 transition group-hover:text-indigo-500" size={19} /></div><h2 className="mt-4 font-semibold">{source.title}</h2><p className="mt-1 line-clamp-2 text-sm text-slate-500">{source.description || 'Без описания'}</p><p className="mt-4 text-xs font-medium uppercase tracking-wide text-slate-400">{source.type === 'test' ? 'Тест' : 'Экзамен'} · {source.status}</p></Link>
}
