import { Check, ChevronDown } from 'lucide-react'
import { useState } from 'react'
import { questionsApi } from '../api/questionsApi'
import type { Question, QuestionEditor } from '../model/types'
import { QuestionPreview } from './QuestionPreview'

export function ReadOnlyQuestionList({ ids, questions }: { ids: string[]; questions: Question[] }) {
  const [expanded, setExpanded] = useState<string>()
  const [details, setDetails] = useState<Record<string, QuestionEditor>>({})
  const [loading, setLoading] = useState<string>()
  async function toggle(id: string) {
    if (expanded === id) return setExpanded(undefined)
    setExpanded(id)
    if (details[id]) return
    setLoading(id)
    try { const question = await questionsApi.getQuestion(id); setDetails((current) => ({ ...current, [id]: question })) }
    finally { setLoading(undefined) }
  }
  return <div className="mt-5 overflow-hidden rounded-lg border border-slate-200">{ids.length ? ids.map((id, index) => { const question = questions.find((item) => item.id === id); const detail = details[id]; const open = expanded === id; return <div className="border-b border-slate-100 last:border-b-0" key={id}><button className="flex w-full items-center gap-3 p-4 text-left hover:bg-slate-50" onClick={() => void toggle(id)} type="button"><span className="grid size-7 shrink-0 place-items-center rounded-full bg-indigo-50 text-xs font-semibold text-indigo-700">{index + 1}</span><span className="min-w-0 flex-1 truncate text-sm font-medium">{question?.text ?? 'Вопрос'}</span>{question && <span className="rounded-full bg-slate-100 px-2 py-1 text-xs text-slate-600">{question.kind}</span>}<ChevronDown className={`text-slate-400 transition-transform ${open ? 'rotate-180' : ''}`} size={18} /></button>{open && <div className="border-t border-slate-100 bg-slate-50 px-4 py-4 text-sm">{loading === id ? <p className="text-slate-500">Загружаем вопрос…</p> : detail ? <QuestionDetails detail={detail} /> : <p className="text-rose-600">Не удалось загрузить вопрос.</p>}</div>}</div> }) : <p className="p-6 text-center text-sm text-slate-500">Вопросов нет</p>}</div>
}

function QuestionDetails({ detail }: { detail: QuestionEditor }) { return <QuestionPreview question={detail} /> }


