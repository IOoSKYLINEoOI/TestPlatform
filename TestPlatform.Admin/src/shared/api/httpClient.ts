import { keycloak } from '@/shared/auth/keycloak'

type ProblemDetails = { title?: string; detail?: string; code?: string; errors?: Record<string, string[]> }
export type RequestOptions = RequestInit & { timeoutMs?: number }

export class ApiError extends Error {
  constructor(public readonly status: number, public readonly code: string, message: string) {
    super(message)
    this.name = 'ApiError'
  }
}

let tokenRefresh: Promise<boolean> | undefined

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { signal, cleanup } = requestSignal(options.signal, options.timeoutMs ?? 15_000)
  try {
    await refreshToken(30)
    let response = await send(path, options, signal)
    if (response.status === 401) {
      try {
        await refreshToken(-1)
        response = await send(path, options, signal)
        if (response.status === 401) {
          await keycloak.login({ redirectUri: window.location.href })
          throw new ApiError(401, 'unauthorized', 'Сессия истекла. Выполняется повторный вход.')
        }
      } catch (cause) {
        if (cause instanceof ApiError) throw cause
        await keycloak.login({ redirectUri: window.location.href })
        throw new ApiError(401, 'unauthorized', 'Сессия истекла. Выполняется повторный вход.')
      }
    }
    if (!response.ok) throw await responseError(response)
    if (response.status === 204) return undefined as T
    const body = await response.text()
    return body.trim() ? JSON.parse(body) as T : undefined as T
  } catch (cause) {
    if (cause instanceof DOMException && cause.name === 'AbortError') {
      if (options.signal?.aborted) throw cause
      throw new ApiError(0, 'request.timeout', 'Сервер не ответил вовремя. Повторите попытку.')
    }
    throw cause
  } finally {
    cleanup()
  }
}

async function send(path: string, options: RequestOptions, signal: AbortSignal) {
  const headers = new Headers(options.headers)
  headers.set('Authorization', `Bearer ${keycloak.token}`)
  if (options.body != null && !(options.body instanceof FormData) && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json')
  return fetch(`/api${path}`, { ...options, headers, signal })
}

function refreshToken(minValidity: number) {
  tokenRefresh ??= keycloak.updateToken(minValidity).finally(() => { tokenRefresh = undefined })
  return tokenRefresh
}

function requestSignal(source: AbortSignal | null | undefined, timeoutMs: number) {
  const controller = new AbortController()
  const abort = () => controller.abort()
  source?.addEventListener('abort', abort, { once: true })
  const timeout = window.setTimeout(abort, timeoutMs)
  return {
    signal: controller.signal,
    cleanup: () => { window.clearTimeout(timeout); source?.removeEventListener('abort', abort) },
  }
}

export async function responseError(response: Response) {
  const problem = await response.json().catch(() => ({})) as ProblemDetails
  const validation = problem.errors ? Object.values(problem.errors).flat().join(' ') : undefined
  const code = problem.code || statusCode(response.status)
  const message = validation || problem.detail || problem.title || statusMessage(response.status)
  return new ApiError(response.status, code, message)
}

export function isAbortError(cause: unknown): cause is DOMException {
  return cause instanceof DOMException && cause.name === 'AbortError'
}

function statusCode(status: number) {
  return ({ 401: 'unauthorized', 403: 'forbidden', 404: 'not_found', 409: 'conflict', 500: 'server.unexpected_error' } as Record<number, string>)[status] ?? `http.${status}`
}

function statusMessage(status: number) {
  return ({
    401: 'Сессия истекла. Войдите снова.',
    403: 'Недостаточно прав для выполнения операции.',
    404: 'Запрошенные данные не найдены.',
    409: 'Операция конфликтует с текущим состоянием данных.',
    500: 'Внутренняя ошибка сервера. Повторите попытку позже.',
  } as Record<number, string>)[status] ?? `Ошибка HTTP ${status}`
}
