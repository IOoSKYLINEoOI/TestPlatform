import type { Page } from '@/shared/api/page'
import type { Tag } from '@/entities/tag/@x/question'
export type QuestionKind = 'choice' | 'text' | 'number' | 'matching'
export type QuestionStatus = 'Draft' | 'Published' | 'Archived' | 'draft' | 'published' | 'archived'
export type Question = { id: string; text: string; imageId: string | null; kind: QuestionKind; type: 'Choice' | 'Text' | 'Number' | 'Matching'; status: QuestionStatus; tags: Tag[] }
export type QuestionPage = Page<Question>
export type QuestionInput = Record<string, unknown> & { kind: QuestionKind; text: string; explanation: string | null; imageId: string | null; tagIds: string[] }
export type QuestionEditor = { id: string; text: string; kind: QuestionKind; type?: Question['type']; imageId: string | null; tags: Tag[]; explanation: string | null; status: QuestionStatus; createdByUserId: string; createdAt: string; updatedAt: string; mode?: 'Single' | 'Multiple'; evaluationMode?: 'Strict' | 'Partial'; options?: Array<{ id: string; text: string; imageId: string | null; isCorrect: boolean }>; correctAnswer?: string | number; leftItems?: Array<{ id: string; text: string; imageId: string | null }>; rightItems?: Array<{ id: string; text: string; imageId: string | null }>; pairs?: Array<{ leftId: string; rightId: string }> }
