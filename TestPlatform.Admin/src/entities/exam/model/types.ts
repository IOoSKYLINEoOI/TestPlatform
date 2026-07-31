import type { Page } from '@/shared/api/page'
export type Exam = { id: string; title: string; description: string; status: string; totalQuestions: number; totalMaxScore: number; createdAt: string; publishedAt: string | null }
export type ExamPage = Page<Exam>
export type ExamInput = { title: string; description: string }
export type ExamSection = { id: string; name: string; questionsToSelect: number; scorePerQuestion: number; maxScore: number; questionIds: string[] }
export type ExamDetails = Exam & { timeLimitSeconds: number | null; coverImageId: string | null; attemptsLimit: number; reviewPolicy: 'Immediately' | 'AfterExamClosed'; authorId: string; schedule: { availableFrom: string | null; availableTo: string | null } | null; passingRule: { minScore: number | null; minPercent: number | null } | null; sections: ExamSection[] }
