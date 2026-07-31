import { ArrowLeft, Save } from 'lucide-react'
import { FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { testsApi } from '@/entities/test'
import { useUnsavedChanges } from '@/shared/lib'
import { ErrorToast } from '@/shared/ui'

export function TestCreatePage() {
  const navigate = useNavigate()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [saving, setSaving] = useState(false)
  const [dirty, setDirty] = useState(false)
  const [error, setError] = useState<string>()
  useUnsavedChanges(dirty && !saving)

  async function submit(event: FormEvent) {
    event.preventDefault(); setSaving(true); setError(undefined)
    try {
      const id = await testsApi.createTest({ title, description })
      setDirty(false)
      navigate(`/tests/${id}`)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Не удалось создать тест.')
    } finally { setSaving(false) }
  }

  return <section className="page-shell">
    <ErrorToast message={error} onClose={() => setError(undefined)} />
    <button className="inline-flex items-center gap-2 text-sm font-medium text-indigo-700 hover:text-indigo-900" onClick={() => navigate('/tests')} type="button"><ArrowLeft size={17} /> Назад к тестам</button>
    <form className="card mx-auto mt-5 w-full max-w-3xl" onChangeCapture={() => setDirty(true)} onSubmit={submit}>
      <h1 className="text-xl font-semibold">Новый тест</h1>
      <p className="mt-1 text-sm text-slate-500">Тест будет создан как черновик. После создания настройте вопросы и опубликуйте его.</p>
      <label className="label mt-6">Название<input autoFocus className="input mt-2" maxLength={200} onChange={(event) => setTitle(event.target.value)} required value={title} /></label>
      <label className="label mt-5">Описание<textarea className="input mt-2 min-h-36 resize-y" maxLength={2000} onChange={(event) => setDescription(event.target.value)} required value={description} /></label>
      <div className="mt-7 flex justify-end gap-3"><button className="button-secondary" onClick={() => navigate('/tests')} type="button">Отмена</button><button className="button-primary" disabled={saving} type="submit"><Save size={16} /> {saving ? 'Создание…' : 'Создать тест'}</button></div>
    </form>
  </section>
}
