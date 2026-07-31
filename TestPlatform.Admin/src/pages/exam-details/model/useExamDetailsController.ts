import { type FormEvent, useCallback, useEffect, useState } from 'react'
import { examsApi, type ExamSection } from '@/entities/exam'
import { imagesApi } from '@/entities/image'
import { questionsApi } from '@/entities/question'
import { useExamForm } from '@/features/exam-editor'
import { isAbortError } from '@/shared/api/httpClient'
import { useAsyncAction, useLatestRequest } from '@/shared/lib'
import type { Confirmation } from '@/shared/ui'

export function useExamDetailsController(id?: string) {
  const [exam, setExam] = useState<Awaited<ReturnType<typeof examsApi.getExam>>>()
  const [questions, setQuestions] = useState<Awaited<ReturnType<typeof questionsApi.getQuestions>>['items']>([])
  const form = useExamForm()
  const [selectedQuestion, setSelectedQuestion] = useState<Record<string, string>>({})
  const [coverUrl, setCoverUrl] = useState<string>()
  const { busy, error, setError, execute } = useAsyncAction('Не удалось сохранить изменения.')
  const [confirmation, setConfirmation] = useState<Confirmation>()
  const nextRequest = useLatestRequest()

  const load = useCallback(async () => {
    if (!id) return
    const signal = nextRequest()
    try {
      const [details, questionPage] = await Promise.all([
        examsApi.getExam(id, signal),
        questionsApi.getQuestions('published', 1, 100, signal),
      ])
      const nextCoverUrl = details.coverImageId
        ? (await imagesApi.getImageUrl(details.coverImageId, signal)).url
        : undefined
      if (signal.aborted) return
      setExam(details)
      setQuestions(questionPage.items)
      form.reset(details)
      setCoverUrl(nextCoverUrl)
    } catch (cause) {
      if (!isAbortError(cause)) setError(toMessage(cause, 'Не удалось загрузить экзамен.'))
    }
  }, [id, nextRequest, form.reset, setError])

  useEffect(() => { void load() }, [load])

  async function run(action: () => Promise<void>) {
    await execute(async () => { await action(); await load() })
  }
  async function saveDetails(event: FormEvent) {
    event.preventDefault()
    if (id) await run(() => examsApi.updateExam(id, { title: form.title, description: form.description }))
  }
  async function toggleTimeLimit(enabled: boolean) {
    form.setHasTimeLimit(enabled)
    if (!enabled && id) await run(() => examsApi.deleteExamTimeLimit(id))
  }
  async function addSection(event: FormEvent) {
    event.preventDefault()
    if (!id) return
    await run(async () => {
      await examsApi.addExamSection(id, {
        name: form.newSection.name,
        questionsToSelect: Number(form.newSection.questionsToSelect),
        scorePerQuestion: Number(form.newSection.scorePerQuestion),
      })
      form.resetNewSection()
    })
  }
  async function addQuestion(section: ExamSection) {
    const questionId = selectedQuestion[section.id]
    if (!id || !questionId) return
    await run(async () => {
      await examsApi.addExamSectionQuestion(id, section.id, questionId)
      setSelectedQuestion((current) => ({ ...current, [section.id]: '' }))
    })
  }
  async function savePassingRule() {
    if (!id || !form.passingValue) return
    const value = Number(form.passingValue)
    await run(() => examsApi.updateExamPassingRule(id, {
      minScore: form.passingRuleType === 'score' ? value : null,
      minPercent: form.passingRuleType === 'percent' ? value : null,
    }))
  }
  async function saveAll() {
    form.setShowValidation(true)
    if (!id || !form.title.trim() || !form.description.trim() ||
      (form.hasTimeLimit && Number(form.timeLimit) < 2) ||
      (form.availableFrom && form.availableTo && form.availableFrom >= form.availableTo)) return
    await run(async () => {
      await examsApi.updateExam(id, { title: form.title, description: form.description })
      if (form.hasTimeLimit) await examsApi.updateExamTimeLimit(id, Math.round(Number(form.timeLimit) * 60))
      else await examsApi.deleteExamTimeLimit(id)
      if (form.availableFrom || form.availableTo) await examsApi.updateExamSchedule(id, {
        availableFrom: form.availableFrom ? new Date(form.availableFrom).toISOString() : null,
        availableTo: form.availableTo ? new Date(form.availableTo).toISOString() : null,
      })
      else await examsApi.deleteExamSchedule(id)
      await examsApi.updateExamAttemptsLimit(id, Number(form.attemptsLimit))
      await examsApi.updateExamReviewPolicy(id, form.reviewPolicy)
      if (form.passingValue && Number(form.passingValue) > 0) await examsApi.updateExamPassingRule(id, {
        minScore: form.passingRuleType === 'score' ? Number(form.passingValue) : null,
        minPercent: form.passingRuleType === 'percent' ? Number(form.passingValue) : null,
      })
    })
  }

  return {
    exam, questions, form, selectedQuestion, setSelectedQuestion, coverUrl, busy, error, setError,
    confirmation, setConfirmation, editable: exam?.status.toLowerCase() === 'draft', saveDetails,
    addSection, addQuestion, saveAll, toggleTimeLimit, savePassingRule,
    updateSection: (section: ExamSection, input: { name: string; questionsToSelect: number; scorePerQuestion: number }) => id ? run(() => examsApi.updateExamSection(id, section.id, input)) : Promise.resolve(),
    saveTimeLimit: () => id ? run(() => examsApi.updateExamTimeLimit(id, Math.round(Number(form.timeLimit) * 60))) : Promise.resolve(),
    saveSchedule: () => id ? run(() => examsApi.updateExamSchedule(id, { availableFrom: form.availableFrom ? new Date(form.availableFrom).toISOString() : null, availableTo: form.availableTo ? new Date(form.availableTo).toISOString() : null })) : Promise.resolve(),
    saveAttemptsLimit: () => id ? run(() => examsApi.updateExamAttemptsLimit(id, Number(form.attemptsLimit))) : Promise.resolve(),
    uploadCover: (file: File) => id ? run(async () => { const uploaded = await imagesApi.uploadImage(file); await examsApi.updateExamCover(id, uploaded.fileId) }) : Promise.resolve(),
    removeCover: () => id ? run(() => examsApi.deleteExamCover(id)) : Promise.resolve(),
    publish: () => id ? run(() => examsApi.publishExam(id)) : Promise.resolve(),
    confirmRemoveSection: (section: ExamSection) => setConfirmation({ title: 'Удалить секцию?', description: `Секция «${section.name}» и её настройки будут удалены.`, confirmLabel: 'Удалить', danger: true, onConfirm: () => id ? run(() => examsApi.deleteExamSection(id, section.id)) : Promise.resolve() }),
    confirmRemoveQuestion: (section: ExamSection, questionId: string) => setConfirmation({ title: 'Убрать вопрос?', description: 'Вопрос будет исключён только из этой секции.', confirmLabel: 'Убрать', danger: true, onConfirm: () => id ? run(() => examsApi.deleteExamSectionQuestion(id, section.id, questionId)) : Promise.resolve() }),
  }
}

function toMessage(cause: unknown, fallback: string) {
  return cause instanceof Error ? cause.message : fallback
}
