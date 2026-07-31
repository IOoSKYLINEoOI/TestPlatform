import { describe, expect, it } from 'vitest'
import { ApiError, responseError } from './httpClient'

describe('responseError', () => {
  it.each([
    [401, 'unauthorized'],
    [403, 'forbidden'],
    [404, 'not_found'],
    [409, 'conflict'],
    [500, 'server.unexpected_error'],
  ])('maps HTTP %s to %s', async (status, code) => {
    const error = await responseError(new Response(null, { status }))
    expect(error).toBeInstanceOf(ApiError)
    expect(error.status).toBe(status)
    expect(error.code).toBe(code)
  })

  it('keeps backend problem code and validation messages', async () => {
    const error = await responseError(new Response(JSON.stringify({ code: 'tag.in_use', errors: { Name: ['Тег используется.'] } }), {
      status: 409,
      headers: { 'Content-Type': 'application/json' },
    }))
    expect(error.code).toBe('tag.in_use')
    expect(error.message).toBe('Тег используется.')
  })
})
