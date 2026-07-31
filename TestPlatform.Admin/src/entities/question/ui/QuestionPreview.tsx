import { Check } from "lucide-react";
import type { Question, QuestionEditor } from '../model/types'

type Props = { question: QuestionEditor; imageUrl?: string; optionImageUrls?: Record<string, string> };

export function QuestionPreview({ question, imageUrl, optionImageUrls = {} }: Props) {
  return <div className="space-y-4 text-sm">
    <div className="flex flex-wrap gap-2"><span className="rounded-full bg-indigo-100 px-2.5 py-1 text-xs font-medium text-indigo-700">{questionKindLabel(question.kind)}</span>{question.tags.map((tag) => <span className="rounded-full bg-white px-2.5 py-1 text-xs text-slate-600 ring-1 ring-slate-200" key={tag.id}>{tag.name}</span>)}</div>
    {imageUrl && <img alt="Изображение вопроса" className="max-h-72 rounded-xl bg-white object-contain" src={imageUrl} />}
    {question.kind === "choice" && <div className="space-y-2">{question.options?.map((option) => <div className={option.isCorrect ? "rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-emerald-800" : "rounded-lg border border-slate-200 bg-white px-3 py-2"} key={option.id}>{option.isCorrect && <Check className="mr-2 inline" size={15} />}{option.imageId && optionImageUrls[option.imageId] && <img alt="Изображение варианта" className="mr-3 inline-block size-12 rounded-md object-cover" src={optionImageUrls[option.imageId]} />}{option.text}</div>)}</div>}
    {(question.kind === "text" || question.kind === "number") && <p className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2">Правильный ответ: {String(question.correctAnswer ?? "—")}</p>}
    {question.kind === "matching" && <div className="space-y-2">{question.pairs?.map((pair) => <div className="grid grid-cols-[1fr_auto_1fr] gap-3 rounded-lg border border-slate-200 bg-white px-3 py-2" key={`${pair.leftId}-${pair.rightId}`}><span>{question.leftItems?.find((item) => item.id === pair.leftId)?.text}</span><span>→</span><span>{question.rightItems?.find((item) => item.id === pair.rightId)?.text}</span></div>)}</div>}
    {question.explanation && <div><p className="text-xs font-medium text-slate-500">Пояснение</p><p className="mt-1 whitespace-pre-wrap">{question.explanation}</p></div>}
  </div>;
}

function questionKindLabel(kind: Question["kind"]) { return ({ choice: "Выбор ответа", text: "Текстовый ответ", number: "Числовой ответ", matching: "Сопоставление" } as const)[kind]; }
