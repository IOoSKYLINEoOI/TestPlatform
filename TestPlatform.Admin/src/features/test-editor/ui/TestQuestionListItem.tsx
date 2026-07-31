import { Check, ChevronDown, GripVertical, Trash2 } from "lucide-react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import type { Question, QuestionEditor } from '@/entities/question'
import { QuestionPreview } from "@/entities/question";

type Props = { id: string; index: number; text: string; kind?: Question["kind"]; editable: boolean; expanded: boolean; loading: boolean; details?: QuestionEditor; imageUrl?: string; optionImageUrls: Record<string, string>; onToggle: () => void; onRemove: () => void };

export function TestQuestionListItem({ id, index, text, kind, editable, expanded, loading, details, imageUrl, optionImageUrls, onToggle, onRemove }: Props) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id, disabled: !editable });
  return <div ref={setNodeRef} style={{ transform: CSS.Transform.toString(transform), transition }} className={isDragging ? "relative z-10 border-b border-slate-100 bg-indigo-50 shadow-lg" : "relative border-b border-slate-100 bg-white last:border-b-0"}>
    <div className="flex cursor-pointer items-center gap-3 p-4 hover:bg-slate-50" onClick={onToggle} role="button" tabIndex={0} onKeyDown={(event) => { if (event.key === "Enter" || event.key === " ") { event.preventDefault(); onToggle(); } }}>
      {editable && <button className="touch-none cursor-grab rounded p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600 active:cursor-grabbing" onClick={(event) => event.stopPropagation()} title="Перетащить" type="button" {...attributes} {...listeners}><GripVertical size={19} /></button>}
      <span className="grid size-7 shrink-0 place-items-center rounded-full bg-indigo-50 text-xs font-semibold text-indigo-700">{index + 1}</span><p className="min-w-0 flex-1 text-sm font-medium">{text}</p>
      {kind && <span className="shrink-0 rounded-full bg-indigo-100 px-2.5 py-1 text-xs font-semibold text-indigo-700">{kindLabel(kind)}</span>}
      {editable && <button className="icon-button shrink-0 text-rose-600" onClick={(event) => { event.stopPropagation(); onRemove(); }} title="Удалить из теста"><Trash2 size={16} /></button>}
      <ChevronDown className={expanded ? "shrink-0 rotate-180 text-slate-400 transition-transform duration-200" : "shrink-0 text-slate-400 transition-transform duration-200"} size={18} />
    </div>
    <div className={expanded ? "grid grid-rows-[1fr] transition-[grid-template-rows] duration-300 ease-in-out" : "grid grid-rows-[0fr] transition-[grid-template-rows] duration-300 ease-in-out"}><div className="overflow-hidden">{expanded && <div className="border-t border-slate-100 bg-slate-50/70 px-4 pb-5 pt-4">{loading ? <p className="text-sm text-slate-500">Загрузка вопроса…</p> : details ? <QuestionPreview question={details} imageUrl={imageUrl} optionImageUrls={optionImageUrls} /> : null}</div>}</div></div>
  </div>;
}

function QuestionDetails({ details, imageUrl, optionImageUrls }: { details: QuestionEditor; imageUrl?: string; optionImageUrls: Record<string, string> }) {
  return <div className="space-y-4 text-sm"><div className="flex flex-wrap gap-2"><span className="rounded-full bg-indigo-100 px-2.5 py-1 text-xs font-medium text-indigo-700">{kindLabel(details.kind)}</span>{details.tags.map((tag) => <span className="rounded-full bg-white px-2.5 py-1 text-xs text-slate-600 ring-1 ring-slate-200" key={tag.id}>{tag.name}</span>)}</div>
    {imageUrl && <img alt="Изображение вопроса" className="max-h-72 rounded-xl bg-white object-contain" src={imageUrl} />}
    {details.kind === "choice" && <div><p className="mb-2 text-xs font-medium uppercase tracking-wide text-slate-500">Варианты ответа</p><div className="space-y-2">{details.options?.map((option) => <div className={option.isCorrect ? "flex items-center gap-3 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-emerald-800" : "flex items-center gap-3 rounded-lg border border-slate-200 bg-white px-3 py-2"} key={option.id}>{option.isCorrect && <Check size={16} />}{option.imageId && optionImageUrls[option.imageId] && <img alt="Изображение варианта" className="size-12 rounded-md object-cover" src={optionImageUrls[option.imageId]} />}<span>{option.text}</span></div>)}</div></div>}
    {(details.kind === "text" || details.kind === "number") && <div><p className="text-xs font-medium uppercase tracking-wide text-slate-500">Правильный ответ</p><p className="mt-1 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-emerald-800">{String(details.correctAnswer ?? "—")}</p></div>}
    {details.kind === "matching" && <div className="space-y-2">{details.pairs?.map((pair) => <div className="grid grid-cols-[1fr_auto_1fr] items-center gap-3 rounded-lg border border-slate-200 bg-white px-3 py-2" key={`${pair.leftId}-${pair.rightId}`}><span>{details.leftItems?.find((item) => item.id === pair.leftId)?.text}</span><span className="text-slate-400">→</span><span>{details.rightItems?.find((item) => item.id === pair.rightId)?.text}</span></div>)}</div>}
    {details.explanation && <div><p className="text-xs font-medium uppercase tracking-wide text-slate-500">Пояснение</p><p className="mt-1 whitespace-pre-wrap text-slate-700">{details.explanation}</p></div>}
  </div>;
}

function kindLabel(kind: Question["kind"]) { return ({ choice: "Выбор ответа", text: "Текстовый ответ", number: "Числовой ответ", matching: "Сопоставление" } as const)[kind]; }
