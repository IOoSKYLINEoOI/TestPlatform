import { useCallback, useState } from 'react'

export function useAsyncAction(fallbackMessage: string) {
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string>()

  const execute = useCallback(async (action: () => Promise<void>) => {
    setBusy(true)
    setError(undefined)
    try { await action() }
    catch (cause) { setError(cause instanceof Error ? cause.message : fallbackMessage) }
    finally { setBusy(false) }
  }, [fallbackMessage])

  return { busy, setBusy, error, setError, execute }
}

