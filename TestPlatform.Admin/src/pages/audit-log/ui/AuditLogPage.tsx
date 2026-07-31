import { ChevronLeft, ChevronRight, Search } from 'lucide-react'
import { FormEvent, useCallback, useEffect, useState } from 'react'
import { auditLogApi } from '@/entities/audit-log'
import type { AuditLogPage as AuditLogData } from '@/entities/audit-log'
import { isAbortError } from '@/shared/api/httpClient'
import { useLatestRequest } from '@/shared/lib'
import { ErrorToast } from '@/shared/ui'

const empty: AuditLogData = { items: [], page: 1, pageSize: 20, totalCount: 0 }
export function AuditLogPage() {
  const [data, setData] = useState(empty)
  const [employee, setEmployee] = useState('')
  const [query, setQuery] = useState('')
  const [method, setMethod] = useState('')
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const nextRequest = useLatestRequest()
  const load = useCallback(async () => { const signal = nextRequest(); setLoading(true); setError(undefined); try { setData(await auditLogApi.getPage(page, query, method, 20, signal)) } catch (cause) { if (!isAbortError(cause)) setError(cause instanceof Error ? cause.message : 'Не удалось загрузить журнал аудита.') } finally { if (!signal.aborted) setLoading(false) } }, [method, nextRequest, page, query])
  useEffect(() => { void load() }, [load])
  function submit(event: FormEvent) { event.preventDefault(); setPage(1); setQuery(employee) }
  const pages = Math.max(1, Math.ceil(data.totalCount / data.pageSize))
  return <section className="page-shell"><ErrorToast message={error} onClose={() => setError(undefined)} /><div><h1 className="page-title">Журнал аудита</h1><p className="page-description">История административных изменений платформы.</p></div><div className="card mt-8 p-0">
    <div className="flex flex-wrap gap-3 border-b border-slate-200 p-4 dark:border-slate-800"><form className="relative min-w-64 flex-1" onSubmit={submit}><Search className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} /><input className="input input-with-icon" onChange={(event) => setEmployee(event.target.value)} placeholder="Табельный номер" value={employee} /></form><select className="input w-auto" onChange={(event) => { setPage(1); setMethod(event.target.value) }} value={method}><option value="">Все операции</option><option value="POST">Создание / действие</option><option value="PUT">Обновление</option><option value="PATCH">Частичное обновление</option><option value="DELETE">Удаление</option></select></div>
    <div className="overflow-x-auto"><table className="w-full text-left text-sm"><thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500 dark:bg-slate-900"><tr><th className="px-5 py-3">Время</th><th className="px-5 py-3">Пользователь</th><th className="px-5 py-3">Метод</th><th className="px-5 py-3">Маршрут</th><th className="px-5 py-3">Статус</th><th className="px-5 py-3">Trace ID</th></tr></thead><tbody className="divide-y divide-slate-100 dark:divide-slate-800">{loading ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={6}>Загрузка…</td></tr> : data.items.length === 0 ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={6}>Записей пока нет</td></tr> : data.items.map((item) => <tr key={item.id}><td className="whitespace-nowrap px-5 py-4">{new Intl.DateTimeFormat('ru-RU', { dateStyle: 'short', timeStyle: 'medium' }).format(new Date(item.createdAt))}</td><td className="px-5 py-4"><p className="font-medium">{item.employeeNumber || 'Система'}</p>{item.userId && <p className="text-xs text-slate-500">{item.userId}</p>}</td><td className="px-5 py-4 font-mono text-xs">{item.method}</td><td className="max-w-md truncate px-5 py-4 font-mono text-xs" title={item.path}>{item.path}</td><td className="px-5 py-4"><span className={`rounded-full px-2 py-1 text-xs font-medium ${item.statusCode < 400 ? 'bg-emerald-50 text-emerald-700' : 'bg-rose-50 text-rose-700'}`}>{item.statusCode}</span></td><td className="max-w-48 truncate px-5 py-4 font-mono text-xs text-slate-500" title={item.traceId}>{item.traceId}</td></tr>)}</tbody></table></div>
    <div className="flex items-center justify-between border-t border-slate-200 px-5 py-4 dark:border-slate-800"><p className="text-sm text-slate-500">Страница {data.page} из {pages} · всего {data.totalCount}</p><div className="flex gap-2"><button className="button-secondary px-3" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}><ChevronLeft size={17} /></button><button className="button-secondary px-3" disabled={page >= pages} onClick={() => setPage((value) => value + 1)}><ChevronRight size={17} /></button></div></div>
  </div></section>
}
