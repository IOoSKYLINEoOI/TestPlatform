import { ArrowLeft, Send } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { ConfirmDialog, ErrorToast } from '@/shared/ui'
import { TestDetailsForm, TestQuestionsPanel, TestReadOnlyView, TestSettingsPanel } from '@/features/test-editor'
import { useTestDetailsController } from '../model/useTestDetailsController'

export function TestDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const controller = useTestDetailsController(id)
  const { test, questions, form, availableQuestions, selectedQuestionId, setSelectedQuestionId,
    coverUrl, expandedQuestionId, questionDetails, questionImageUrls, optionImageUrls,
    loadingQuestionId, busy, error, setError, confirmation, setConfirmation, editable,
    saveDetails, toggleTimeLimit, addQuestion, toggleQuestion, moveQuestion, saveTimeLimit,
    uploadCover, removeCover, removeQuestion, publish } = controller
  const { title, setTitle, description, setDescription, timeLimitMinutes, setTimeLimitMinutes,
    hasTimeLimit } = form

  if (!test && !error) return <section className="page-shell text-sm text-slate-500">Загружаем тест…</section>
  if (test && !editable) return <TestReadOnlyView questions={questions} test={test} />

  return <section className="page-shell">
    <ErrorToast message={error} onClose={() => setError(undefined)} />
    <ConfirmDialog confirmation={confirmation} onClose={() => setConfirmation(undefined)} />
    <Link className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900" to="/tests">
      <ArrowLeft size={17} /> Назад к тестам
    </Link>
    {test && <>
      <div className="mt-5 flex flex-wrap items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-3"><h1 className="page-title">{test.title}</h1><span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-medium">{statusLabel(test.status)}</span></div>
          <p className="page-description">Полная настройка теста и его вопросов.</p>
        </div>
        {editable && <button className="button-primary" disabled={busy} onClick={() => setConfirmation({ title: 'Опубликовать тест?', description: 'После публикации тест и его состав нельзя будет редактировать.', confirmLabel: 'Опубликовать', onConfirm: publish })} type="button"><Send size={16} /> Опубликовать</button>}
      </div>
      <div className="mt-8 grid gap-6 xl:grid-cols-[1.4fr_1fr]">
        <div className="space-y-6">
          <TestDetailsForm title={title} description={description} editable={editable} busy={busy} setTitle={setTitle} setDescription={setDescription} saveDetails={saveDetails} />
          <TestQuestionsPanel test={test} questions={questions} availableQuestions={availableQuestions} selectedQuestionId={selectedQuestionId} setSelectedQuestionId={setSelectedQuestionId} editable={editable} busy={busy} expandedQuestionId={expandedQuestionId} loadingQuestionId={loadingQuestionId} questionDetails={questionDetails} questionImageUrls={questionImageUrls} optionImageUrls={optionImageUrls} addQuestion={addQuestion} removeQuestion={removeQuestion} toggleQuestion={toggleQuestion} moveQuestion={moveQuestion} />
        </div>
        <TestSettingsPanel test={test} editable={editable} busy={busy} hasTimeLimit={hasTimeLimit} timeLimitMinutes={timeLimitMinutes} coverUrl={coverUrl} onToggleTimeLimit={toggleTimeLimit} onTimeLimitChange={setTimeLimitMinutes} onSaveTimeLimit={saveTimeLimit} onUploadCover={uploadCover} onRemoveCover={removeCover} />
      </div>
    </>}
  </section>
}

function statusLabel(status: string) {
  return ({ draft: 'Черновик', published: 'Опубликован', archived: 'В архиве' } as Record<string, string>)[status.toLowerCase()] ?? status
}
