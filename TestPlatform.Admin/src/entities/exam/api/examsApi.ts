import { request } from '@/shared/api/httpClient'
import type { ExamDetails, ExamInput, ExamPage } from '../model/types'

export const examsApi = {
  getExams(search: string, page: number, pageSize = 10, signal?: AbortSignal) { const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) }); if (search.trim()) query.set('search', search.trim()); return request<ExamPage>(`/exams/management?${query}`, { signal }) },
  createExam(input: ExamInput) { return request<string>('/exams', { method: 'POST', body: JSON.stringify(input) }) },
  updateExam(id: string, input: ExamInput) { return request<void>(`/exams/${id}`, { method: 'PATCH', body: JSON.stringify(input) }) },
  getExam(id: string, signal?: AbortSignal) { return request<ExamDetails>(`/exams/${id}`, { signal }) },
  publishExam(id: string) { return request<void>(`/exams/${id}/publish`, { method: 'POST' }) }, archiveExam(id: string) { return request<void>(`/exams/${id}/archive`, { method: 'POST' }) },
  updateExamTimeLimit(id: string, seconds: number) { return request<void>(`/exams/${id}/time-limit`, { method: 'PUT', body: JSON.stringify({ timeLimitSeconds: seconds }) }) }, deleteExamTimeLimit(id: string) { return request<void>(`/exams/${id}/time-limit`, { method: 'DELETE' }) },
  updateExamCover(id: string, fileId: string) { return request<void>(`/exams/${id}/cover-image`, { method: 'PUT', body: JSON.stringify({ fileId }) }) }, deleteExamCover(id: string) { return request<void>(`/exams/${id}/cover-image`, { method: 'DELETE' }) },
  updateExamSchedule(id: string, input: { availableFrom: string | null; availableTo: string | null }) { return request<void>(`/exams/${id}/schedule`, { method: 'PUT', body: JSON.stringify(input) }) }, deleteExamSchedule(id: string) { return request<void>(`/exams/${id}/schedule`, { method: 'DELETE' }) },
  updateExamReviewPolicy(id: string, reviewPolicy: 'Immediately' | 'AfterExamClosed') { return request<void>(`/exams/${id}/review-policy`, { method: 'PUT', body: JSON.stringify({ reviewPolicy }) }) },
  updateExamAttemptsLimit(id: string, attemptsLimit: number) { return request<void>(`/exams/${id}/attempts-limit`, { method: 'PUT', body: JSON.stringify({ attemptsLimit }) }) },
  updateExamPassingRule(id: string, input: { minScore: number | null; minPercent: number | null }) { return request<void>(`/exams/${id}/passing-rule`, { method: 'PUT', body: JSON.stringify(input) }) },
  addExamSection(id: string, input: { name: string; questionsToSelect: number; scorePerQuestion: number }) { return request<{ id: string }>(`/exams/${id}/sections`, { method: 'POST', body: JSON.stringify(input) }) },
  updateExamSection(examId: string, sectionId: string, input: { name: string; questionsToSelect: number; scorePerQuestion: number }) { return request<void>(`/exams/${examId}/sections/${sectionId}`, { method: 'PATCH', body: JSON.stringify(input) }) },
  deleteExamSection(examId: string, sectionId: string) { return request<void>(`/exams/${examId}/sections/${sectionId}`, { method: 'DELETE' }) },
  addExamSectionQuestion(examId: string, sectionId: string, questionId: string) { return request<void>(`/exams/${examId}/sections/${sectionId}/questions`, { method: 'POST', body: JSON.stringify({ questionId }) }) },
  deleteExamSectionQuestion(examId: string, sectionId: string, questionId: string) { return request<void>(`/exams/${examId}/sections/${sectionId}/questions/${questionId}`, { method: 'DELETE' }) },
}
