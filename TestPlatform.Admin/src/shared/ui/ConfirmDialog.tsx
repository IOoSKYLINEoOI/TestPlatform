import { AlertTriangle, X } from 'lucide-react'
import { useState } from 'react'

export type Confirmation = {
  title: string
  description: string
  confirmLabel: string
  danger?: boolean
  onConfirm: () => Promise<void>
}

export function ConfirmDialog({ confirmation, onClose }: { confirmation?: Confirmation; onClose: () => void }) {
  const [submitting, setSubmitting] = useState(false)
  if (!confirmation) return null
  const currentConfirmation = confirmation

  async function confirm() {
    setSubmitting(true)
    try { await currentConfirmation.onConfirm(); onClose() }
    finally { setSubmitting(false) }
  }

  return <div className="fixed inset-0 z-[90] grid place-items-center bg-slate-950/45 p-4" onMouseDown={(event) => event.target === event.currentTarget && !submitting && onClose()} role="dialog" aria-modal="true" aria-labelledby="confirmation-title"><div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl dark:bg-slate-900"><div className="flex items-start gap-4"><div className={`rounded-full p-2 ${confirmation.danger ? 'bg-rose-100 text-rose-700' : 'bg-amber-100 text-amber-700'}`}><AlertTriangle size={20} /></div><div className="min-w-0 flex-1"><h2 className="text-lg font-semibold" id="confirmation-title">{confirmation.title}</h2><p className="mt-2 text-sm text-slate-600 dark:text-slate-300">{confirmation.description}</p></div><button aria-label="Закрыть" className="icon-button" disabled={submitting} onClick={onClose} type="button"><X size={18} /></button></div><div className="mt-7 flex justify-end gap-3"><button className="button-secondary" disabled={submitting} onClick={onClose} type="button">Отмена</button><button className={confirmation.danger ? 'button-primary bg-rose-600 hover:bg-rose-700' : 'button-primary'} disabled={submitting} onClick={() => void confirm()} type="button">{submitting ? 'Выполняем…' : confirmation.confirmLabel}</button></div></div></div>
}

