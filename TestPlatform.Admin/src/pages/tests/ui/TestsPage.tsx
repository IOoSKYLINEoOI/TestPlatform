import { Archive, ChevronLeft, ChevronRight, Pencil, Plus, Search, Send } from 'lucide-react'
import { FormEvent, useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { testsApi } from '@/entities/test'
import type { Test, TestPage } from '@/entities/test'
import { isAbortError } from '@/shared/api/httpClient'
import { useLatestRequest } from '@/shared/lib'
import { ErrorToast } from '@/shared/ui'
import { Confirmation, ConfirmDialog } from '@/shared/ui'

const emptyPage: TestPage = { items: [], page: 1, pageSize: 10, totalCount: 0 }
const statuses: Record<string, string> = { Draft: 'Черновик', Published: 'Опубликован', Archived: 'В архиве', draft: 'Черновик', published: 'Опубликован', archived: 'В архиве' }

export function TestsPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<TestPage>(emptyPage)
  const [search, setSearch] = useState('')
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [confirmation, setConfirmation] = useState<Confirmation>()
  const nextRequest = useLatestRequest()

  const load = useCallback(async () => {
    setLoading(true); setError(undefined)
    const signal = nextRequest()
    try { setData(await testsApi.getTests(query, page, 10, signal)) }
    catch (cause) { if (!isAbortError(cause)) setError(message(cause, 'Не удалось загрузить тесты.')) }
    finally { if (!signal.aborted) setLoading(false) }
  }, [nextRequest, page, query])
  useEffect(() => { void load() }, [load])

  function submitSearch(event: FormEvent) { event.preventDefault(); setPage(1); setQuery(search) }
  async function changeStatus(test: Test, action: 'publish' | 'archive') {
    const verb = action === 'publish' ? 'опубликовать' : 'перенести в архив'
    setConfirmation({ title: action === 'publish' ? 'Опубликовать тест?' : 'Архивировать тест?', description: `Действительно ${verb} тест «${test.title}»?`, confirmLabel: action === 'publish' ? 'Опубликовать' : 'Архивировать', danger: action === 'archive', onConfirm: async () => { try { if (action === 'publish') await testsApi.publishTest(test.id); else await testsApi.archiveTest(test.id); await load() } catch (cause) { setError(message(cause, `Не удалось ${verb} тест.`)) } } })
  }
  const pageCount = Math.max(1, Math.ceil(data.totalCount / data.pageSize))

  return <section className="page-shell">
    <ErrorToast message={error} onClose={() => setError(undefined)} />
    <ConfirmDialog confirmation={confirmation} onClose={() => setConfirmation(undefined)} />
    <div className="flex flex-wrap items-end justify-between gap-4"><div><h1 className="page-title">Тесты</h1><p className="page-description">Управляйте тестами и их публикацией.</p></div><button className="button-primary" onClick={() => navigate('/tests/new')}><Plus size={17} /> Новый тест</button></div>
    <div className="card mt-8 p-0">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 p-4"><form className="relative w-full max-w-sm" onSubmit={submitSearch}><Search className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} /><input className="input input-with-icon" onChange={(event) => setSearch(event.target.value)} placeholder="Поиск по названию" value={search} /></form><p className="text-sm text-slate-500">Всего: {data.totalCount}</p></div>
      <div className="overflow-x-auto"><table className="w-full text-left text-sm"><thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr><th className="px-5 py-3 font-medium">Тест</th><th className="px-5 py-3 font-medium">Статус</th><th className="px-5 py-3 font-medium">Вопросов</th><th className="px-5 py-3 font-medium">Обновлён</th><th className="w-32 px-5 py-3"><span className="sr-only">Действия</span></th></tr></thead><tbody className="divide-y divide-slate-100">
        {loading ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={5}>Загрузка…</td></tr> : data.items.length === 0 ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={5}>Тесты не найдены</td></tr> : data.items.map((test) => <TestRow key={test.id} onArchive={() => void changeStatus(test, 'archive')} onOpen={() => navigate(`/tests/${test.id}`)} onPublish={() => void changeStatus(test, 'publish')} test={test} />)}
      </tbody></table></div>
      <div className="flex items-center justify-between border-t border-slate-200 px-5 py-4"><p className="text-sm text-slate-500">Страница {data.page} из {pageCount}</p><div className="flex gap-2"><button className="button-secondary px-3" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}><ChevronLeft size={17} /></button><button className="button-secondary px-3" disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)}><ChevronRight size={17} /></button></div></div>
    </div>
  </section>
}

function TestRow({ test, onOpen, onPublish, onArchive }: { test: Test; onOpen: () => void; onPublish: () => void; onArchive: () => void }) {
  const status = test.status.toLowerCase()
  return <tr className="cursor-pointer hover:bg-slate-50/70" onClick={onOpen}><td className="max-w-xl px-5 py-4"><p className="font-medium">{test.title}</p><p className="mt-1 truncate text-xs text-slate-500">{test.description}</p></td><td className="px-5 py-4"><span className={`rounded-full px-2.5 py-1 text-xs font-medium ${status === 'published' ? 'bg-emerald-50 text-emerald-700' : status === 'archived' ? 'bg-slate-100 text-slate-600' : 'bg-amber-50 text-amber-700'}`}>{statuses[test.status] ?? test.status}</span></td><td className="px-5 py-4 text-slate-600">{test.totalQuestions}</td><td className="px-5 py-4 text-slate-600">{new Intl.DateTimeFormat('ru-RU').format(new Date(test.updatedAt))}</td><td className="px-5 py-4"><div className="flex justify-end gap-1">{status === 'draft' && <><button className="icon-button" onClick={(event) => { event.stopPropagation(); onOpen() }} title="Редактировать"><Pencil size={16} /></button><button className="icon-button text-emerald-700 hover:bg-emerald-50" onClick={(event) => { event.stopPropagation(); onPublish() }} title="Опубликовать"><Send size={16} /></button></>}{status === 'published' && <button className="icon-button" onClick={(event) => { event.stopPropagation(); onArchive() }} title="В архив"><Archive size={16} /></button>}</div></td></tr>
}

function message(cause: unknown, fallback: string) { return cause instanceof Error ? cause.message : fallback }

