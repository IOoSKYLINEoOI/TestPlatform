import { request } from '@/shared/api/httpClient'
import type { TestDetails, TestInput, TestPage } from '../model/types'

export const testsApi = {
  getTests(search: string, page: number, pageSize = 10, signal?: AbortSignal) { const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) }); if (search.trim()) query.set('search', search.trim()); return request<TestPage>(`/tests/management?${query}`, { signal }) },
  createTest(input: TestInput) { return request<string>('/tests', { method: 'POST', body: JSON.stringify(input) }) },
  updateTest(id: string, input: TestInput) { return request<void>(`/tests/${id}`, { method: 'PATCH', body: JSON.stringify(input) }) },
  getTest(id: string, signal?: AbortSignal) { return request<TestDetails>(`/tests/${id}`, { signal }) },
  publishTest(id: string) { return request<void>(`/tests/${id}/publish`, { method: 'POST' }) },
  archiveTest(id: string) { return request<void>(`/tests/${id}/archive`, { method: 'POST' }) },
  updateTestTimeLimit(id: string, seconds: number) { return request<void>(`/tests/${id}/time-limit`, { method: 'PUT', body: JSON.stringify({ timeLimitSeconds: seconds }) }) },
  deleteTestTimeLimit(id: string) { return request<void>(`/tests/${id}/time-limit`, { method: 'DELETE' }) },
  updateTestCover(id: string, fileId: string) { return request<void>(`/tests/${id}/cover-image`, { method: 'PUT', body: JSON.stringify({ fileId }) }) },
  deleteTestCover(id: string) { return request<void>(`/tests/${id}/cover-image`, { method: 'DELETE' }) },
  addTestQuestion(id: string, questionId: string) { return request<void>(`/tests/${id}/questions`, { method: 'POST', body: JSON.stringify({ questionId }) }) },
  deleteTestQuestion(id: string, questionId: string) { return request<void>(`/tests/${id}/questions/${questionId}`, { method: 'DELETE' }) },
  reorderTestQuestions(id: string, questionIds: string[]) { return request<void>(`/tests/${id}/questions/order`, { method: 'PUT', body: JSON.stringify({ questionIds }) }) },
}
