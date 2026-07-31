import { ArrowLeft, History } from "lucide-react";
import { Link } from "react-router-dom";
import type { ExamDetails } from '@/entities/exam'
import type { Question } from '@/entities/question'
import { ReadOnlyQuestionList } from "@/entities/question";

export function ExamReadOnlyView({
  exam,
  questions,
}: {
  exam: ExamDetails;
  questions: Question[];
}) {
  return (
    <section className="page-shell">
      <Link
        className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900"
        to="/exams"
      >
        <ArrowLeft size={17} /> Назад к экзаменам
      </Link>
      <div className="mt-5 flex flex-wrap items-center justify-between gap-4">
        <h1 className="page-title">{exam.title}</h1>
        <Link className="button-secondary" to={`/exams/${exam.id}/attempts`}><History size={17} /> Попытки</Link>
      </div>
      <div className="mt-8 grid gap-6 xl:grid-cols-[1.4fr_1fr]">
        <div className="flex flex-col gap-6">
          <div className="card">
            <h2 className="text-lg font-semibold">Основная информация</h2>
            <dl className="mt-5 divide-y divide-slate-100 text-sm">
              <Row label="Название" value={exam.title} />
              <Row label="Описание" value={exam.description || "—"} />
            </dl>
          </div>
          <div className="card">
            <h2 className="text-lg font-semibold">Секции экзамена</h2>
            <div className="mt-5 space-y-5">
              {exam.sections.map((section) => (
                <div
                  className="rounded-xl border border-slate-200 p-4"
                  key={section.id}
                >
                  <h3 className="font-semibold">{section.name}</h3>
                  <p className="mt-1 text-sm text-slate-500">
                    Выбрано: {section.questionsToSelect} ·{" "}
                    {section.scorePerQuestion} балл(а) · максимум{" "}
                    {section.maxScore}
                  </p>
                  <ReadOnlyQuestionList
                    ids={section.questionIds}
                    questions={questions}
                  />
                </div>
              ))}
            </div>
          </div>
        </div>
        <div className="card">
          <h2 className="text-lg font-semibold">Параметры</h2>
          <dl className="mt-4 divide-y divide-slate-100 text-sm">
            <Row label="Статус" value={exam.status} />
            <Row label="Вопросов" value={String(exam.totalQuestions)} />
            <Row label="Макс. балл" value={String(exam.totalMaxScore)} />
            <Row label="Попыток" value={String(exam.attemptsLimit)} />
            <Row
              label="Доступен с"
              value={
                exam.schedule?.availableFrom
                  ? new Intl.DateTimeFormat("ru-RU", {
                      dateStyle: "medium",
                      timeStyle: "short",
                    }).format(new Date(exam.schedule.availableFrom))
                  : "Без ограничения"
              }
            />
            <Row
              label="Доступен до"
              value={
                exam.schedule?.availableTo
                  ? new Intl.DateTimeFormat("ru-RU", {
                      dateStyle: "medium",
                      timeStyle: "short",
                    }).format(new Date(exam.schedule.availableTo))
                  : "Без ограничения"
              }
            />
            <Row
              label="Время"
              value={
                exam.timeLimitSeconds
                  ? `${Math.round(exam.timeLimitSeconds / 60)} мин.`
                  : "Без ограничения"
              }
            />
          </dl>
        </div>
      </div>
    </section>
  );
}

function Row({ label, value }: { label: string; value: string }) { return <div className="flex justify-between gap-4 py-3"><dt className="text-slate-500">{label}</dt><dd className="text-right font-medium">{value}</dd></div>; }

