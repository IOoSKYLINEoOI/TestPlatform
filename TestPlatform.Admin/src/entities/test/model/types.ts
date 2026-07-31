import type { Page } from '@/shared/api/page'
export type Test = { id: string; title: string; description: string; timeLimitSeconds: number | null; createdAt: string; updatedAt: string; status: string; totalQuestions: number }
export type TestPage = Page<Test>
export type TestInput = { title: string; description: string }
export type TestDetails = Omit<Test, 'totalQuestions'> & { coverImageId: string | null; publishedAt: string | null; questions: Array<{ questionId: string; order: number }> }
