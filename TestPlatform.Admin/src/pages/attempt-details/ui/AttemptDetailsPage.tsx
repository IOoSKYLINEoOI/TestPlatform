import { ArrowLeft, CheckCircle2, XCircle } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { attemptsApi } from '@/entities/attempt'
import type { AttemptDetails } from '@/entities/attempt'
import { isAbortError } from '@/shared/api/httpClient'
import { ErrorToast } from '@/shared/ui'

export function AttemptDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const location = useLocation()
  const [attempt, setAttempt] = useState<AttemptDetails>()
  const [error, setError] = useState<string>()
  const backPath = (location.state as { backPath?: string } | null)?.backPath ?? '/tests'

  useEffect(() => {
    if (!id) return
    const controller = new AbortController()
    attemptsApi.getResult(id, controller.signal).then(setAttempt).catch((cause) => {
      if (!isAbortError(cause)) setError(cause instanceof Error ? cause.message : 'Не удалось загрузить результат попытки.')
    })
    return () => controller.abort()
  }, [id])

  if (!attempt && !error) return <section className="page-shell text-sm text-slate-500">Загружаем результат…</section>

  return <section className="page-shell">
    <ErrorToast message={error} onClose={() => setError(undefined)} />
    <Link className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900" to={backPath}><ArrowLeft size={17} /> Назад к попыткам</Link>
    {attempt && <>
      <div className="mt-5 flex flex-wrap items-end justify-between gap-4">
        <div><p className="text-sm font-medium text-indigo-600">Попытка №{attempt.attemptNumber}</p><h1 className="page-title">{attempt.sourceTitle || 'Результат попытки'}</h1><p className="page-description">Сотрудник: {attempt.employeeNumber || attempt.userId} · завершена {formatDate(attempt.finishedAt)}</p></div>
        {attempt.type === 'exam' && attempt.passed != null && <span className={`rounded-full px-3 py-1.5 text-sm font-medium ${attempt.passed ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-700'}`}>{attempt.passed ? 'Экзамен сдан' : 'Экзамен не сдан'}</span>}
      </div>
      <div className="mt-8 grid gap-4 sm:grid-cols-2 xl:grid-cols-4"><Metric label="Правильных ответов" value={`${attempt.correctAnswers} из ${attempt.totalQuestions}`} /><Metric label="Результат" value={`${formatNumber(attempt.percentage)}%`} />{attempt.earnedPoints != null && <Metric label="Набрано баллов" value={`${formatNumber(attempt.earnedPoints)} из ${formatNumber(attempt.totalMaxScore ?? 0)}`} />}<Metric label="Время прохождения" value={formatDuration(attempt.startedAt, attempt.finishedAt)} /></div>
      <div className="mt-6 space-y-4">{attempt.questions.map((item) => <article className="card" key={item.question.id}>
        <div className="flex items-start justify-between gap-4"><div><p className="text-xs font-medium uppercase tracking-wide text-slate-500">Вопрос {item.order} · {questionKindLabel(item.question.kind)}</p><h2 className="mt-1 font-semibold">{item.question.text}</h2></div><div className={`flex shrink-0 items-center gap-2 text-sm font-medium ${item.isCorrect ? 'text-emerald-600' : 'text-red-600'}`}>{item.isCorrect ? <CheckCircle2 size={21} /> : <XCircle size={21} />}{item.isCorrect ? 'Верно' : 'Неверно'}</div></div>
        <div className="mt-4 grid gap-4 text-sm md:grid-cols-2"><AnswerBlock label="Ответ пользователя" value={formatUserAnswer(item.userAnswer, item.question)} tone={item.isCorrect ? 'correct' : 'incorrect'} /><AnswerBlock label="Правильный ответ" value={formatCorrectAnswer(item.question)} tone="correct" /></div>
        <p className="mt-4 text-sm text-slate-500">Баллы: <span className="font-medium text-slate-900 dark:text-slate-100">{formatNumber(item.earnedScore)} из {formatNumber(item.maxScore)}</span></p>
        {item.question.explanation && <div className="mt-4 rounded-lg bg-slate-50 p-3 text-sm dark:bg-slate-900"><span className="font-medium">Пояснение:</span> {item.question.explanation}</div>}
      </article>)}</div>
    </>}
  </section>
}

function Metric({ label, value }: { label: string; value: string }) { return <div className="card"><p className="text-sm text-slate-500">{label}</p><p className="mt-2 text-2xl font-semibold">{value}</p></div> }
function AnswerBlock({ label, value, tone }: { label: string; value: string; tone: 'correct' | 'incorrect' }) { return <div className={`rounded-lg border p-3 ${tone === 'correct' ? 'border-emerald-200 bg-emerald-50/40 dark:border-emerald-900 dark:bg-emerald-950/20' : 'border-red-200 bg-red-50/40 dark:border-red-900 dark:bg-red-950/20'}`}><p className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</p><p className="mt-2 whitespace-pre-wrap">{value}</p></div> }
function formatUserAnswer(answer: AttemptDetails['questions'][number]['userAnswer'], question: AttemptDetails['questions'][number]['question']) { if (!answer) return 'Нет ответа'; if (answer.selectedOptionIds) return labels(answer.selectedOptionIds, question.options); if (answer.textAnswer != null) return answer.textAnswer; if (answer.numberAnswer != null) return String(answer.numberAnswer); if (answer.matchingPairs) return answer.matchingPairs.map((pair) => `${label(pair.leftOptionId, question.leftItems)} → ${label(pair.rightOptionId, question.rightItems)}`).join('\n'); return 'Ответ сохранён' }
function formatCorrectAnswer(question: AttemptDetails['questions'][number]['question']) { if (question.options) return question.options.filter((option) => option.isCorrect).map((option) => option.text || option.id).join(', ') || '—'; if (question.correctAnswer != null) return String(question.correctAnswer); if (question.pairs) return question.pairs.map((pair) => `${label(pair.leftId, question.leftItems)} → ${label(pair.rightId, question.rightItems)}`).join('\n'); return '—' }
function labels(ids: string[], items?: Array<{ id: string; text: string }>) { return ids.map((id) => label(id, items)).join(', ') }
function label(id: string, items?: Array<{ id: string; text: string }>) { return items?.find((item) => item.id === id)?.text || id }
function formatNumber(value: number) { return new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 2 }).format(value) }
function questionKindLabel(kind: AttemptDetails['questions'][number]['question']['kind']) { return ({ choice: 'выбор ответа', text: 'текстовый ответ', number: 'числовой ответ', matching: 'сопоставление' } as const)[kind] }
function formatDate(value: string | null) { return value ? new Intl.DateTimeFormat('ru-RU', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)) : '—' }
function formatDuration(startedAt: string | null, finishedAt: string | null) { if (!startedAt || !finishedAt) return '—'; const minutes = Math.max(0, Math.round((new Date(finishedAt).getTime() - new Date(startedAt).getTime()) / 60000)); return `${minutes} мин.` }
