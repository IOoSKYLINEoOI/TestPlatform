import type { Page } from '@/shared/api/page'
export type AuditLogItem = { id: string; userId: string | null; employeeNumber: string | null; method: string; path: string; statusCode: number; traceId: string; createdAt: string }
export type AuditLogPage = Page<AuditLogItem>
