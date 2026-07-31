import { ArrowLeft, Save } from "lucide-react";
import { FormEvent, useState } from "react";
import { useNavigate } from "react-router-dom";
import { examsApi } from "@/entities/exam";
import { ErrorToast } from "@/shared/ui";
import { useUnsavedChanges } from "@/shared/lib";

export function ExamCreatePage() {
  const navigate = useNavigate();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [reviewPolicy, setReviewPolicy] = useState<"Immediately" | "AfterExamClosed">("Immediately");
  const [availableFrom, setAvailableFrom] = useState("");
  const [availableTo, setAvailableTo] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string>();
  const [dirty, setDirty] = useState(false);
  useUnsavedChanges(dirty && !saving);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError(undefined);
    try {
      const id = await examsApi.createExam({ title, description });
      await examsApi.updateExamReviewPolicy(id, reviewPolicy);
      if (availableFrom || availableTo) await examsApi.updateExamSchedule(id, { availableFrom: availableFrom ? new Date(availableFrom).toISOString() : null, availableTo: availableTo ? new Date(availableTo).toISOString() : null });
      setDirty(false);
      navigate(`/exams/${id}`);
    } catch (cause) {
      setError(
        cause instanceof Error ? cause.message : "Не удалось создать экзамен.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="page-shell">
      <ErrorToast message={error} onClose={() => setError(undefined)} />
      <button
        className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900"
        onClick={() => navigate("/exams")}
        type="button"
      >
        <ArrowLeft size={17} /> Назад к экзаменам
      </button>
      <form className="card mx-auto mt-5 w-full max-w-3xl" onChangeCapture={() => setDirty(true)} onSubmit={submit}>
        <h1 className="text-xl font-semibold">Новый экзамен</h1>
        <p className="mt-1 text-sm text-slate-500">
          Экзамен будет создан как черновик. После создания настройте секции,
          вопросы и опубликуйте его.
        </p>
        <label className="label mt-6">
          Название
          <input
            autoFocus
            className="input mt-2"
            maxLength={200}
            onChange={(event) => setTitle(event.target.value)}
            required
            value={title}
          />
        </label>
        <label className="label mt-5">
          Описание
          <textarea
            className="input mt-2 min-h-36 resize-y"
            maxLength={2000}
            onChange={(event) => setDescription(event.target.value)}
            required
            value={description}
          />
        </label>
        <div className="mt-6 rounded-xl border border-slate-200 p-4">
          <h2 className="font-semibold">Период доступности</h2>
          <p className="mt-1 text-sm text-slate-500">Оставьте поля пустыми, если экзамен доступен без ограничения по датам.</p>
          <div className="mt-4 grid gap-4 sm:grid-cols-2"><label className="label">Доступен с<input className="input mt-2" onChange={(event) => setAvailableFrom(event.target.value)} type="datetime-local" value={availableFrom} /></label><label className="label">Доступен до<input className="input mt-2" min={availableFrom || undefined} onChange={(event) => setAvailableTo(event.target.value)} required={reviewPolicy === "AfterExamClosed"} type="datetime-local" value={availableTo} /></label></div>
        </div>
        <div className="mt-4 rounded-xl border border-slate-200 p-4">
          <h2 className="font-semibold">Параметры прохождения</h2>
          <label className="label mt-4">Политика просмотра<select className="input mt-2" onChange={(event) => setReviewPolicy(event.target.value as "Immediately" | "AfterExamClosed")} value={reviewPolicy}><option value="Immediately">Сразу после завершения попытки</option><option value="AfterExamClosed">После окончания экзамена</option></select></label>
          {reviewPolicy === "AfterExamClosed" && <label className="label mt-4">Дата и время окончания<input className="input mt-2" min={new Date().toISOString().slice(0, 16)} onChange={(event) => setAvailableTo(event.target.value)} required type="datetime-local" value={availableTo} /></label>}
        </div>
        <div className="mt-7 flex justify-end gap-3">
          <button
            className="button-secondary"
            onClick={() => navigate("/exams")}
            type="button"
          >
            Отмена
          </button>
          <button className="button-primary" disabled={saving} type="submit">
            <Save size={16} /> {saving ? "Создание…" : "Создать экзамен"}
          </button>
        </div>
      </form>
    </section>
  );
}

