import type { Question, QuestionInput } from '@/entities/question'
import type { ChoiceOption, MatchPair } from '@/features/question-editor'

export interface QuestionFormValues {
  kind: Question['kind']
  text: string
  explanation: string
  imageId: string | null
  tagIds: string[]
  choiceMode: 'single' | 'multiple'
  evaluationMode: 'strict' | 'partial'
  options: ChoiceOption[]
  correctAnswer: string
  pairs: MatchPair[]
}

export function buildQuestionInput(values: QuestionFormValues): QuestionInput {
  const common = {
    kind: values.kind,
    text: values.text,
    explanation: values.explanation.trim() || null,
    imageId: values.imageId,
    tagIds: values.tagIds,
  }
  if (values.kind === 'choice') return {
    ...common,
    mode: values.choiceMode,
    evaluationMode: values.choiceMode === 'single' ? 'strict' : values.evaluationMode,
    options: values.options.map(({ text, isCorrect, imageId }) => ({ text, isCorrect, imageId: imageId ?? null })),
  }
  if (values.kind === 'text') return { ...common, correctAnswer: values.correctAnswer }
  if (values.kind === 'number') return { ...common, correctAnswer: Number(values.correctAnswer) }
  return {
    ...common,
    evaluationMode: values.evaluationMode,
    leftItems: values.pairs.map((pair) => ({ id: pair.leftId, text: pair.left, imageId: pair.leftImageId ?? null })),
    rightItems: values.pairs.map((pair) => ({ id: pair.rightId, text: pair.right, imageId: pair.rightImageId ?? null })),
    pairs: values.pairs.map((pair) => ({ leftId: pair.leftId, rightId: pair.rightId })),
  }
}
