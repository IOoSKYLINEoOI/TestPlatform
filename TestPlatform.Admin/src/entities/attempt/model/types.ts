import type { Page } from '@/shared/api/page'

export type AttemptSourceType = 'test' | 'exam'
export type AttemptSource = { id: string; title: string; description: string; type: AttemptSourceType; status: string }
export type AttemptSourcesPage = Page<AttemptSource>
export type AttemptSourcesQuery = { search?: string; type?: AttemptSourceType; page: number; pageSize?: number }
export type AttemptStatus = 'started' | 'finished' | 'expired' | 'abandoned' | 'cancelled' | 'notStarted'

export type AttemptListItem = {
  attemptId: string
  attemptNumber: number
  userId: string
  employeeNumber: string
  status: AttemptStatus
  totalQuestions: number
  answeredQuestions: number
  correctAnswers?: number | null
  earnedPoints?: number | null
  maxPoints?: number
  percentage?: number | null
  passed?: boolean | null
  startedAt: string | null
  finishedAt: string | null
}

export type AttemptsPage = Page<AttemptListItem>
export type AttemptsQuery = { status?: AttemptStatus; passed?: boolean; employeeNumber?: string; page: number; pageSize?: number }

export type AttemptAnswer = {
  type?: 'choice' | 'text' | 'number' | 'matching'
  questionId: string
  selectedOptionIds?: string[]
  textAnswer?: string
  numberAnswer?: number
  matchingPairs?: Array<{ leftOptionId: string; rightOptionId: string }>
}

export type AttemptResultQuestion = {
  order: number
  isCorrect: boolean
  earnedScore: number
  maxScore: number
  question: {
    id: string
    text: string
    kind: 'choice' | 'text' | 'number' | 'matching'
    explanation: string | null
    options?: Array<{ id: string; text: string; isCorrect: boolean }>
    correctAnswer?: string | number
    leftItems?: Array<{ id: string; text: string }>
    rightItems?: Array<{ id: string; text: string }>
    pairs?: Array<{ leftId: string; rightId: string }>
  }
  userAnswer: AttemptAnswer | null
}

export type AttemptDetails = {
  type: AttemptSourceType
  id: string
  attemptNumber: number
  sourceId: string
  sourceTitle: string
  userId: string
  employeeNumber: string
  startedAt: string | null
  finishedAt: string | null
  status: AttemptStatus
  correctAnswers: number
  totalQuestions: number
  percentage: number
  earnedPoints?: number
  totalMaxScore?: number
  passed?: boolean
  questions: AttemptResultQuestion[]
}
