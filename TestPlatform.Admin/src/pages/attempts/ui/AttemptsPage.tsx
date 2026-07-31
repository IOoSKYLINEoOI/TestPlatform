import { ArrowLeft, Ban, ChevronLeft, ChevronRight, Download, Search } from 'lucide-react'
import { FormEvent, useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { attemptsApi, normalizeAttemptSourceType } from '@/entities/attempt'
import type { AttemptListItem, AttemptsPage as AttemptsPageData, AttemptSourceType, AttemptStatus } from '@/entities/attempt'
import { exportAttemptsCsv, exportAttemptsXlsx } from '@/features/attempt-export'
import { isAbortError } from '@/shared/api/httpClient'
import { useLatestRequest } from '@/shared/lib'
import { ConfirmDialog, ErrorToast } from '@/shared/ui'
import type { Confirmation } from '@/shared/ui'

const emptyPage: AttemptsPageData = { items: [], page: 1, pageSize: 20, totalCount: 0 }
const statusOptions: Array<{ value: '' | AttemptStatus; label: string }> = [
  { value: '', label: 'Все статусы' },
  { value: 'notStarted', label: 'Не начата' },
  { value: 'started', label: 'В процессе' },
  { value: 'finished', label: 'Завершена' },
  { value: 'expired', label: 'Истекла' },
  { value: 'abandoned', label: 'Прервана' },
  { value: 'cancelled', label: 'Отменена' },
]

export function AttemptsPage() {
  const { sourceType, sourceId } = useParams<{ sourceType: string; sourceId: string }>()
  const navigate = useNavigate()
  const [data, setData] = useState(emptyPage)
  const [employee, setEmployee] = useState('')
  const [employeeQuery, setEmployeeQuery] = useState('')
  const [status, setStatus] = useState<'' | AttemptStatus>('')
  const [passed, setPassed] = useState('')
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string>()
  const [confirmation, setConfirmation] = useState<Confirmation>()
  const [exporting, setExporting] = useState(false)
  const nextRequest = useLatestRequest()
  const type: AttemptSourceType = normalizeAttemptSourceType(sourceType)
  const backPath = `/${type === 'test' ? 'tests' : 'exams'}/${sourceId ?? ''}`

  const load = useCallback(async () => {
    if (!sourceId) return
    const signal = nextRequest()
    setLoading(true)
    setError(undefined)
    try {
      setData(await attemptsApi.getBySource(type, sourceId, {
        page,
        status: status || undefined,
        employeeNumber: employeeQuery,
        passed: passed === '' ? undefined : passed === 'true',
      }, signal))
    } catch (cause) {
      if (!isAbortError(cause)) setError(message(cause, 'Не удалось загрузить попытки.'))
    } finally {
      if (!signal.aborted) setLoading(false)
    }
  }, [employeeQuery, nextRequest, page, passed, sourceId, status, type])

  useEffect(() => { void load() }, [load])

  function search(event: FormEvent) {
    event.preventDefault()
    setPage(1)
    setEmployeeQuery(employee)
  }

  function confirmCancel(item: AttemptListItem) {
    setConfirmation({
      title: 'Отменить попытку?',
      description: `Попытка №${item.attemptNumber} сотрудника ${item.employeeNumber} будет отменена.`,
      confirmLabel: 'Отменить попытку',
      danger: true,
      onConfirm: async () => {
        try {
          await attemptsApi.cancel(item.attemptId)
          await load()
        } catch (cause) {
          setError(message(cause, 'Не удалось отменить попытку.'))
        }
      },
    })
  }

  const pages = Math.max(1, Math.ceil(data.totalCount / data.pageSize))

  async function exportAll(format: 'csv' | 'xlsx') {
    if (!sourceId) return
    setExporting(true)
    setError(undefined)
    try {
      const query = {
        pageSize: 100,
        status: status || undefined,
        employeeNumber: employeeQuery,
        passed: passed === '' ? undefined : passed === 'true',
      }
      const first = await attemptsApi.getBySource(type, sourceId, { ...query, page: 1 })
      const all = [...first.items]
      const pageCount = Math.ceil(first.totalCount / first.pageSize)
      for (let nextPage = 2; nextPage <= pageCount; nextPage += 1) {
        const next = await attemptsApi.getBySource(type, sourceId, { ...query, page: nextPage })
        all.push(...next.items)
      }
      const filename = `attempts-${type}-${sourceId}-${new Date().toISOString().slice(0, 10)}`
      if (format === 'csv') exportAttemptsCsv(all, filename)
      else await exportAttemptsXlsx(all, filename)
    } catch (cause) {
      setError(message(cause, 'Не удалось экспортировать попытки.'))
    } finally {
      setExporting(false)
    }
  }

  return <section className="page-shell">
    <ErrorToast message={error} onClose={() => setError(undefined)} />
    <ConfirmDialog confirmation={confirmation} onClose={() => setConfirmation(undefined)} />
    <Link className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900" to={backPath}>
      <ArrowLeft size={17} /> Назад к {type === 'test' ? 'тесту' : 'экзамену'}
    </Link>
    <div className="mt-5 flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 className="page-title">Попытки {type === 'test' ? 'теста' : 'экзамена'}</h1>
        <p className="page-description">История прохождений, результаты и управление активными попытками.</p>
      </div>
      <div className="flex gap-2">
        <button className="button-secondary" disabled={exporting || data.totalCount === 0} onClick={() => void exportAll('csv')}><Download size={16} /> CSV</button>
        <button className="button-secondary" disabled={exporting || data.totalCount === 0} onClick={() => void exportAll('xlsx')}><Download size={16} /> XLSX</button>
      </div>
    </div>
    <div className="card mt-8 p-0">
      <div className="flex flex-wrap gap-3 border-b border-slate-200 p-4 dark:border-slate-800">
        <form className="relative min-w-64 flex-1" onSubmit={search}>
          <Search className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
          <input className="input input-with-icon" onChange={(event) => setEmployee(event.target.value)} placeholder="Табельный номер" value={employee} />
        </form>
        <select className="input w-auto" onChange={(event) => { setPage(1); setStatus(event.target.value as '' | AttemptStatus) }} value={status}>
          {statusOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
        </select>
        {type === 'exam' && <select className="input w-auto" onChange={(event) => { setPage(1); setPassed(event.target.value) }} value={passed}>
          <option value="">Любой результат</option><option value="true">Сдан</option><option value="false">Не сдан</option>
        </select>}
      </div>
      <div className="overflow-x-auto"><table className="w-full text-left text-sm">
        <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500 dark:bg-slate-900"><tr>
          <th className="px-5 py-3">№</th><th className="px-5 py-3">Сотрудник</th><th className="px-5 py-3">Статус</th><th className="px-5 py-3">Ответы</th><th className="px-5 py-3">Результат</th><th className="px-5 py-3">Время</th><th className="px-5 py-3"><span className="sr-only">Действия</span></th>
        </tr></thead>
        <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
          {loading ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={7}>Загрузка…</td></tr>
            : data.items.length === 0 ? <tr><td className="px-5 py-12 text-center text-slate-500" colSpan={7}>Попытки не найдены</td></tr>
              : data.items.map((item) => <AttemptRow item={item} key={item.attemptId} onCancel={() => confirmCancel(item)} onOpen={() => navigate(`/attempts/${item.attemptId}`, { state: { backPath } })} />)}
        </tbody>
      </table></div>
      <div className="flex items-center justify-between border-t border-slate-200 px-5 py-4 dark:border-slate-800">
        <p className="text-sm text-slate-500">Всего: {data.totalCount}. Страница {data.page} из {pages}</p>
        <div className="flex gap-2"><button className="button-secondary px-3" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}><ChevronLeft size={17} /></button><button className="button-secondary px-3" disabled={page >= pages} onClick={() => setPage((value) => value + 1)}><ChevronRight size={17} /></button></div>
      </div>
    </div>
  </section>
}

function AttemptRow({ item, onOpen, onCancel }: { item: AttemptListItem; onOpen: () => void; onCancel: () => void }) {
  const result = item.percentage == null ? '—' : `${formatNumber(item.percentage)}%${item.passed == null ? '' : item.passed ? ' · сдан' : ' · не сдан'}`
  const cancellable = item.status === 'started' || item.status === 'notStarted'
  const openable = item.status === 'finished'
  return <tr aria-label={openable ? `Открыть попытку №${item.attemptNumber}` : undefined} className={`${openable ? 'cursor-pointer' : ''} hover:bg-slate-50/70 dark:hover:bg-slate-900/60`} onClick={openable ? onOpen : undefined} onKeyDown={openable ? (event) => { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); onOpen() } } : undefined} role={openable ? 'link' : undefined} tabIndex={openable ? 0 : undefined}>
    <td className="px-5 py-4 font-medium">{item.attemptNumber}</td><td className="px-5 py-4"><p className="font-medium">{item.employeeNumber}</p><p className="text-xs text-slate-500">{item.userId}</p></td><td className="px-5 py-4"><StatusBadge status={item.status} /></td><td className="px-5 py-4">{item.answeredQuestions} / {item.totalQuestions}</td><td className="px-5 py-4">{result}</td><td className="px-5 py-4 text-slate-500">{formatDate(item.finishedAt ?? item.startedAt)}</td><td className="px-5 py-4"><div className="flex justify-end gap-1">{cancellable && <button className="icon-button text-red-600 hover:bg-red-50" onClick={(event) => { event.stopPropagation(); onCancel() }} title="Отменить попытку"><Ban size={16} /></button>}</div></td>
  </tr>
}

function StatusBadge({ status }: { status: AttemptStatus }) {
  const labels: Record<AttemptStatus, string> = { notStarted: 'Не начата', started: 'В процессе', finished: 'Завершена', expired: 'Истекла', abandoned: 'Прервана', cancelled: 'Отменена' }
  const tone = status === 'finished' ? 'bg-emerald-50 text-emerald-700' : status === 'started' || status === 'notStarted' ? 'bg-indigo-50 text-indigo-700' : 'bg-slate-100 text-slate-600'
  return <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${tone}`}>{labels[status] ?? status}</span>
}

function formatDate(value: string | null) { return value ? new Intl.DateTimeFormat('ru-RU', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value)) : '—' }
function formatNumber(value: number) { return new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 1 }).format(value) }
function message(cause: unknown, fallback: string) { return cause instanceof Error ? cause.message : fallback }
