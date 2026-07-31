import { Archive, ArrowRight as Pencil, ChevronLeft, ChevronRight, Copy, Plus } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { questionsApi } from '@/entities/question'
import type { Question, QuestionPage } from '@/entities/question'
import { isAbortError } from '@/shared/api/httpClient'
import { useLatestRequest } from '@/shared/lib'
import { ErrorToast } from '@/shared/ui'
import { Confirmation, ConfirmDialog } from '@/shared/ui'

const emptyPage: QuestionPage = { items: [], page: 1, pageSize: 10, totalCount: 0 }
const typeLabels: Record<string, string> = { choice: 'Выбор ответа', text: 'Текст', number: 'Число', matching: 'Сопоставление' }
const statusLabels: Record<string, string> = { Draft: 'Черновик', Published: 'Опубликован', Archived: 'В архиве', draft: 'Черновик', published: 'Опубликован', archived: 'В архиве' }

export function QuestionsPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<QuestionPage>(emptyPage)
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [confirmation, setConfirmation] = useState<Confirmation>()
  const nextRequest = useLatestRequest()

  const load = useCallback(async () => {
    setLoading(true); setError(undefined)
    const signal = nextRequest()
    try { setData(await questionsApi.getQuestions(status, page, 10, signal)) }
    catch (cause) { if (!isAbortError(cause)) setError(toMessage(cause, 'Не удалось загрузить вопросы.')) }
    finally { if (!signal.aborted) setLoading(false) }
  }, [nextRequest, page, status])

  useEffect(() => { void load() }, [load])
  const pageCount = Math.max(1, Math.ceil(data.totalCount / data.pageSize))
  async function clone(question: Question) {
    try {
      const id = await questionsApi.cloneQuestion(question.id)
      navigate(`/questions/${id}`)
    } catch (cause) { setError(toMessage(cause, 'Не удалось клонировать вопрос.')) }
  }
  function archive(question: Question) {
    setConfirmation({ title: 'Архивировать вопрос?', description: 'Вопрос будет исключён из дальнейшего использования в новых тестах и экзаменах.', confirmLabel: 'Архивировать', danger: true, onConfirm: async () => { try { await questionsApi.archiveQuestion(question.id); await load() } catch (cause) { setError(toMessage(cause, 'Не удалось архивировать вопрос.')) } } })
  }

  return <section className="page-shell">
    <ErrorToast message={error} onClose={() => setError(undefined)} />
    <ConfirmDialog confirmation={confirmation} onClose={() => setConfirmation(undefined)} />
    <div className="flex flex-wrap items-end justify-between gap-4"><div><h1 className="page-title">Вопросы</h1><p className="page-description">Банк вопросов для тестов и экзаменов.</p></div><button className="button-primary" onClick={() => navigate('/questions/new')}><Plus size={17} /> Новый вопрос</button></div>
    <div className="card mt-8 p-0">
      <div className="flex items-center justify-between gap-3 border-b border-slate-200 p-4"><select className="input max-w-52" onChange={(event) => { setPage(1); setStatus(event.target.value) }} value={status}><option value="">Все статусы</option><option value="draft">Черновики</option><option value="published">Опубликованные</option><option value="archived">Архивные</option></select><p className="text-sm text-slate-500">Всего: {data.totalCount}</p></div>
      <div className="overflow-x-auto"><table className="w-full text-left text-sm"><thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr><th className="px-5 py-3 font-medium">Вопрос</th><th className="px-5 py-3 font-medium">Тип</th><th className="px-5 py-3 font-medium">Теги</th><th className="px-5 py-3 font-medium">Статус</th><th className="w-16 px-5 py-3"><span className="sr-only">Действия</span></th></tr></thead><tbody className="divide-y divide-slate-100">
        {loading ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={5}>Загрузка…</td></tr> : data.items.length === 0 ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={5}>Вопросы не найдены</td></tr> : data.items.map((question) => <QuestionRow key={question.id} onArchive={() => archive(question)} onClone={() => void clone(question)} onEdit={() => navigate(`/questions/${question.id}`)} question={question} />)}
      </tbody></table></div>
      <div className="flex items-center justify-between border-t border-slate-200 px-5 py-4"><p className="text-sm text-slate-500">Страница {data.page} из {pageCount}</p><div className="flex gap-2"><button className="button-secondary px-3" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}><ChevronLeft size={17} /></button><button className="button-secondary px-3" disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)}><ChevronRight size={17} /></button></div></div>
    </div>
  </section>
}

function QuestionRow({ question, onEdit, onClone, onArchive }: { question: Question; onEdit: () => void; onClone: () => void; onArchive: () => void }) {
  const status = question.status.toLowerCase()
  return <tr className="cursor-pointer hover:bg-slate-50/70" onClick={onEdit}><td className="max-w-2xl px-5 py-4 font-medium">{question.text}</td><td className="px-5 py-4 text-slate-600">{typeLabels[question.kind] ?? question.type}</td><td className="px-5 py-4"><div className="flex flex-wrap gap-1">{question.tags.length ? question.tags.map((tag) => <span className="rounded bg-indigo-50 px-2 py-1 text-xs text-indigo-700" key={tag.id}>{tag.name}</span>) : <span className="text-slate-400">а</span>}</div></td><td className="px-5 py-4"><span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-700">{statusLabels[question.status] ?? question.status}</span></td><td className="px-5 py-4"><div className="flex justify-end gap-1"><button className="icon-button" onClick={(event) => { event.stopPropagation(); onEdit() }} title="Открыть вопрос"><Pencil size={16} /></button>{status === 'published' && <><button className="icon-button" onClick={(event) => { event.stopPropagation(); onClone() }} title="Создать копию"><Copy size={16} /></button><button className="icon-button text-rose-600 hover:bg-rose-50" onClick={(event) => { event.stopPropagation(); onArchive() }} title="Архивировать"><Archive size={16} /></button></>}</div></td></tr>
}


function toMessage(cause: unknown, fallback: string) { return cause instanceof Error ? cause.message : fallback }

