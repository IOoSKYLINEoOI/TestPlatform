import { request } from '@/shared/api/httpClient'
import type { DashboardStats } from '../model/types'

export const dashboardApi = {
  getStats(signal?: AbortSignal) { return request<DashboardStats>('/dashboard', { signal }) },
}
