import type { AttemptSourceType } from '../model/types'

export function normalizeAttemptSourceType(value?: string): AttemptSourceType {
  return value === 'exam' || value === 'exams' ? 'exam' : 'test'
}
