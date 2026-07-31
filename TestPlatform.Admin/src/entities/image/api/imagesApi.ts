import { request } from '@/shared/api/httpClient'

type UploadedImage = { fileId: string; url: string }

export const imagesApi = {
  getImageUrl(id: string, signal?: AbortSignal) { return request<{ url: string }>(`/images/${id}/url`, { signal }) },
  async uploadImage(file: File, signal?: AbortSignal) {
    const form = new FormData()
    form.append('file', file)
    const uploaded = await request<UploadedImage>('/images', { method: 'POST', body: form, signal })
    const preview = await request<{ url: string }>(`/images/${uploaded.fileId}/url`, { signal })
    return { ...uploaded, previewUrl: preview.url }
  },
}
