import { BookOpen, ClipboardCheck, ClipboardList, FileClock, History, LayoutDashboard, LogOut, Moon, Settings, Sun, Tags, Users } from 'lucide-react'
import { useEffect, useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { keycloak } from '@/shared/auth/keycloak'
import { environment } from '@/shared/config/environment'

const navigation = [
  { to: '/', label: 'Обзор', icon: LayoutDashboard, end: true },
  { to: '/exams', label: 'Экзамены', icon: ClipboardList },
  { to: '/tests', label: 'Тесты', icon: ClipboardCheck },
  { to: '/attempts', label: 'Попытки', icon: History },
  { to: '/audit-log', label: 'Журнал аудита', icon: FileClock },
  { to: '/questions', label: 'Вопросы', icon: BookOpen },
  { to: '/tags', label: 'Теги', icon: Tags },
  { href: environment.keycloakAdminUrl, label: 'Пользователи', icon: Users },
]

export function AppLayout() {
  const username = keycloak.tokenParsed?.preferred_username as string | undefined
  const [dark, setDark] = useState(() => document.documentElement.classList.contains('dark'))

  useEffect(() => {
    const syncTheme = () => setDark(document.documentElement.classList.contains('dark'))
    window.addEventListener('themechange', syncTheme)
    return () => window.removeEventListener('themechange', syncTheme)
  }, [])

  function toggleTheme() {
    const next = !dark
    setDark(next)
    document.documentElement.classList.toggle('dark', next)
    localStorage.setItem('theme', next ? 'dark' : 'light')
  }

  return (
    <div className="min-h-screen bg-slate-50 text-slate-950 transition-colors dark:bg-slate-950 dark:text-slate-100">
      <aside className="fixed inset-y-0 hidden w-64 border-r border-slate-200 bg-white p-5 transition-colors dark:border-slate-800 dark:bg-slate-900 lg:block">
        <NavLink className="flex items-center gap-3 px-2 text-lg font-semibold" to="/">
          <span className="grid size-9 place-items-center rounded-lg bg-indigo-600 font-bold text-white">T</span>
          TestPlatform
        </NavLink>
        <p className="mt-1 px-2 text-sm text-slate-500">Административная панель</p>
        <nav className="mt-9 space-y-1">
          {navigation.map(({ to, href, label, icon: Icon, end }) => href ? (
            <a className="nav-item" href={href} key={href}><Icon size={19} /> {label}</a>
          ) : (
            <NavLink className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`} end={end} key={to} to={to!}>
              <Icon size={19} /> {label}
            </NavLink>
          ))}
        </nav>
        <div className="absolute inset-x-5 bottom-5 space-y-1 border-t border-slate-200 pt-4 dark:border-slate-800">
          <NavLink className={({ isActive }) => `nav-item ${isActive ? 'nav-item-active' : ''}`} to="/settings"><Settings size={19} /> Настройки</NavLink>
          <button className="nav-item w-full" onClick={() => keycloak.logout({ redirectUri: window.location.origin })}><LogOut size={19} /> Выйти</button>
        </div>
      </aside>

      <main className="lg:pl-64">
        <header className="flex h-16 items-center justify-between border-b border-slate-200 bg-white px-6 transition-colors dark:border-slate-800 dark:bg-slate-900 lg:px-10">
          <div className="font-medium lg:hidden">TestPlatform</div>
          <div className="ml-auto flex items-center gap-3">
            <button aria-label={dark ? 'Включить светлую тему' : 'Включить тёмную тему'} className="icon-button" onClick={toggleTheme} title={dark ? 'Светлая тема' : 'Тёмная тема'} type="button">
              {dark ? <Sun size={19} /> : <Moon size={19} />}
            </button>
            <div className="text-right">
              <p className="text-sm font-medium">{username ?? 'Администратор'}</p>
              <p className="text-xs text-slate-500">Admin</p>
            </div>
            <div className="grid size-9 place-items-center rounded-full bg-indigo-100 text-sm font-semibold text-indigo-700">{(username?.[0] ?? 'A').toUpperCase()}</div>
          </div>
        </header>
        <Outlet />
      </main>
    </div>
  )
}
