import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { AttemptDetails } from '@/entities/attempt'
import { attemptsApi } from '@/entities/attempt'
import { AttemptDetailsPage } from './AttemptDetailsPage'

vi.mock('@/entities/attempt', async (importOriginal) => {
  const original = await importOriginal<typeof import('@/entities/attempt')>()
  return { ...original, attemptsApi: { ...original.attemptsApi, getResult: vi.fn() } }
})

const details: AttemptDetails = {
  type: 'test',
  id: 'attempt-id',
  attemptNumber: 2,
  sourceId: 'test-id',
  sourceTitle: 'Release readiness',
  userId: 'user-id',
  employeeNumber: 'EMP-001',
  startedAt: '2026-07-31T10:00:00Z',
  finishedAt: '2026-07-31T10:05:00Z',
  status: 'finished',
  correctAnswers: 1,
  totalQuestions: 2,
  percentage: 50,
  questions: [
    {
      order: 1,
      isCorrect: true,
      earnedScore: 1,
      maxScore: 1,
      question: {
        id: 'question-1',
        text: 'First release question',
        kind: 'choice',
        explanation: null,
        options: [
          { id: 'option-1', text: 'Correct option', isCorrect: true },
          { id: 'option-2', text: 'Wrong option', isCorrect: false },
        ],
      },
      userAnswer: { questionId: 'question-1', selectedOptionIds: ['option-1'] },
    },
    {
      order: 2,
      isCorrect: false,
      earnedScore: 0,
      maxScore: 1,
      question: {
        id: 'question-2',
        text: 'Second release question',
        kind: 'text',
        explanation: 'Expected canonical spelling',
        correctAnswer: 'production',
      },
      userAnswer: { questionId: 'question-2', textAnswer: 'prod' },
    },
  ],
}

describe('AttemptDetailsPage', () => {
  beforeEach(() => {
    vi.mocked(attemptsApi.getResult).mockResolvedValue(details)
  })

  it('renders every question with the user and correct answers', async () => {
    render(
      <MemoryRouter initialEntries={['/attempts/attempt-id']}>
        <Routes>
          <Route path="/attempts/:id" element={<AttemptDetailsPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('First release question')).toBeInTheDocument()
    expect(screen.getByText('Second release question')).toBeInTheDocument()
    expect(screen.getAllByText('Correct option')).toHaveLength(2)
    expect(screen.getByText('prod')).toBeInTheDocument()
    expect(screen.getByText('production')).toBeInTheDocument()
    expect(attemptsApi.getResult).toHaveBeenCalledWith('attempt-id', expect.any(AbortSignal))
  })
})
