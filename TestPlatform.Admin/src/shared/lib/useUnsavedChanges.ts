import { useEffect } from 'react'

const warning = 'Есть несохранённые изменения. Покинуть страницу?'

export function useUnsavedChanges(enabled: boolean) {
  useEffect(() => {
    if (!enabled) return
    const beforeUnload = (event: BeforeUnloadEvent) => { event.preventDefault(); event.returnValue = '' }
    const linkClick = (event: MouseEvent) => {
      const target = event.target instanceof Element ? event.target.closest('a[href]') : null
      if (!(target instanceof HTMLAnchorElement) || target.target === '_blank') return
      const url = new URL(target.href, window.location.href)
      if (url.origin !== window.location.origin || url.href === window.location.href) return
      if (!window.confirm(warning)) { event.preventDefault(); event.stopPropagation() }
    }
    window.addEventListener('beforeunload', beforeUnload)
    document.addEventListener('click', linkClick, true)
    return () => { window.removeEventListener('beforeunload', beforeUnload); document.removeEventListener('click', linkClick, true) }
  }, [enabled])
}
