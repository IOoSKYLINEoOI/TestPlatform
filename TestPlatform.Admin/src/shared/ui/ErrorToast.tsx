import { AlertCircle, X } from 'lucide-react'
import { useEffect } from 'react'

type ErrorToastProps = {
  message?: string
  onClose: () => void
}

export function ErrorToast({ message, onClose }: ErrorToastProps) {
  useEffect(() => {
    if (!message) return
    const timer = window.setTimeout(onClose, 8000)
    return () => window.clearTimeout(timer)
  }, [message, onClose])

  if (!message) return null

  return <div className="fixed right-5 top-5 z-[100] flex w-[min(28rem,calc(100vw-2.5rem))] animate-[toast-in_180ms_ease-out] items-start gap-3 rounded-xl border border-rose-200 bg-white p-4 text-rose-800 shadow-2xl" role="alert">
    <AlertCircle className="mt-0.5 shrink-0 text-rose-600" size={20} />
    <div className="min-w-0 flex-1"><p className="font-semibold">Ошибка</p><p className="mt-1 break-words text-sm text-slate-700">{message}</p></div>
    <button aria-label="Закрыть уведомление" className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-700" onClick={onClose} type="button"><X size={17} /></button>
  </div>
}

