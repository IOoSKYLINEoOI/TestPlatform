import { useCallback, useState } from 'react'
import type { ExamDetails } from '@/entities/exam'

export type ExamSectionInput = { name: string; questionsToSelect: string; scorePerQuestion: string }
const createEmptySection = (): ExamSectionInput => ({ name: '', questionsToSelect: '1', scorePerQuestion: '1' })

export function useExamForm() {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [timeLimit, setTimeLimit] = useState('')
  const [hasTimeLimit, setHasTimeLimit] = useState(false)
  const [availableFrom, setAvailableFrom] = useState('')
  const [availableTo, setAvailableTo] = useState('')
  const [attemptsLimit, setAttemptsLimit] = useState('1')
  const [passingRuleType, setPassingRuleType] = useState<'score' | 'percent'>('score')
  const [passingValue, setPassingValue] = useState('')
  const [reviewPolicy, setReviewPolicy] = useState<'Immediately' | 'AfterExamClosed'>('Immediately')
  const [newSection, setNewSection] = useState<ExamSectionInput>(createEmptySection)
  const [showValidation, setShowValidation] = useState(false)

  const reset = useCallback((exam: ExamDetails) => {
    setTitle(exam.title); setDescription(exam.description)
    setHasTimeLimit(exam.timeLimitSeconds != null)
    setTimeLimit(exam.timeLimitSeconds ? String(exam.timeLimitSeconds / 60) : '')
    setAvailableFrom(exam.schedule?.availableFrom?.slice(0, 16) ?? '')
    setAvailableTo(exam.schedule?.availableTo?.slice(0, 16) ?? '')
    setAttemptsLimit(String(exam.attemptsLimit))
    setPassingRuleType(exam.passingRule?.minPercent != null ? 'percent' : 'score')
    setPassingValue(String(exam.passingRule?.minPercent ?? exam.passingRule?.minScore ?? ''))
    setReviewPolicy(exam.reviewPolicy)
  }, [])
  const resetNewSection = useCallback(() => setNewSection(createEmptySection()), [])

  return { title, setTitle, description, setDescription, timeLimit, setTimeLimit, hasTimeLimit, setHasTimeLimit, availableFrom, setAvailableFrom, availableTo, setAvailableTo, attemptsLimit, setAttemptsLimit, passingRuleType, setPassingRuleType, passingValue, setPassingValue, reviewPolicy, setReviewPolicy, newSection, setNewSection, showValidation, setShowValidation, reset, resetNewSection }
}

