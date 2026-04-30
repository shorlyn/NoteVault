import { api } from './client'
import type { Folder, NoteDetail, NoteList, NotePayload, Tag } from '../types'

export const authApi = {
  login: (username: string, password: string) => api.post('/api/auth/login', { username, password }).then(r => r.data),
  refresh: (refreshToken: string) => api.post('/api/auth/refresh', { refreshToken }).then(r => r.data),
  changePassword: (oldPassword: string, newPassword: string) => api.post('/api/auth/change-password', { oldPassword, newPassword }),
}

export const foldersApi = {
  list: () => api.get<Folder[]>('/api/folders').then(r => r.data),
  create: (data: Partial<Folder>) => api.post<Folder>('/api/folders', data).then(r => r.data),
  update: (id: string, data: Partial<Folder>) => api.put<Folder>(`/api/folders/${id}`, data).then(r => r.data),
  remove: (id: string) => api.delete(`/api/folders/${id}`),
}

export const notesApi = {
  list: (params: Record<string, unknown>) => api.get<NoteList[]>('/api/notes', { params }).then(r => r.data),
  detail: (id: string) => api.get<NoteDetail>(`/api/notes/${id}`).then(r => r.data),
  create: (data: NotePayload) => api.post<NoteDetail>('/api/notes', data).then(r => r.data),
  update: (id: string, data: NotePayload) => api.put<NoteDetail>(`/api/notes/${id}`, data).then(r => r.data),
  remove: (id: string) => api.delete(`/api/notes/${id}`),
  restore: (id: string) => api.post(`/api/notes/${id}/restore`),
  hardRemove: (id: string) => api.delete(`/api/notes/${id}/hard`),
  open: (id: string) => api.post(`/api/notes/${id}/open`),
}

export const tagsApi = {
  list: () => api.get<Tag[]>('/api/tags').then(r => r.data),
  create: (data: { name: string; color: string }) => api.post<Tag>('/api/tags', data).then(r => r.data),
  remove: (id: string) => api.delete(`/api/tags/${id}`),
}

export const uploadApi = {
  file: (file: File, noteId?: string) => {
    const form = new FormData()
    form.append('file', file)
    if (noteId) form.append('noteId', noteId)
    return api.post('/api/upload', form).then(r => r.data)
  },
}
