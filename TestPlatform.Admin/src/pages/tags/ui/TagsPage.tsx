import { ChevronLeft, ChevronRight, Eye, Merge, Pencil, Plus, Search, Trash2, X } from 'lucide-react'
import { FormEvent, useCallback, useEffect, useState } from 'react'
import { tagsApi } from '@/entities/tag'
import type { Tag, TagPage, TagQuestionsPage, TagUsage } from '@/entities/tag'
import { TagDialog } from '@/features/tag-editor'
import { isAbortError } from '@/shared/api/httpClient'
import { useLatestRequest } from '@/shared/lib'
import { ConfirmDialog, ErrorToast } from '@/shared/ui'
import type { Confirmation } from '@/shared/ui'

const emptyPage: TagPage = { items: [], page: 1, pageSize: 10, totalCount: 0 }
const errorMessage = (cause: unknown, fallback: string) => cause instanceof Error ? cause.message : fallback

export function TagsPage() {
  const [data, setData] = useState(emptyPage)
  const [search, setSearch] = useState('')
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [editing, setEditing] = useState<Tag | null | undefined>()
  const [confirmation, setConfirmation] = useState<Confirmation>()
  const [inspecting, setInspecting] = useState<Tag>()
  const [usage, setUsage] = useState<TagUsage>()
  const [tagQuestions, setTagQuestions] = useState<TagQuestionsPage>()
  const [mergeSource, setMergeSource] = useState<Tag>()
  const nextRequest = useLatestRequest()

  const load = useCallback(async () => {
    setLoading(true); setError(undefined)
    const signal = nextRequest()
    try { setData(await tagsApi.getTags(query, page, 10, signal)) }
    catch (cause) { if (!isAbortError(cause)) setError(errorMessage(cause, 'Не удалось загрузить теги.')) }
    finally { if (!signal.aborted) setLoading(false) }
  }, [nextRequest, page, query])
  useEffect(() => { void load() }, [load])

  function submitSearch(event: FormEvent) { event.preventDefault(); setPage(1); setQuery(search) }
  async function remove(tag: Tag) {
    try {
      const currentUsage = await tagsApi.getUsage(tag.id)
      setConfirmation({
        title: 'Удалить тег?',
        description: currentUsage.questionCount ? `Тег «${tag.name}» используется в ${currentUsage.questionCount} вопросах. Сначала объедините его с другим тегом.` : `Тег «${tag.name}» будет удалён без возможности восстановления.`,
        confirmLabel: 'Удалить', danger: true,
        onConfirm: async () => { try { await tagsApi.deleteTag(tag.id); await load() } catch (cause) { setError(errorMessage(cause, 'Не удалось удалить тег.')) } },
      })
    } catch (cause) { setError(errorMessage(cause, 'Не удалось проверить использование тега.')) }
  }
  async function inspect(tag: Tag) {
    setInspecting(tag); setUsage(undefined); setTagQuestions(undefined)
    try {
      const [nextUsage, questions] = await Promise.all([tagsApi.getUsage(tag.id), tagsApi.getQuestions(tag.id)])
      setUsage(nextUsage); setTagQuestions(questions)
    } catch (cause) { setError(errorMessage(cause, 'Не удалось загрузить использование тега.')) }
  }
  function merge(source: Tag, targetId: string) {
    const target = data.items.find((tag) => tag.id === targetId)
    if (!target) return
    setConfirmation({ title: 'Объединить теги?', description: `Тег «${source.name}» будет объединён с тегом «${target.name}», затем исходный тег удалится.`, confirmLabel: 'Объединить', danger: true, onConfirm: async () => {
      try { await tagsApi.mergeTags(source.id, target.id); setMergeSource(undefined); await load() }
      catch (cause) { setError(errorMessage(cause, 'Не удалось объединить теги.')) }
    } })
  }

  const pageCount = Math.max(1, Math.ceil(data.totalCount / data.pageSize))
  return <section className="page-shell">
    <ErrorToast message={error} onClose={() => setError(undefined)} /><ConfirmDialog confirmation={confirmation} onClose={() => setConfirmation(undefined)} />
    <div className="flex flex-wrap items-end justify-between gap-4"><div><h1 className="page-title">Теги</h1><p className="page-description">Создавайте и редактируйте классификацию вопросов.</p></div><button className="button-primary" onClick={() => setEditing(null)}><Plus size={17} /> Новый тег</button></div>
    <div className="card mt-8 p-0">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 p-4"><form className="relative w-full max-w-sm" onSubmit={submitSearch}><Search className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} /><input className="input input-with-icon" onChange={(event) => setSearch(event.target.value)} placeholder="Поиск по названию" value={search} /></form><p className="text-sm text-slate-500">Всего: {data.totalCount}</p></div>
      <div className="overflow-x-auto"><table className="w-full text-left text-sm"><thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500"><tr><th className="px-5 py-3 font-medium">Название</th><th className="px-5 py-3 font-medium">Описание</th><th className="w-28 px-5 py-3"><span className="sr-only">Действия</span></th></tr></thead><tbody className="divide-y divide-slate-100">
        {loading ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={3}>Загрузка…</td></tr> : data.items.length === 0 ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={3}>Теги не найдены</td></tr> : data.items.map((tag) => <tr className="hover:bg-slate-50/70" key={tag.id}><td className="px-5 py-4 font-medium">{tag.name}</td><td className="max-w-xl px-5 py-4 text-slate-600">{tag.description || '—'}</td><td className="px-5 py-4"><div className="flex justify-end gap-1"><button className="icon-button" onClick={() => setEditing(tag)} title="Редактировать"><Pencil size={16} /></button><button className="icon-button" onClick={() => void inspect(tag)} title="Использование"><Eye size={16} /></button><button className="icon-button" onClick={() => setMergeSource(tag)} title="Объединить"><Merge size={16} /></button><button className="icon-button text-rose-600 hover:bg-rose-50" onClick={() => void remove(tag)} title="Удалить"><Trash2 size={16} /></button></div></td></tr>)}
      </tbody></table></div>
      <div className="flex items-center justify-between border-t border-slate-200 px-5 py-4"><p className="text-sm text-slate-500">Страница {data.page} из {pageCount}</p><div className="flex gap-2"><button className="button-secondary px-3" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}><ChevronLeft size={17} /></button><button className="button-secondary px-3" disabled={page >= pageCount} onClick={() => setPage((value) => value + 1)}><ChevronRight size={17} /></button></div></div>
    </div>
    {editing !== undefined && <TagDialog tag={editing} onClose={() => setEditing(undefined)} onSaved={async () => { setEditing(undefined); await load() }} />}
    {inspecting && <div className="fixed inset-0 z-40 grid place-items-center bg-slate-950/40 p-4"><div className="card max-h-[80vh] w-full max-w-2xl overflow-auto"><div className="flex items-start justify-between gap-4"><div><h2 className="text-lg font-semibold">Вопросы с тегом «{inspecting.name}»</h2><p className="mt-1 text-sm text-slate-500">Вопросов: {usage?.questionCount ?? '—'}</p></div><button className="icon-button" onClick={() => setInspecting(undefined)}><X size={18} /></button></div><div className="mt-5 space-y-2">{tagQuestions?.items.length === 0 ? <p className="text-sm text-slate-500">Тег пока не используется.</p> : tagQuestions?.items.map((question) => <a className="block rounded-lg border border-slate-200 p-3 text-sm hover:border-indigo-300 dark:border-slate-800" href={`/questions/${question.id}`} key={question.id}><p className="font-medium">{question.text}</p><p className="mt-1 text-xs text-slate-500">{question.type ?? question.kind} · {question.status}</p></a>) ?? <p className="text-sm text-slate-500">Загрузка…</p>}</div></div></div>}
    {mergeSource && <div className="fixed inset-0 z-40 grid place-items-center bg-slate-950/40 p-4"><div className="card w-full max-w-md"><div className="flex items-start justify-between gap-4"><div><h2 className="text-lg font-semibold">Объединить тег</h2><p className="mt-1 text-sm text-slate-500">Исходный тег: «{mergeSource.name}»</p></div><button className="icon-button" onClick={() => setMergeSource(undefined)}><X size={18} /></button></div><label className="mt-5 block text-sm font-medium">Целевой тег<select className="input mt-2" defaultValue="" onChange={(event) => { if (event.target.value) merge(mergeSource, event.target.value) }}><option disabled value="">Выберите тег</option>{data.items.filter((tag) => tag.id !== mergeSource.id).map((tag) => <option key={tag.id} value={tag.id}>{tag.name}</option>)}</select></label></div></div>}
  </section>
}
