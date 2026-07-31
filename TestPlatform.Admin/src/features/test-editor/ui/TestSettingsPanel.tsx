import { ImagePlus } from 'lucide-react'
import type { TestDetails } from '@/entities/test'

type Props = {
  test: TestDetails
  editable: boolean
  busy: boolean
  hasTimeLimit: boolean
  timeLimitMinutes: string
  coverUrl?: string
  onToggleTimeLimit: (enabled: boolean) => Promise<void>
  onTimeLimitChange: (value: string) => void
  onSaveTimeLimit: () => Promise<void>
  onUploadCover: (file: File) => Promise<void>
  onRemoveCover: () => Promise<void>
}

export function TestSettingsPanel(props: Props) {
  const { test, editable, busy, hasTimeLimit, timeLimitMinutes, coverUrl } = props
  return <div className="space-y-6">
    <div className="card">
      <h2 className="text-lg font-semibold">Информация о тесте</h2>
      <dl className="mt-5 divide-y divide-slate-100 text-sm">
        <InfoRow label="Статус" value={statusLabel(test.status)} />
        <InfoRow label="Вопросов" value={String(test.questions.length)} />
        <InfoRow label="Время" value={test.timeLimitSeconds ? formatDuration(test.timeLimitSeconds) : 'Без ограничения'} />
        <InfoRow label="Создан" value={formatDate(test.createdAt)} />
        <InfoRow label="Изменён" value={formatDate(test.updatedAt)} />
        {test.publishedAt && <InfoRow label="Опубликован" value={formatDate(test.publishedAt)} />}
        <InfoRow label="ID" value={test.id} mono />
      </dl>
    </div>
    <div className="card">
      <h2 className="text-lg font-semibold">Ограничение времени</h2>
      <label className={`mt-4 flex items-center gap-3 text-sm font-medium ${editable ? 'cursor-pointer' : 'text-slate-500'}`}>
        <input checked={hasTimeLimit} className="size-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500" disabled={!editable || busy} onChange={(event) => void props.onToggleTimeLimit(event.target.checked)} type="checkbox" />
        Ограничить время прохождения
      </label>
      {hasTimeLimit && <div className="mt-5 flex gap-2">
        <label className="min-w-0 flex-1"><span className="label">Время в минутах</span><input className="input mt-2" disabled={!editable} min="2" onChange={(event) => props.onTimeLimitChange(event.target.value)} placeholder="Например, 30" required step="1" type="number" value={timeLimitMinutes} /></label>
        {editable && <button className="button-secondary mt-7 shrink-0" disabled={busy || Number(timeLimitMinutes) < 2} onClick={() => void props.onSaveTimeLimit()} type="button">Сохранить</button>}
      </div>}
    </div>
    <div className="card">
      <h2 className="text-lg font-semibold">Обложка</h2>
      <div className="mt-5">{coverUrl ? <>
        <img alt="Обложка теста" className="max-h-64 w-full rounded-xl bg-slate-50 object-contain" src={coverUrl} />
        {editable && <button className="button-secondary mt-3 w-full text-rose-600" disabled={busy} onClick={() => void props.onRemoveCover()} type="button">Удалить обложку</button>}
      </> : editable ? <label className="flex cursor-pointer flex-col items-center gap-2 rounded-xl border border-dashed border-slate-300 bg-slate-50 px-4 py-10 text-sm text-slate-600 hover:bg-slate-100">
        <ImagePlus size={24} /> Загрузить изображение
        <input accept="image/*" className="sr-only" disabled={busy} onChange={(event) => { const file = event.target.files?.[0]; if (file) void props.onUploadCover(file) }} type="file" />
      </label> : <p className="text-sm text-slate-500">Обложка не задана</p>}</div>
    </div>
  </div>
}

function InfoRow({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return <div className="grid grid-cols-[7rem_1fr] gap-3 py-3 first:pt-0 last:pb-0"><dt className="text-slate-500">{label}</dt><dd className={`min-w-0 text-right font-medium ${mono ? 'truncate font-mono text-xs' : ''}`} title={mono ? value : undefined}>{value}</dd></div>
}
function formatDate(value: string) { return new Intl.DateTimeFormat('ru-RU', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)) }
function formatDuration(seconds: number) { return `${Math.round(seconds / 60)} мин.` }
function statusLabel(status: string) { return ({ draft: 'Черновик', published: 'Опубликован', archived: 'В архиве' } as Record<string, string>)[status.toLowerCase()] ?? status }
