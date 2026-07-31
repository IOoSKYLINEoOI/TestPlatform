import { ArrowLeft } from 'lucide-react'
import type { QuestionKind } from '@/entities/question'
import type { Tag } from '@/entities/tag'
import type { ChoiceOption, MatchPair } from './QuestionAnswerFields'

type Props = { onClose: () => void; tags: Tag[]; tagIds: string[]; text: string; imageUrl: string | null; kind: QuestionKind; options: ChoiceOption[]; correctAnswer: string; pairs: MatchPair[]; explanation: string }

export function QuestionReadOnlyView({ onClose, tags, tagIds, text, imageUrl, kind, options, correctAnswer, pairs, explanation }: Props) {
  return <section className="page-shell"><button className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900" onClick={onClose} type="button"><ArrowLeft size={17} /> Назад к вопросам</button><div className="card mx-auto mt-5 w-full max-w-4xl"><h1 className="text-xl font-semibold">Вопрос</h1><div className="mt-5 flex flex-wrap gap-2">{tags.filter((tag) => tagIds.includes(tag.id)).map((tag) => <span className="rounded-full bg-indigo-50 px-3 py-1 text-xs text-indigo-700" key={tag.id}>{tag.name}</span>)}</div><p className="mt-5 whitespace-pre-wrap text-lg font-medium">{text}</p>{imageUrl && <img alt="Изображение вопроса" className="mt-5 max-h-80 rounded-xl object-contain" src={imageUrl} />}{kind === 'choice' && <div className="mt-6 space-y-2">{options.map((option, index) => <div className={option.isCorrect ? 'rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3' : 'rounded-lg border border-slate-200 px-4 py-3'} key={index}>{option.text}</div>)}</div>}{(kind === 'text' || kind === 'number') && <p className="mt-6 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3">Правильный ответ: {correctAnswer}</p>}{kind === 'matching' && <div className="mt-6 space-y-2">{pairs.map((pair) => <div className="grid grid-cols-[1fr_auto_1fr] gap-3 rounded-lg border border-slate-200 px-4 py-3" key={pair.leftId}><span>{pair.left}</span><span>→</span><span>{pair.right}</span></div>)}</div>}{explanation && <div className="mt-6"><p className="text-sm font-medium text-slate-500">Пояснение</p><p className="mt-2 whitespace-pre-wrap">{explanation}</p></div>}</div></section>
}


