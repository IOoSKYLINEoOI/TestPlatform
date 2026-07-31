import { request } from '@/shared/api/httpClient'
import type { AuditLogPage } from '../model/types'
export const auditLogApi = { getPage(page: number, employeeNumber: string, method: string, pageSize = 20, signal?: AbortSignal) { const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) }); if (employeeNumber.trim()) query.set('employeeNumber', employeeNumber.trim()); if (method) query.set('method', method); return request<AuditLogPage>(`/audit-log?${query}`, { signal }) } }
