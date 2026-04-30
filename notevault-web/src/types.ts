export type ContentType = 'markdown' | 'html'

export interface Folder { id: string; parentId?: string | null; name: string; sort: number; createdAt: string; updatedAt: string }
export interface Tag { id: string; name: string; color: string; createdAt: string }
export interface Attachment { id: string; noteId?: string | null; fileName: string; fileKey: string; fileUrl: string; bucket: string; fileSize: number; mimeType: string; createdAt: string }
export interface NoteList {
  id: string; folderId?: string | null; title: string; contentType: ContentType; isEncrypted: boolean; isPinned: boolean; isDeleted: boolean; lastOpenedAt?: string | null; createdAt: string; updatedAt: string; tags: Tag[]
}
export interface NoteDetail extends NoteList {
  content: string; salt?: string | null; iv?: string | null; cipherText?: string | null; attachments: Attachment[]
}
export interface NotePayload {
  folderId?: string | null; title: string; content: string; contentType: ContentType; isEncrypted: boolean; salt?: string | null; iv?: string | null; cipherText?: string | null; isPinned: boolean; tagIds: string[]
}
