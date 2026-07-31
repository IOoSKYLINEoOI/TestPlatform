import type { DragEndEvent } from '@dnd-kit/core'
import { arrayMove } from '@dnd-kit/sortable'
import { type FormEvent, useCallback, useEffect, useMemo, useState } from 'react'
import { imagesApi } from '@/entities/image'
import { questionsApi, type Question, type QuestionEditor } from '@/entities/question'
import { testsApi, type TestDetails } from '@/entities/test'
import { useTestForm } from '@/features/test-editor'
import { isAbortError } from '@/shared/api/httpClient'
import { useAsyncAction, useLatestRequest } from '@/shared/lib'
import type { Confirmation } from '@/shared/ui'

export function useTestDetailsController(id?: string) {
  const [test, setTest] = useState<TestDetails>()
  const [questions, setQuestions] = useState<Question[]>([])
  const form = useTestForm()
  const [selectedQuestionId, setSelectedQuestionId] = useState('')
  const [coverUrl, setCoverUrl] = useState<string>()
  const [expandedQuestionId, setExpandedQuestionId] = useState<string>()
  const [questionDetails, setQuestionDetails] = useState<Record<string, QuestionEditor>>({})
  const [questionImageUrls, setQuestionImageUrls] = useState<Record<string, string>>({})
  const [optionImageUrls, setOptionImageUrls] = useState<Record<string, string>>({})
  const [loadingQuestionId, setLoadingQuestionId] = useState<string>()
  const { busy, setBusy, error, setError, execute } = useAsyncAction('Не удалось сохранить изменения.')
  const [confirmation, setConfirmation] = useState<Confirmation>()
  const nextRequest = useLatestRequest()

  const load = useCallback(async () => {
    if (!id) return
    const signal = nextRequest()
    try {
      setError(undefined)
      const [details, bank] = await Promise.all([
        testsApi.getTest(id, signal),
        questionsApi.getQuestions('published', 1, 100, signal),
      ])
      const attached = (await Promise.all(details.questions.map((item) =>
        questionsApi.getQuestion(item.questionId, signal).catch(() => undefined),
      ))).filter((question): question is QuestionEditor => question !== undefined)
      const detailsById = Object.fromEntries(attached.map((question) => [question.id, question]))
      const questionsById = new Map(bank.items.map((question) => [question.id, question]))
      attached.forEach((question) => questionsById.set(question.id, toQuestion(question)))
      const nextCoverUrl = details.coverImageId
        ? (await imagesApi.getImageUrl(details.coverImageId, signal)).url
        : undefined
      if (signal.aborted) return
      setTest(details)
      setQuestions([...questionsById.values()])
      setQuestionDetails(detailsById)
      form.reset(details)
      setCoverUrl(nextCoverUrl)
    } catch (cause) {
      if (!isAbortError(cause)) setError(toMessage(cause, 'Не удалось загрузить тест.'))
    }
  }, [id, nextRequest, form.reset, setError])
  useEffect(() => { void load() }, [load])

  const attachedIds = useMemo(() => new Set(test?.questions.map((item) => item.questionId) ?? []), [test])
  const availableQuestions = questions.filter((question) => !attachedIds.has(question.id))

  async function run(action: () => Promise<void>) { await execute(action) }
  async function saveDetails(event: FormEvent) {
    event.preventDefault()
    if (id) await run(async () => { await testsApi.updateTest(id, { title: form.title, description: form.description }); await load() })
  }
  async function toggleTimeLimit(enabled: boolean) {
    form.setHasTimeLimit(enabled)
    if (!enabled && id && test?.timeLimitSeconds != null) await run(async () => { await testsApi.deleteTestTimeLimit(id); await load() })
  }
  async function addQuestion() {
    if (!id || !selectedQuestionId) return
    await run(async () => { await testsApi.addTestQuestion(id, selectedQuestionId); setSelectedQuestionId(''); await load() })
  }
  async function toggleQuestion(questionId: string) {
    if (expandedQuestionId === questionId) { setExpandedQuestionId(undefined); return }
    setExpandedQuestionId(questionId)
    const cached = questionDetails[questionId]
    if (cached) { await loadQuestionImages(questionId, cached); return }
    setLoadingQuestionId(questionId)
    setError(undefined)
    try {
      const details = await questionsApi.getQuestion(questionId)
      setQuestionDetails((current) => ({ ...current, [questionId]: details }))
      await loadQuestionImages(questionId, details)
    } catch (cause) {
      setExpandedQuestionId(undefined)
      setError(toMessage(cause, 'Не удалось загрузить вопрос.'))
    } finally { setLoadingQuestionId(undefined) }
  }
  async function loadQuestionImages(questionId: string, details: QuestionEditor) {
    const requests: Promise<void>[] = []
    if (details.imageId && !questionImageUrls[questionId]) requests.push(imagesApi.getImageUrl(details.imageId).then((image) => setQuestionImageUrls((current) => ({ ...current, [questionId]: image.url }))))
    for (const option of details.options ?? []) if (option.imageId && !optionImageUrls[option.imageId]) requests.push(imagesApi.getImageUrl(option.imageId).then((image) => setOptionImageUrls((current) => ({ ...current, [option.imageId!]: image.url }))))
    await Promise.all(requests)
  }
  async function moveQuestion(event: DragEndEvent) {
    if (!id || !test || !event.over || event.active.id === event.over.id) return
    const previous = test.questions
    const fromIndex = previous.findIndex((item) => item.questionId === event.active.id)
    const toIndex = previous.findIndex((item) => item.questionId === event.over?.id)
    if (fromIndex < 0 || toIndex < 0) return
    const reordered = arrayMove(previous, fromIndex, toIndex)
    setTest({ ...test, questions: reordered })
    setBusy(true); setError(undefined)
    try { await testsApi.reorderTestQuestions(id, reordered.map((item) => item.questionId)) }
    catch (cause) { setTest((current) => current ? { ...current, questions: previous } : current); setError(toMessage(cause, 'Не удалось изменить порядок вопросов.')) }
    finally { setBusy(false) }
  }

  return {
    test, questions, form, availableQuestions, selectedQuestionId, setSelectedQuestionId, coverUrl,
    expandedQuestionId, questionDetails, questionImageUrls, optionImageUrls, loadingQuestionId,
    busy, error, setError, confirmation, setConfirmation, editable: test?.status.toLowerCase() === 'draft',
    saveDetails, toggleTimeLimit, addQuestion, toggleQuestion, moveQuestion,
    saveTimeLimit: () => id ? run(async () => { await testsApi.updateTestTimeLimit(id, Math.round(Number(form.timeLimitMinutes) * 60)); await load() }) : Promise.resolve(),
    uploadCover: (file: File) => id ? run(async () => { const uploaded = await imagesApi.uploadImage(file); await testsApi.updateTestCover(id, uploaded.fileId); await load() }) : Promise.resolve(),
    removeCover: () => id ? run(async () => { await testsApi.deleteTestCover(id); await load() }) : Promise.resolve(),
    removeQuestion: (questionId: string) => id ? run(async () => { await testsApi.deleteTestQuestion(id, questionId); await load() }) : Promise.resolve(),
    publish: () => id ? run(async () => { await testsApi.publishTest(id); await load() }) : Promise.resolve(),
  }
}

function toQuestion(question: QuestionEditor): Question {
  return { id: question.id, text: question.text, imageId: question.imageId, kind: question.kind, type: ({ choice: 'Choice', text: 'Text', number: 'Number', matching: 'Matching' } as const)[question.kind], status: question.status, tags: question.tags }
}

function toMessage(cause: unknown, fallback: string) { return cause instanceof Error ? cause.message : fallback }
