import { ArrowLeft, Plus, Send, X } from 'lucide-react'
import { useNavigate, useParams } from 'react-router-dom'
import { ErrorToast } from '@/shared/ui'
import { ConfirmDialog } from '@/shared/ui'
import { ChoiceFields, MatchingFields, QuestionReadOnlyView } from '@/features/question-editor'
import { useQuestionEditorController } from '../model/useQuestionEditorController'
import type { Question } from '@/entities/question'

type Kind = Question['kind']
export function QuestionEditorPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const questionId = id === 'new' ? undefined : id
  return <QuestionDialog questionId={questionId} onClose={() => navigate('/questions')} onSaved={async () => navigate('/questions')} />
}

function QuestionDialog({ questionId, onClose, onSaved }: { questionId?: string; onClose: () => void; onSaved: () => Promise<void> }) {
  const controller = useQuestionEditorController(questionId, onSaved)
  const { kind, setKind, text, setText, explanation, setExplanation, tags, tagIds, setTagIds,
    newTagName, setNewTagName, newTagDescription, setNewTagDescription, creatingTag, imageId,
    setImageId, imagePreviewUrl, setImagePreviewUrl, uploadingImage, choiceMode, evaluationMode,
    setEvaluationMode, options, setOptions, correctAnswer, setCorrectAnswer, pairs, setPairs,
    saving, publishing, loadingQuestion, readonly, error, setError, confirmation, setConfirmation,
    setDirty, changeChoiceMode, setCorrect, createTag, uploadImage, submit, publish } = controller
  const changeKind = (value: Kind) => { setKind(value); setError(undefined) }
  const uploadOptionImage = (index: number, file: File) => uploadImage(file, index)

  if (readonly) return <QuestionReadOnlyView onClose={onClose} tags={tags} tagIds={tagIds} text={text} imageUrl={imagePreviewUrl} kind={kind} options={options} correctAnswer={correctAnswer} pairs={pairs} explanation={explanation} />

  return <section className="page-shell"><ErrorToast message={error} onClose={() => setError(undefined)} /><ConfirmDialog confirmation={confirmation} onClose={() => setConfirmation(undefined)} /><button className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900" onClick={onClose} type="button"><ArrowLeft size={17} /> Назад к вопросам</button><form className="card mx-auto mt-5 w-full max-w-4xl" onChangeCapture={() => setDirty(true)} onSubmit={submit}>
    <div className="flex items-start justify-between"><div><h2 className="text-xl font-semibold">{questionId ? 'Редактирование вопроса' : 'Новый вопрос'}</h2><p className="mt-1 text-sm text-slate-500">{questionId ? 'Изменения будут сохранены в черновике.' : 'Вопрос можно будет опубликовать после создания.'}</p></div><button className="icon-button" onClick={onClose} type="button"><X size={18} /></button></div>
    {loadingQuestion && <div className="mt-6 rounded-lg bg-slate-50 px-4 py-8 text-center text-sm text-slate-500">Загрузка вопроса…</div>}
    <fieldset disabled={loadingQuestion || readonly}>
    <div className="mt-6 grid gap-5 md:grid-cols-2"><label className="label">Тип вопроса<select className="input mt-2" onChange={(event) => changeKind(event.target.value as Kind)} value={kind}><option value="choice">Выбор ответа</option><option value="text">Текстовый ответ</option><option value="number">Числовой ответ</option><option value="matching">Сопоставление</option></select></label>{(kind === 'choice' || kind === 'matching') && <label className={`mt-7 flex items-center gap-3 rounded-lg border border-slate-200 px-4 py-3 text-sm font-medium ${kind === 'choice' && choiceMode === 'single' ? 'opacity-50' : ''}`}><input checked={evaluationMode === 'partial'} disabled={kind === 'choice' && choiceMode === 'single'} onChange={(event) => setEvaluationMode(event.target.checked ? 'partial' : 'strict')} type="checkbox" />Начислять частичные баллы</label>}</div>
    <label className="label mt-5">Текст вопроса<textarea className="input mt-2 min-h-24 resize-y" maxLength={500} onChange={(event) => setText(event.target.value)} required value={text} /></label>
    <label className="label mt-5">Пояснение после ответа<textarea className="input mt-2 min-h-20 resize-y" maxLength={2000} onChange={(event) => setExplanation(event.target.value)} value={explanation} /></label>
    <fieldset className="mt-5"><legend className="label">Изображение вопроса</legend><div className="mt-2 rounded-lg border border-dashed border-slate-300 p-4">{imageId && imagePreviewUrl ? <div className="flex items-start gap-4"><img alt="Изображение вопроса" className="max-h-44 max-w-xs rounded-lg object-contain" src={imagePreviewUrl} /><button className="button-secondary" onClick={() => { setImageId(null); setImagePreviewUrl(null) }} type="button">Удалить</button></div> : <label className="flex cursor-pointer items-center justify-center rounded-lg bg-slate-50 px-4 py-6 text-sm text-slate-600 hover:bg-slate-100">{uploadingImage ? 'Загрузка…' : 'Загрузить изображение'}<input accept="image/*" className="sr-only" disabled={uploadingImage} onChange={(event) => { const file = event.target.files?.[0]; if (file) void uploadImage(file) }} type="file" /></label>}</div></fieldset>
    <fieldset className="mt-5"><legend className="label">Теги</legend><div className="mt-2 flex max-h-32 flex-wrap gap-2 overflow-y-auto rounded-lg border border-slate-200 p-3">{tags.length ? tags.map((tag) => <label className="flex cursor-pointer items-center gap-2 rounded-md bg-slate-50 px-3 py-2 text-sm" key={tag.id}><input checked={tagIds.includes(tag.id)} onChange={(event) => setTagIds((current) => event.target.checked ? [...current, tag.id] : current.filter((id) => id !== tag.id))} type="checkbox" />{tag.name}</label>) : <span className="text-sm text-slate-400">Тегов пока нет</span>}</div><div className="mt-3 grid gap-2 md:grid-cols-[1fr_1.5fr_auto]"><input className="input" maxLength={100} onChange={(event) => setNewTagName(event.target.value)} placeholder="Название нового тега" value={newTagName} /><input className="input" maxLength={250} onChange={(event) => setNewTagDescription(event.target.value)} placeholder="Описание" value={newTagDescription} /><button className="button-secondary" disabled={creatingTag || !newTagName.trim() || !newTagDescription.trim()} onClick={() => void createTag()} type="button"><Plus size={16} /> {creatingTag ? 'Создание…' : 'Создать тег'}</button></div></fieldset>
    {kind === 'choice' && <ChoiceFields choiceMode={choiceMode} onModeChange={changeChoiceMode} onSetCorrect={setCorrect} onUploadImage={uploadOptionImage} options={options} setOptions={setOptions} uploadingImage={uploadingImage} />}
    {(kind === 'text' || kind === 'number') && <label className="label mt-5">Правильный ответ<input className="input mt-2" onChange={(event) => setCorrectAnswer(event.target.value)} required step={kind === 'number' ? 'any' : undefined} type={kind === 'number' ? 'number' : 'text'} value={correctAnswer} /></label>}
    {kind === 'matching' && <MatchingFields pairs={pairs} setPairs={setPairs} />}
    <div className="mt-7 flex justify-end gap-3"><button className="button-secondary" onClick={onClose} type="button">Отмена</button>{questionId && !readonly && <button className="button-secondary text-emerald-700 hover:bg-emerald-50" disabled={saving || publishing || uploadingImage || loadingQuestion} onClick={() => setConfirmation({ title: 'Опубликовать вопрос?', description: 'После публикации вопрос нельзя будет редактировать.', confirmLabel: 'Опубликовать', onConfirm: publish })} type="button"><Send size={16} /> {publishing ? 'Публикация…' : 'Опубликовать'}</button>}<button className="button-primary" disabled={saving || publishing || uploadingImage || loadingQuestion} type="submit">{saving ? 'Сохранение…' : uploadingImage ? 'Загрузить изображение' : questionId ? 'Сохранить изменения' : 'Создать вопрос'}</button></div>
    </fieldset>
  </form></section>
}


