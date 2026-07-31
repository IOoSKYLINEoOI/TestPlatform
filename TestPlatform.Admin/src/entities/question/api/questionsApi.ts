import { request } from '@/shared/api/httpClient'
import type { Question, QuestionEditor, QuestionInput, QuestionPage } from '../model/types'

export const questionsApi = {
  getQuestions(status: string, page: number, pageSize = 10, signal?: AbortSignal) { const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) }); if (status) query.set('status', status); return request<QuestionPage>(`/questions?${query}`, { signal }) },
  createQuestion(input: QuestionInput) { return request<string>('/questions', { method: 'POST', body: JSON.stringify(input) }) },
  async getQuestion(id: string, signal?: AbortSignal) { const question = await request<QuestionEditor>(`/questions/${id}`, { signal }); return { ...question, kind: normalizeKind(question.kind, question.type) } },
  updateQuestion(id: string, input: QuestionInput) { return request<void>(`/questions/${id}`, { method: 'PUT', body: JSON.stringify(input) }) },
  publishQuestion(id: string) { return request<void>(`/questions/${id}/publish`, { method: 'POST' }) },
  archiveQuestion(id: string) { return request<void>(`/questions/${id}/archive`, { method: 'POST' }) },
  cloneQuestion(id: string) { return request<string>(`/questions/${id}/clone`, { method: 'POST' }) },
}

function normalizeKind(kind?: string, type?: string): Question['kind'] { const value = (kind || type || '').toLowerCase(); if (value === 'choice' || value === 'text' || value === 'number' || value === 'matching') return value; throw new Error(`Неизвестный тип вопроса: ${kind || type || 'не указан'}`) }
