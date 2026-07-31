import { Pencil, Plus, Save, Trash2, X } from "lucide-react";
import { useState } from "react";
import type { ExamSection } from '@/entities/exam'
import type { Question } from '@/entities/question'

export function ExamSectionCard({
  section,
  questions,
  editable,
  busy,
  selectedQuestion,
  onSelect,
  onAdd,
  onRemove,
  onRemoveQuestion,
  onUpdate,
}: {
  section: ExamSection;
  questions: Question[];
  editable: boolean;
  busy: boolean;
  selectedQuestion: string;
  onSelect: (value: string) => void;
  onAdd: () => void;
  onRemove: () => void;
  onRemoveQuestion: (id: string) => void;
  onUpdate: (input: { name: string; questionsToSelect: number; scorePerQuestion: number }) => Promise<void>;
}) {
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(section.name);
  const [questionsToSelect, setQuestionsToSelect] = useState(String(section.questionsToSelect));
  const [scorePerQuestion, setScorePerQuestion] = useState(String(section.scorePerQuestion));
  const available = questions.filter(
    (q) => !section.questionIds.includes(q.id),
  );
  return (
    <div className="rounded-xl border border-slate-200 p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
          {editing ? <div className="grid gap-2 md:grid-cols-[1fr_8rem_8rem]"><input className="input" onChange={(event) => setName(event.target.value)} value={name} /><input className="input" min="1" onChange={(event) => setQuestionsToSelect(event.target.value)} type="number" value={questionsToSelect} /><input className="input" min="1" onChange={(event) => setScorePerQuestion(event.target.value)} type="number" value={scorePerQuestion} /></div> : <h3 className="font-semibold">{section.name}</h3>}
          <p className="mt-1 text-sm text-slate-500">
            Выбрано: {section.questionsToSelect} · {section.scorePerQuestion}{" "}
            балл(а) · максимум {section.maxScore}
          </p>
        </div>
        {editable && (
          <div className="flex gap-1">{editing ? <><button className="icon-button text-emerald-700" disabled={busy || !name.trim() || Number(questionsToSelect) < 1 || Number(scorePerQuestion) < 1} onClick={async () => { await onUpdate({ name: name.trim(), questionsToSelect: Number(questionsToSelect), scorePerQuestion: Number(scorePerQuestion) }); setEditing(false) }} title="Сохранить секцию" type="button"><Save size={16} /></button><button className="icon-button" onClick={() => { setName(section.name); setQuestionsToSelect(String(section.questionsToSelect)); setScorePerQuestion(String(section.scorePerQuestion)); setEditing(false) }} title="Отмена" type="button"><X size={16} /></button></> : <button className="icon-button" disabled={busy} onClick={() => setEditing(true)} title="Редактировать секцию" type="button"><Pencil size={16} /></button>}<button
            className="icon-button text-rose-600"
            disabled={busy}
            onClick={onRemove}
            type="button"
          >
            <Trash2 size={16} />
          </button></div>
        )}
      </div>
      {editable && (
        <div className="mt-4 flex gap-2">
          <select
            className="input"
            onChange={(e) => onSelect(e.target.value)}
            value={selectedQuestion}
          >
            <option value="">Выберите вопрос для добавления</option>
            {available.map((q) => (
              <option key={q.id} value={q.id}>
                {q.text}
              </option>
            ))}
          </select>
          <button
            className="button-secondary"
            disabled={!selectedQuestion || busy}
            onClick={onAdd}
            type="button"
          >
            <Plus size={16} />
          </button>
        </div>
      )}
      <div className="mt-3 space-y-2">
        {section.questionIds.map((id) => {
          const question = questions.find((q) => q.id === id);
          return (
            <div
              className="flex items-center gap-3 rounded-lg bg-slate-50 px-3 py-2 text-sm"
              key={id}
            >
              <span className="min-w-0 flex-1 truncate">
                {question?.text ?? id}
              </span>
              {editable && (
                <button
                  className="text-rose-600"
                  disabled={busy}
                  onClick={() => onRemoveQuestion(id)}
                  type="button"
                >
                  <Trash2 size={15} />
                </button>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

