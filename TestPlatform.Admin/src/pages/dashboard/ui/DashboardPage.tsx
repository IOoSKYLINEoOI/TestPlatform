import { BookOpen, CheckCircle2, ClipboardList, History, Tags, Timer } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { dashboardApi } from '@/entities/dashboard'
import type { DashboardStats } from '@/entities/dashboard'
import { isAbortError } from '@/shared/api/httpClient'
import { ErrorToast } from '@/shared/ui'

const cards = [
  { label: 'Экзамены', note: 'Управление расписанием и публикацией', icon: ClipboardList, to: '/exams' },
  { label: 'Банк вопросов', note: 'Создание и проверка вопросов', icon: BookOpen, to: '/questions' },
  { label: 'Теги', note: 'Классификация учебного контента', icon: Tags, to: '/tags' },
  { label: 'Попытки', note: 'История прохождений и результаты пользователей', icon: History, to: '/attempts' },
]

export function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats>()
  const [error, setError] = useState<string>()
  useEffect(() => { const controller = new AbortController(); dashboardApi.getStats(controller.signal).then(setStats).catch((cause) => { if (!isAbortError(cause)) setError(cause instanceof Error ? cause.message : 'Не удалось загрузить статистику.') }); return () => controller.abort() }, [])
  const metrics = [
    { label: 'Активные экзамены', value: stats?.activeExams, icon: ClipboardList },
    { label: 'Всего попыток', value: stats?.totalAttempts, icon: History },
    { label: 'Незавершённые', value: stats?.unfinishedAttempts, icon: Timer },
    { label: 'Процент сдачи', value: stats ? `${stats.passRate}%` : undefined, icon: CheckCircle2 },
  ]
  return (
    <section className="page-shell">
      <ErrorToast message={error} onClose={() => setError(undefined)} />
      <div><h1 className="page-title">Обзор</h1><p className="page-description">Текущее состояние учебной платформы.</p></div>
      <div className="mt-8 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{metrics.map(({ label, value, icon: Icon }) => <div className="card" key={label}><div className="flex items-center justify-between"><p className="text-sm text-slate-500">{label}</p><Icon className="text-indigo-500" size={19} /></div><p className="mt-3 text-3xl font-semibold">{value ?? '—'}</p></div>)}</div>
      <div className="mt-8 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {cards.map(({ label, note, icon: Icon, to }) => (
          <Link className="card transition hover:-translate-y-0.5 hover:border-indigo-200 hover:shadow-md" key={label} to={to}>
            <span className="grid size-10 place-items-center rounded-lg bg-indigo-50 text-indigo-600"><Icon size={20} /></span>
            <h2 className="mt-5 font-semibold">{label}</h2>
            <p className="mt-1 text-sm text-slate-500">{note}</p>
          </Link>
        ))}
      </div>
    </section>
  )
}
