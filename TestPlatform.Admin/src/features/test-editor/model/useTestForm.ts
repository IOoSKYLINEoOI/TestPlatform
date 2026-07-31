import { useCallback, useState } from 'react'
import type { TestDetails } from '@/entities/test'

export function useTestForm() {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [timeLimitMinutes, setTimeLimitMinutes] = useState('')
  const [hasTimeLimit, setHasTimeLimit] = useState(false)
  const reset = useCallback((test: TestDetails) => {
    setTitle(test.title); setDescription(test.description)
    setTimeLimitMinutes(test.timeLimitSeconds ? String(test.timeLimitSeconds / 60) : '')
    setHasTimeLimit(test.timeLimitSeconds != null)
  }, [])
  return { title, setTitle, description, setDescription, timeLimitMinutes, setTimeLimitMinutes, hasTimeLimit, setHasTimeLimit, reset }
}


