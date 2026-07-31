import { type FormEvent, useEffect, useState } from 'react'
import { imagesApi } from '@/entities/image'
import { questionsApi, type Question } from '@/entities/question'
import { tagsApi, type Tag } from '@/entities/tag'
import { newPair, type ChoiceOption, type MatchPair } from '@/features/question-editor'
import { isAbortError } from '@/shared/api/httpClient'
import { useUnsavedChanges } from '@/shared/lib'
import type { Confirmation } from '@/shared/ui'
import { buildQuestionInput } from './buildQuestionInput'

export function useQuestionEditorController(questionId: string | undefined, onSaved: () => Promise<void>) {
  const [kind, setKind] = useState<Question['kind']>('choice')
  const [text, setText] = useState('')
  const [explanation, setExplanation] = useState('')
  const [tags, setTags] = useState<Tag[]>([])
  const [tagIds, setTagIds] = useState<string[]>([])
  const [newTagName, setNewTagName] = useState('')
  const [newTagDescription, setNewTagDescription] = useState('')
  const [creatingTag, setCreatingTag] = useState(false)
  const [imageId, setImageId] = useState<string | null>(null)
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(null)
  const [uploadingImage, setUploadingImage] = useState(false)
  const [choiceMode, setChoiceMode] = useState<'single' | 'multiple'>('single')
  const [evaluationMode, setEvaluationMode] = useState<'strict' | 'partial'>('strict')
  const [options, setOptions] = useState<ChoiceOption[]>([{ text: '', isCorrect: true }, { text: '', isCorrect: false }])
  const [correctAnswer, setCorrectAnswer] = useState('')
  const [pairs, setPairs] = useState<MatchPair[]>([newPair(), newPair()])
  const [saving, setSaving] = useState(false)
  const [publishing, setPublishing] = useState(false)
  const [loadingQuestion, setLoadingQuestion] = useState(Boolean(questionId))
  const [readonly, setReadonly] = useState(false)
  const [error, setError] = useState<string>()
  const [confirmation, setConfirmation] = useState<Confirmation>()
  const [dirty, setDirty] = useState(false)
  useUnsavedChanges(dirty && !saving && !publishing && !readonly)

  useEffect(() => {
    const controller = new AbortController()
    tagsApi.getTags('', 1, 100, controller.signal).then((result) => setTags(result.items)).catch((cause) => { if (!isAbortError(cause)) setTags([]) })
    if (!questionId) return () => controller.abort()
    questionsApi.getQuestion(questionId, controller.signal).then(async (question) => {
      setKind(question.kind); setReadonly(question.status.toLowerCase() !== 'draft'); setText(question.text)
      setExplanation(question.explanation ?? ''); setTagIds(question.tags.map((tag) => tag.id)); setImageId(question.imageId)
      if (question.imageId) setImagePreviewUrl((await imagesApi.getImageUrl(question.imageId, controller.signal)).url)
      if (question.kind === 'choice') {
        setChoiceMode((question.mode?.toLowerCase() ?? 'single') as 'single' | 'multiple')
        setEvaluationMode((question.evaluationMode?.toLowerCase() ?? 'strict') as 'strict' | 'partial')
        const editorOptions = question.options?.map((option) => ({ text: option.text, isCorrect: option.isCorrect, imageId: option.imageId })) ?? []
        setOptions(editorOptions)
        void Promise.all(editorOptions.map(async (option, index) => {
          if (!option.imageId) return
          const image = await imagesApi.getImageUrl(option.imageId, controller.signal)
          setOptions((current) => current.map((item, itemIndex) => itemIndex === index ? { ...item, previewUrl: image.url } : item))
        }))
      } else if (question.kind === 'text' || question.kind === 'number') setCorrectAnswer(String(question.correctAnswer ?? ''))
      else {
        setEvaluationMode((question.evaluationMode?.toLowerCase() ?? 'strict') as 'strict' | 'partial')
        setPairs((question.pairs ?? []).map((pair) => {
          const left = question.leftItems?.find((item) => item.id === pair.leftId)
          const right = question.rightItems?.find((item) => item.id === pair.rightId)
          return { leftId: pair.leftId, left: left?.text ?? '', leftImageId: left?.imageId, rightId: pair.rightId, right: right?.text ?? '', rightImageId: right?.imageId }
        }))
      }
    }).catch((cause) => { if (!isAbortError(cause)) setError(toMessage(cause, 'Не удалось загрузить вопрос.')) })
      .finally(() => { if (!controller.signal.aborted) setLoadingQuestion(false) })
    return () => controller.abort()
  }, [questionId])

  function changeChoiceMode(value: 'single' | 'multiple') {
    setChoiceMode(value)
    if (value === 'single') { setEvaluationMode('strict'); setOptions((current) => current.map((option, index) => ({ ...option, isCorrect: index === 0 }))) }
  }
  function setCorrect(index: number, checked: boolean) {
    setOptions((current) => current.map((option, optionIndex) => ({ ...option, isCorrect: choiceMode === 'single' ? optionIndex === index : optionIndex === index ? checked : option.isCorrect })))
  }
  async function createTag() {
    if (!newTagName.trim() || !newTagDescription.trim()) return
    setCreatingTag(true); setError(undefined)
    try {
      const result = await tagsApi.createTag({ name: newTagName.trim(), description: newTagDescription.trim() })
      const tag = { id: result.id, name: newTagName.trim(), description: newTagDescription.trim() }
      setTags((current) => [...current, tag]); setTagIds((current) => [...current, tag.id]); setNewTagName(''); setNewTagDescription('')
    } catch (cause) { setError(toMessage(cause, 'Не удалось создать тег.')) }
    finally { setCreatingTag(false) }
  }
  async function uploadImage(file: File, optionIndex?: number) {
    setUploadingImage(true); setError(undefined)
    try {
      const uploaded = await imagesApi.uploadImage(file)
      if (optionIndex === undefined) { setImageId(uploaded.fileId); setImagePreviewUrl(uploaded.previewUrl) }
      else setOptions((current) => current.map((option, index) => index === optionIndex ? { ...option, imageId: uploaded.fileId, previewUrl: uploaded.previewUrl } : option))
    } catch (cause) { setError(toMessage(cause, 'Не удалось загрузить изображение.')) }
    finally { setUploadingImage(false) }
  }
  async function submit(event: FormEvent) {
    event.preventDefault(); setSaving(true); setError(undefined)
    try {
      const input = buildQuestionInput({ kind, text, explanation, imageId, tagIds, choiceMode, evaluationMode, options, correctAnswer, pairs })
      if (questionId) await questionsApi.updateQuestion(questionId, input); else await questionsApi.createQuestion(input)
      setDirty(false); await onSaved()
    } catch (cause) { setError(toMessage(cause, questionId ? 'Не удалось сохранить вопрос.' : 'Не удалось создать вопрос.')) }
    finally { setSaving(false) }
  }
  async function publish() {
    if (!questionId) return
    setPublishing(true); setError(undefined)
    try { await questionsApi.publishQuestion(questionId); setDirty(false); setReadonly(true) }
    catch (cause) { setError(toMessage(cause, 'Не удалось опубликовать вопрос.')) }
    finally { setPublishing(false) }
  }

  return { kind, setKind, text, setText, explanation, setExplanation, tags, tagIds, setTagIds,
    newTagName, setNewTagName, newTagDescription, setNewTagDescription, creatingTag, imageId, setImageId,
    imagePreviewUrl, setImagePreviewUrl, uploadingImage, choiceMode, evaluationMode, setEvaluationMode,
    options, setOptions, correctAnswer, setCorrectAnswer, pairs, setPairs, saving, publishing,
    loadingQuestion, readonly, error, setError, confirmation, setConfirmation, setDirty,
    changeChoiceMode, setCorrect, createTag, uploadImage, submit, publish }
}

function toMessage(cause: unknown, fallback: string) { return cause instanceof Error ? cause.message : fallback }
