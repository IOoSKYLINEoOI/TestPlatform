import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { ErrorPage } from './ErrorPage'

describe('ErrorPage', () => {
  it('shows status, explanation and recovery link', () => {
    render(<MemoryRouter><ErrorPage code={404} title="Страница не найдена" description="Проверьте адрес" /></MemoryRouter>)
    expect(screen.getByText('Ошибка 404')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Страница не найдена' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'На главную' })).toHaveAttribute('href', '/')
  })
})
