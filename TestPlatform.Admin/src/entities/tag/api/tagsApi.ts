import { request } from '@/shared/api/httpClient'
import type { TagInput, TagPage, TagQuestionsPage, TagUsage } from '../model/types'

export const tagsApi = {
  getTags(search: string, page: number, pageSize = 10, signal?: AbortSignal) { const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) }); if (search.trim()) query.set('search', search.trim()); return request<TagPage>(`/tags?${query}`, { signal }) },
  createTag(input: TagInput) { return request<{ id: string }>('/tags', { method: 'POST', body: JSON.stringify(input) }) },
  updateTag(id: string, input: TagInput) { return request<void>(`/tags/${id}`, { method: 'PUT', body: JSON.stringify(input) }) },
  deleteTag(id: string) { return request<void>(`/tags/${id}`, { method: 'DELETE' }) },
  getUsage(id: string) { return request<TagUsage>(`/tags/${id}/usage`) },
  getQuestions(id: string, page = 1, pageSize = 20) { return request<TagQuestionsPage>(`/tags/${id}/questions?page=${page}&pageSize=${pageSize}`) },
  mergeTags(sourceId: string, targetId: string) { return request<{ affectedQuestionCount: number }>(`/tags/${sourceId}/merge/${targetId}`, { method: 'POST' }) },
}
