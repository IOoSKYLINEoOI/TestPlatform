import { ArrowLeft, Save, Send } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { ConfirmDialog } from "@/shared/ui";
import { ErrorToast } from "@/shared/ui";
import { ExamReadOnlyView } from "@/features/exam-editor";
import { ExamSettingsPanel } from "@/features/exam-editor";
import { ExamDetailsForm } from "@/features/exam-editor";
import { ExamSectionsPanel } from "@/features/exam-editor";
import { useExamDetailsController } from "../model/useExamDetailsController";

export function ExamDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const controller = useExamDetailsController(id);
  const { exam, questions, form, selectedQuestion, setSelectedQuestion, coverUrl, busy, error,
    setError, confirmation, setConfirmation, editable, saveDetails, addSection, addQuestion,
    saveAll, toggleTimeLimit, savePassingRule, updateSection, saveTimeLimit, saveSchedule,
    saveAttemptsLimit, uploadCover, removeCover, publish, confirmRemoveSection,
    confirmRemoveQuestion } = controller;
  const { title, setTitle, description, setDescription, timeLimit, setTimeLimit, hasTimeLimit,
    availableFrom, setAvailableFrom, availableTo, setAvailableTo, attemptsLimit, setAttemptsLimit,
    passingRuleType, setPassingRuleType, passingValue, setPassingValue, reviewPolicy,
    setReviewPolicy, newSection, setNewSection, showValidation } = form;
  if (!exam && !error)
    return (
      <section className="page-shell text-sm text-slate-500">
        Загрузка экзамена…
      </section>
    );
  if (exam && !editable)
    return <ExamReadOnlyView exam={exam} questions={questions} />;
  return (
    <section className="page-shell">
      <ErrorToast message={error} onClose={() => setError(undefined)} />
      <ConfirmDialog
        confirmation={confirmation}
        onClose={() => setConfirmation(undefined)}
      />
      <Link
        className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900"
        to="/exams"
      >
        <ArrowLeft size={17} /> Назад к экзаменам
      </Link>
      {exam && (
        <>
          <div className="mt-5">
            <h1 className="page-title">{exam.title}</h1>
            <p className="page-description">
              Полная настройка экзамена, секций и вопросов.
            </p>
          </div>
          <div className="mt-8 grid gap-6 xl:grid-cols-[1.4fr_1fr]">

            <div className="flex flex-col gap-6">
              <ExamDetailsForm title={title} description={description} editable={Boolean(editable)} busy={busy} showValidation={showValidation} setTitle={setTitle} setDescription={setDescription} saveDetails={saveDetails} />
              <ExamSectionsPanel exam={exam} questions={questions} editable={Boolean(editable)} busy={busy} newSection={newSection} setNewSection={setNewSection} selectedQuestion={selectedQuestion} setSelectedQuestion={setSelectedQuestion} addSection={addSection} addQuestion={addQuestion} onUpdateSection={updateSection} onRemoveSection={confirmRemoveSection} onRemoveQuestion={confirmRemoveQuestion} />
            </div>
            <ExamSettingsPanel exam={exam} editable={Boolean(editable)} busy={busy} coverUrl={coverUrl} passingRuleType={passingRuleType} passingValue={passingValue} reviewPolicy={reviewPolicy} hasTimeLimit={hasTimeLimit} timeLimit={timeLimit} availableFrom={availableFrom} availableTo={availableTo} attemptsLimit={attemptsLimit} setPassingRuleType={setPassingRuleType} setPassingValue={setPassingValue} setReviewPolicy={setReviewPolicy} setTimeLimit={setTimeLimit} setAvailableFrom={setAvailableFrom} setAvailableTo={setAvailableTo} setAttemptsLimit={setAttemptsLimit} onRemoveCover={removeCover} onUploadCover={uploadCover} onSavePassingRule={savePassingRule} onToggleTimeLimit={toggleTimeLimit} onSaveTimeLimit={saveTimeLimit} onSaveSchedule={saveSchedule} onSaveAttempts={saveAttemptsLimit} />
          </div>
          {editable && (
            <div className="mt-8 flex flex-wrap justify-end gap-3">
              <button
                className="button-secondary"
                disabled={busy}
                onClick={() => void saveAll()}
                type="button"
              >
                <Save size={16} /> Сохранить изменения
              </button>
              <button
                className="button-primary"
                disabled={busy}
                onClick={() =>
                  setConfirmation({
                    title: "Опубликовать экзамен?",
                    description:
                      "После публикации экзамен, его секции и вопросы нельзя будет редактировать.",
                    confirmLabel: "Опубликовать",
                    onConfirm: publish,
                  })
                }
                type="button"
              >
                <Send size={16} /> Опубликовать
              </button>
            </div>
          )}
        </>
      )}
    </section>
  );
}


