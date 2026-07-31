import { describe, expect, it } from 'vitest'
import { normalizeAttemptSourceType } from './normalizeAttemptSourceType'

describe('normalizeAttemptSourceType', () => {
  it.each([['exam', 'exam'], ['exams', 'exam'], ['test', 'test'], ['tests', 'test']])(
    'maps route segment %s to %s',
    (segment, expected) => expect(normalizeAttemptSourceType(segment)).toBe(expected),
  )
})
