import { X } from 'lucide-react'
import { FormEvent, useState } from 'react'
import { tagsApi } from '@/entities/tag'
import type { Tag, TagInput } from '@/entities/tag'
import { ErrorToast } from '@/shared/ui'

export function TagDialog({ tag, onClose, onSaved }: { tag: Tag | null; onClose: () => void; onSaved: () => Promise<void> }) {
  const [input, setInput] = useState<TagInput>({ name: tag?.name ?? '', description: tag?.description ?? '' })
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string>()

  async function submit(event: FormEvent) {
    event.preventDefault()
    setSaving(true)
    setError(undefined)
    try {
      if (tag) await tagsApi.updateTag(tag.id, input)
      else await tagsApi.createTag(input)
      await onSaved()
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Не удалось сохранить тег.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-slate-950/40 p-4" onMouseDown={(event) => event.target === event.currentTarget && onClose()}>
      <ErrorToast message={error} onClose={() => setError(undefined)} />
      <form className="w-full max-w-lg rounded-2xl bg-white p-6 shadow-xl" onSubmit={submit}>
        <div className="flex items-start justify-between"><div><h2 className="text-xl font-semibold">{tag ? 'Редактирование тега' : 'Новый тег'}</h2><p className="mt-1 text-sm text-slate-500">Название должно быть понятным и уникальным.</p></div><button className="icon-button" onClick={onClose} type="button"><X size={18} /></button></div>
        <label className="label mt-6">Название<input autoFocus className="input mt-2" maxLength={100} onChange={(event) => setInput({ ...input, name: event.target.value })} required value={input.name} /></label>
        <label className="label mt-5">Описание<textarea className="input mt-2 min-h-28 resize-y" maxLength={500} onChange={(event) => setInput({ ...input, description: event.target.value })} required value={input.description} /></label>
        <div className="mt-7 flex justify-end gap-3"><button className="button-secondary" onClick={onClose} type="button">Отмена</button><button className="button-primary" disabled={saving} type="submit">{saving ? 'Сохранение…' : 'Сохранить'}</button></div>
      </form>
    </div>
  )
}




