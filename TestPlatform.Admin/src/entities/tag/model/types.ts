import type { Page } from '@/shared/api/page'
export type Tag = { id: string; name: string; description: string | null }
export type TagPage = Page<Tag>
export type TagInput = { name: string; description: string }
export type TagUsage = { tagId: string; questionCount: number }
export type TagQuestion = { id: string; text: string; kind: string; type: string; status: string }
export type TagQuestionsPage = Page<TagQuestion>
