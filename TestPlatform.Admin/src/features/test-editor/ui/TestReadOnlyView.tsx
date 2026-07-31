import { ArrowLeft, History } from "lucide-react";
import { Link } from "react-router-dom";
import type { Question } from '@/entities/question'
import type { TestDetails } from '@/entities/test'
import { ReadOnlyQuestionList } from "@/entities/question";

export function TestReadOnlyView({
  test,
  questions,
}: {
  test: TestDetails;
  questions: Question[];
}) {
  return (
    <section className="page-shell">
      <Link
        className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900"
        to="/tests"
      >
        <ArrowLeft size={17} /> Назад к тестам
      </Link>
      <div className="mt-5 flex flex-wrap items-center justify-between gap-4">
        <h1 className="page-title">{test.title}</h1>
        <Link className="button-secondary" to={`/tests/${test.id}/attempts`}><History size={17} /> Попытки</Link>
      </div>
      <div className="mt-8 grid gap-6 xl:grid-cols-[1.4fr_1fr]">
        <div className="space-y-6">
          <div className="card">
            <h2 className="text-lg font-semibold">Основная информация</h2>
            <dl className="mt-5 divide-y divide-slate-100 text-sm">
              <InfoRow label="Название" value={test.title} />
              <InfoRow label="Описание" value={test.description || "—"} />
            </dl>
          </div>
          <div className="card">
            <h2 className="text-lg font-semibold">Вопросы</h2>
            <p className="mt-1 text-sm text-slate-500">
              Нажмите на вопрос, чтобы посмотреть подробности.
            </p>
            <ReadOnlyQuestionList
              ids={test.questions.map((item) => item.questionId)}
              questions={questions}
            />
          </div>
        </div>
        <div className="space-y-6">
        <div className="card">
          <h2 className="text-lg font-semibold">Параметры</h2>
          <dl className="mt-5 divide-y divide-slate-100 text-sm">
            <InfoRow label="Статус" value={statusLabel(test.status)} />
            <InfoRow label="Вопросов" value={String(test.questions.length)} />
            <InfoRow
              label="Время"
              value={
                test.timeLimitSeconds
                  ? formatDuration(test.timeLimitSeconds)
                  : "Без ограничения"
              }
            />
            <InfoRow label="Создан" value={formatDate(test.createdAt)} />
            <InfoRow
              label="Опубликован"
              value={test.publishedAt ? formatDate(test.publishedAt) : "а"}
            />
          </dl>
        </div>
        <div className="card"><h2 className="text-lg font-semibold">Ограничения</h2><dl className="mt-5 divide-y divide-slate-100 text-sm"><InfoRow label="Время на прохождение" value={test.timeLimitSeconds ? formatDuration(test.timeLimitSeconds) : "Без ограничения"} /></dl></div>
        </div>
      </div>
    </section>
  );
}


function InfoRow({ label, value }: { label: string; value: string }) { return <div className="grid grid-cols-[7rem_1fr] gap-3 py-3 first:pt-0 last:pb-0"><dt className="text-slate-500">{label}</dt><dd className="min-w-0 text-right font-medium">{value}</dd></div>; }
function formatDate(value: string) { return new Intl.DateTimeFormat("ru-RU", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value)); }
function formatDuration(seconds: number) { return `${Math.round(seconds / 60)} мин.`; }
function statusLabel(status: string) { return ({ draft: "Черновик", published: "Опубликован", archived: "В архиве" } as Record<string, string>)[status.toLowerCase()] ?? status; }
