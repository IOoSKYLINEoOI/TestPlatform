import { ErrorPage } from '@/shared/ui'

export function ForbiddenPage() { return <ErrorPage code={403} title="Недостаточно прав" description="У вашей учётной записи нет доступа к этому разделу или операции." /> }
export function NotFoundPage() { return <ErrorPage code={404} title="Страница не найдена" description="Проверьте адрес или вернитесь на главную страницу административной панели." /> }
export function ServerErrorPage() { return <ErrorPage code={500} title="Внутренняя ошибка" description="Сервис временно не может выполнить запрос. Попробуйте повторить операцию позже." /> }
