import { BookOpen, ExternalLink, Moon, ShieldCheck, Sun } from 'lucide-react'
import { useState } from 'react'
import type { ReactNode } from 'react'
import { environment } from '@/shared/config/environment'

function currentTheme() { return document.documentElement.classList.contains('dark') ? 'dark' : 'light' }

export function SettingsPage() {
  const [theme, setTheme] = useState(currentTheme)
  function changeTheme(next: 'light' | 'dark') {
    setTheme(next)
    document.documentElement.classList.toggle('dark', next === 'dark')
    localStorage.setItem('theme', next)
    window.dispatchEvent(new Event('themechange'))
  }
  return <section className="page-shell"><h1 className="page-title">Настройки</h1><p className="page-description">Интерфейс и служебные инструменты платформы.</p><div className="mt-8 grid gap-6 lg:grid-cols-2"><div className="card"><div className="flex items-center gap-3"><span className="grid size-10 place-items-center rounded-lg bg-indigo-50 text-indigo-600">{theme === 'dark' ? <Moon size={20} /> : <Sun size={20} />}</span><div><h2 className="font-semibold">Оформление</h2><p className="mt-1 text-sm text-slate-500">Выберите тему административной панели.</p></div></div><div className="mt-5 grid grid-cols-2 gap-3"><button className={`rounded-lg border p-4 text-left ${theme === 'light' ? 'border-indigo-500 bg-indigo-50' : 'border-slate-200'}`} onClick={() => changeTheme('light')} type="button"><Sun size={18} /><p className="mt-3 text-sm font-semibold">Светлая</p></button><button className={`rounded-lg border p-4 text-left ${theme === 'dark' ? 'border-indigo-500 bg-indigo-50' : 'border-slate-200'}`} onClick={() => changeTheme('dark')} type="button"><Moon size={18} /><p className="mt-3 text-sm font-semibold">Тёмная</p></button></div></div><div className="card"><div className="flex items-center gap-3"><span className="grid size-10 place-items-center rounded-lg bg-indigo-50 text-indigo-600"><ShieldCheck size={20} /></span><div><h2 className="font-semibold">Системное администрирование</h2><p className="mt-1 text-sm text-slate-500">Ссылки на сервисы, где управляются системные настройки.</p></div></div><div className="mt-5 space-y-2"><SystemLink href={environment.keycloakAdminUrl} icon={<ShieldCheck size={18} />} label="Пользователи и роли" note="Keycloak" /><SystemLink href={environment.swaggerUrl} icon={<BookOpen size={18} />} label="Документация API" note="Swagger" /><SystemLink href={environment.seqUrl} icon={<ExternalLink size={18} />} label="Журналы платформы" note="Seq" /><SystemLink href={environment.minioConsoleUrl} icon={<ExternalLink size={18} />} label="Хранилище файлов" note="MinIO" /></div></div></div></section>
}

function SystemLink({ href, icon, label, note }: { href: string; icon: ReactNode; label: string; note: string }) { return <a className="flex items-center gap-3 rounded-lg border border-slate-200 p-3 text-sm hover:bg-slate-50" href={href} target="_blank" rel="noreferrer"><span className="text-indigo-600">{icon}</span><span className="flex-1 font-medium">{label}</span><span className="text-slate-500">{note}</span><ExternalLink className="text-slate-400" size={16} /></a> }
