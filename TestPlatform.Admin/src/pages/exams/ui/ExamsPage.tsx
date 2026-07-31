import { Archive, ArrowRight, ChevronLeft, ChevronRight, Plus, Search, Send } from 'lucide-react'
import { FormEvent, useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { examsApi } from '@/entities/exam'
import type { Exam, ExamPage } from '@/entities/exam'
import { isAbortError } from '@/shared/api/httpClient'
import { useLatestRequest } from '@/shared/lib'
import { ErrorToast } from '@/shared/ui'
import { Confirmation, ConfirmDialog } from '@/shared/ui'

const emptyPage: ExamPage = { items: [], page: 1, pageSize: 10, totalCount: 0 }
const statusLabels: Record<string, string> = { Draft: 'Черновик', Published: 'Опубликован', Archived: 'В архиве' }

export function ExamsPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<ExamPage>(emptyPage)
  const [search, setSearch] = useState('')
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [confirmation, setConfirmation] = useState<Confirmation>()
  const nextRequest = useLatestRequest()

  const load = useCallback(async () => {
    setLoading(true)
    setError(undefined)
    const signal = nextRequest()
    try { setData(await examsApi.getExams(query, page, 10, signal)) }
    catch (cause) { if (!isAbortError(cause)) setError(message(cause, 'Не удалось загрузить экзамены.')) }
    finally { if (!signal.aborted) setLoading(false) }
  }, [nextRequest, page, query])

  useEffect(() => { void load() }, [load])

  function submitSearch(event: FormEvent) {
    event.preventDefault()
    setPage(1)
    setQuery(search)
  }

  async function changeStatus(exam: Exam, action: 'publish' | 'archive') {
    const verb = action === 'publish' ? 'опубликовать' : 'перенести в архив'
    setConfirmation({ title: action === 'publish' ? 'Опубликовать экзамен?' : 'Архивировать экзамен?', description: `Действительно ${verb} экзамен «${exam.title}»?`, confirmLabel: action === 'publish' ? 'Опубликовать' : 'Архивировать', danger: action === 'archive', onConfirm: async () => { try { setError(undefined); if (action === 'publish') await examsApi.publishExam(exam.id); else await examsApi.archiveExam(exam.id); await load() } catch (cause) { setError(message(cause, `Не удалось ${verb} экзамен.`)) } } })
  }

  const pageCount = Math.max(1, Math.ceil(data.totalCount / data.pageSize))

  return (
    <section className="page-shell">
      <ErrorToast message={error} onClose={() => setError(undefined)} />
      <ConfirmDialog confirmation={confirmation} onClose={() => setConfirmation(undefined)} />
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div><h1 className="page-title">Экзамены</h1><p className="page-description">Создавайте, редактируйте и публикуйте экзамены.</p></div>
        <button className="button-primary" onClick={() => navigate('/exams/new')}><Plus size={17} /> Новый экзамен</button>
      </div>

      <div className="card mt-8 p-0">
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 p-4">
          <form className="relative w-full max-w-sm" onSubmit={submitSearch}>
            <Search className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
            <input className="input input-with-icon" onChange={(event) => setSearch(event.target.value)} placeholder="Поиск по названию" value={search} />
          </form>
          <p className="text-sm text-slate-500">Всего: {data.totalCount}</p>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr><th className="px-5 py-3 font-medium">Экзамен</th><th className="px-5 py-3 font-medium">Статус</th><th className="px-5 py-3 font-medium">Вопросы и баллы</th><th className="px-5 py-3 font-medium">Создан</th><th className="w-36 px-5 py-3"><span className="sr-only">Действия</span></th></tr></thead>
            <tbody className="divide-y divide-slate-100">
              {loading ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={5}>Загрузка…</td></tr>
                : data.items.length === 0 ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={5}>Экзамены не найдены</td></tr>
                  : data.items.map((exam) => { const status = exam.status.toLowerCase(); return <tr className="cursor-pointer hover:bg-slate-50/70" key={exam.id} onClick={() => navigate(`/exams/${exam.id}`)}>
                    <td className="max-w-lg px-5 py-4"><p className="font-medium">{exam.title}</p><p className="mt-1 truncate text-xs text-slate-500">{exam.description}</p></td>
                    <td className="px-5 py-4"><span className={`rounded-full px-2.5 py-1 text-xs font-medium ${exam.status === 'Published' ? 'bg-emerald-50 text-emerald-700' : exam.status === 'Archived' ? 'bg-slate-100 text-slate-600' : 'bg-amber-50 text-amber-700'}`}>{statusLabels[exam.status] ?? exam.status}</span></td>
                    <td className="px-5 py-4 text-slate-600">{exam.totalQuestions} · {exam.totalMaxScore} баллов</td>
                    <td className="px-5 py-4 text-slate-600">{new Intl.DateTimeFormat('ru-RU').format(new Date(exam.createdAt))}</td>
                    <td className="px-5 py-4"><div className="flex justify-end gap-1">
                      <button className="icon-button" onClick={(event) => { event.stopPropagation(); navigate(`/exams/${exam.id}`) }} title="Открыть экзамен"><ArrowRight size={16} /></button>
                      {status === 'draft' && <button className="icon-button text-emerald-700 hover:bg-emerald-50" onClick={(event) => { event.stopPropagation(); void changeStatus(exam, 'publish') }} title="Опубликовать"><Send size={16} /></button>}
                      {status === 'published' && <button className="icon-button" onClick={(event) => { event.stopPropagation(); void changeStatus(exam, 'archive') }} title="В архив"><Archive size={16} /></button>}
                    </div></td>
                  </tr>})}
            </tbody>
          </table>
        </div>
        <div className="flex items-center justify-between border-t border-slate-200 px-5 py-4"><p className="text-sm text-slate-500">Страница {data.page} из {pageCount}</p><div className="flex gap-2"><button className="button-secondary px-3" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}><ChevronLeft size={17} /></button><button className="button-secondary px-3" disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)}><ChevronRight size={17} /></button></div></div>
      </div>
    </section>
  )
}

function message(cause: unknown, fallback: string) { return cause instanceof Error ? cause.message : fallback }


