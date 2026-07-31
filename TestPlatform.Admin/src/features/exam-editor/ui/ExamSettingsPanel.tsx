import type { ExamDetails } from '@/entities/exam'
import { ExamCoverCard, ExamInfoCard, ExamPassingRuleCard, ExamRestrictionsCard } from './ExamSettingsCards'

export type ExamSettingsPanelProps = {
  exam: ExamDetails; editable: boolean; busy: boolean; coverUrl?: string
  passingRuleType: 'score' | 'percent'; passingValue: string; reviewPolicy: 'Immediately' | 'AfterExamClosed'
  hasTimeLimit: boolean; timeLimit: string; availableFrom: string; availableTo: string; attemptsLimit: string
  setPassingRuleType: (value: 'score' | 'percent') => void; setPassingValue: (value: string) => void
  setReviewPolicy: (value: 'Immediately' | 'AfterExamClosed') => void; setTimeLimit: (value: string) => void
  setAvailableFrom: (value: string) => void; setAvailableTo: (value: string) => void; setAttemptsLimit: (value: string) => void
  onRemoveCover: () => Promise<void>; onUploadCover: (file: File) => Promise<void>; onSavePassingRule: () => Promise<void>
  onToggleTimeLimit: (enabled: boolean) => Promise<void>; onSaveTimeLimit: () => Promise<void>; onSaveSchedule: () => Promise<void>; onSaveAttempts: () => Promise<void>
}

export function ExamSettingsPanel(props: ExamSettingsPanelProps) {
  return <div className="flex flex-col gap-6">
    <ExamInfoCard exam={props.exam} />
    <ExamCoverCard busy={props.busy} coverUrl={props.coverUrl} editable={props.editable} onRemove={props.onRemoveCover} onUpload={props.onUploadCover} />
    <ExamPassingRuleCard busy={props.busy} editable={props.editable} exam={props.exam} onChangeType={props.setPassingRuleType} onChangeValue={props.setPassingValue} onSave={props.onSavePassingRule} type={props.passingRuleType} value={props.passingValue} />
    <ExamRestrictionsCard {...props} />
  </div>
}
