import { lazy, Suspense } from 'react'
import type { ComponentType } from 'react'
import { Route, Routes } from 'react-router-dom'
import { AppLayout } from '@/app/layout/AppLayout'
import { LoadingScreen } from '@/features/auth'

const page = <T extends Record<string, unknown>>(load: () => Promise<T>, name: keyof T) => lazy(() => load().then((module) => ({ default: module[name] as ComponentType })))
const DashboardPage = page(() => import('@/pages/dashboard'), 'DashboardPage')
const ExamsPage = page(() => import('@/pages/exams'), 'ExamsPage')
const ExamCreatePage = page(() => import('@/pages/exam-create'), 'ExamCreatePage')
const ExamDetailsPage = page(() => import('@/pages/exam-details'), 'ExamDetailsPage')
const SettingsPage = page(() => import('@/pages/settings'), 'SettingsPage')
const QuestionsPage = page(() => import('@/pages/questions'), 'QuestionsPage')
const QuestionEditorPage = page(() => import('@/pages/question-editor'), 'QuestionEditorPage')
const TagsPage = page(() => import('@/pages/tags'), 'TagsPage')
const TestsPage = page(() => import('@/pages/tests'), 'TestsPage')
const TestCreatePage = page(() => import('@/pages/test-create'), 'TestCreatePage')
const TestDetailsPage = page(() => import('@/pages/test-details'), 'TestDetailsPage')
const AttemptsPage = page(() => import('@/pages/attempts'), 'AttemptsPage')
const AttemptDetailsPage = page(() => import('@/pages/attempt-details'), 'AttemptDetailsPage')
const AttemptsOverviewPage = page(() => import('@/pages/attempts-overview'), 'AttemptsOverviewPage')
const ForbiddenPage = page(() => import('@/pages/error'), 'ForbiddenPage')
const NotFoundPage = page(() => import('@/pages/error'), 'NotFoundPage')
const ServerErrorPage = page(() => import('@/pages/error'), 'ServerErrorPage')
const AuditLogPage = page(() => import('@/pages/audit-log'), 'AuditLogPage')

export function AppRoutes() {
  return <Suspense fallback={<LoadingScreen label="Загружаем страницу…" />}><Routes>
    <Route element={<AppLayout />}><Route index element={<DashboardPage />} /><Route path="audit-log" element={<AuditLogPage />} /><Route path="attempts" element={<AttemptsOverviewPage />} /><Route path="tags" element={<TagsPage />} /><Route path="exams" element={<ExamsPage />} /><Route path="exams/new" element={<ExamCreatePage />} /><Route path="exams/:id" element={<ExamDetailsPage />} /><Route path=":sourceType/:sourceId/attempts" element={<AttemptsPage />} /><Route path="attempts/:id" element={<AttemptDetailsPage />} /><Route path="tests" element={<TestsPage />} /><Route path="tests/new" element={<TestCreatePage />} /><Route path="tests/:id" element={<TestDetailsPage />} /><Route path="questions" element={<QuestionsPage />} /><Route path="questions/new" element={<QuestionEditorPage />} /><Route path="questions/:id" element={<QuestionEditorPage />} /><Route path="settings" element={<SettingsPage />} /><Route path="403" element={<ForbiddenPage />} /><Route path="500" element={<ServerErrorPage />} /><Route path="*" element={<NotFoundPage />} /></Route>
  </Routes></Suspense>
}
