import { useCallback, useEffect, useRef } from 'react'

export function useLatestRequest() {
  const activeRequest = useRef<AbortController>(undefined)
  useEffect(() => () => activeRequest.current?.abort(), [])
  return useCallback(() => {
    activeRequest.current?.abort()
    const controller = new AbortController()
    activeRequest.current = controller
    return controller.signal
  }, [])
}
