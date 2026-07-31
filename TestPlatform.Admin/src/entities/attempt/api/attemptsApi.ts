import { request } from '@/shared/api/httpClient'
import type { AttemptDetails, AttemptsPage, AttemptsQuery, AttemptSourceType, AttemptSourcesPage, AttemptSourcesQuery } from '../model/types'

function queryString(query: AttemptsQuery, includePassed: boolean) {
  const params = new URLSearchParams({ page: String(query.page), pageSize: String(query.pageSize ?? 20) })
  if (query.status) params.set('status', query.status)
  if (query.employeeNumber?.trim()) params.set('employeeNumber', query.employeeNumber.trim())
  if (includePassed && query.passed !== undefined) params.set('passed', String(query.passed))
  return params.toString()
}

export const attemptsApi = {
  getSources(query: AttemptSourcesQuery, signal?: AbortSignal) {
    const params = new URLSearchParams({ page: String(query.page), pageSize: String(query.pageSize ?? 20) })
    if (query.search?.trim()) params.set('search', query.search.trim())
    if (query.type) params.set('type', query.type)
    return request<AttemptSourcesPage>(`/attempts/sources?${params}`, { signal })
  },
  getBySource(type: AttemptSourceType, sourceId: string, query: AttemptsQuery, signal?: AbortSignal) {
    return request<AttemptsPage>(`/${type === 'test' ? 'tests' : 'exams'}/${sourceId}/attempts?${queryString(query, type === 'exam')}`, { signal })
  },
  getResult(id: string, signal?: AbortSignal) { return request<AttemptDetails>(`/attempts/${id}/result`, { signal }) },
  cancel(id: string) { return request<void>(`/attempts/${id}/cancel`, { method: 'POST' }) },
}
