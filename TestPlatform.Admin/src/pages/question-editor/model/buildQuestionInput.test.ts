import { describe, expect, it } from 'vitest'
import { buildQuestionInput, type QuestionFormValues } from './buildQuestionInput'

function values(overrides: Partial<QuestionFormValues> = {}): QuestionFormValues {
  return {
    kind: 'choice',
    text: 'Вопрос',
    explanation: '  Пояснение  ',
    imageId: null,
    tagIds: ['tag-1'],
    choiceMode: 'single',
    evaluationMode: 'partial',
    options: [{ text: 'Да', isCorrect: true }, { text: 'Нет', isCorrect: false }],
    correctAnswer: '',
    pairs: [],
    ...overrides,
  }
}

describe('buildQuestionInput', () => {
  it('forces strict evaluation for a single-choice question', () => {
    expect(buildQuestionInput(values())).toMatchObject({
      kind: 'choice',
      explanation: 'Пояснение',
      mode: 'single',
      evaluationMode: 'strict',
    })
  })

  it('converts a numeric answer to number', () => {
    expect(buildQuestionInput(values({ kind: 'number', correctAnswer: '12.5' }))).toMatchObject({
      kind: 'number',
      correctAnswer: 12.5,
    })
  })

  it('maps matching pairs to backend collections', () => {
    const input = buildQuestionInput(values({
      kind: 'matching',
      pairs: [{ leftId: 'left-1', left: 'A', rightId: 'right-1', right: 'B' }],
    }))
    expect(input).toMatchObject({
      leftItems: [{ id: 'left-1', text: 'A', imageId: null }],
      rightItems: [{ id: 'right-1', text: 'B', imageId: null }],
      pairs: [{ leftId: 'left-1', rightId: 'right-1' }],
    })
  })

  it('normalizes an empty explanation to null', () => {
    expect(buildQuestionInput(values({ kind: 'text', explanation: '   ', correctAnswer: 'Ответ' }))).toMatchObject({
      explanation: null,
      correctAnswer: 'Ответ',
    })
  })
})
