using NoteVault.Api.Models;

namespace NoteVault.Api.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, bool MustChangePassword, UserInfo User);
public record UserInfo(string Id, string Username);
public record RefreshRequest(string RefreshToken);
public record ChangePasswordRequest(string OldPassword, string NewPassword);
public record FolderRequest(string? ParentId, string Name, int Sort);
public record FolderDto(string Id, string? ParentId, string Name, int Sort, DateTime CreatedAt, DateTime UpdatedAt);
public record NoteRequest(string? FolderId, string Title, string Content, string ContentType, bool IsEncrypted, string? Salt, string? Iv, string? CipherText, bool IsPinned, string[] TagIds);
public record NoteListDto(string Id, string? FolderId, string Title, string ContentType, bool IsEncrypted, bool IsPinned, bool IsDeleted, DateTime? LastOpenedAt, DateTime CreatedAt, DateTime UpdatedAt, IEnumerable<TagDto> Tags);
public record NoteDetailDto(string Id, string? FolderId, string Title, string Content, string ContentType, bool IsEncrypted, string? Salt, string? Iv, string? CipherText, bool IsPinned, bool IsDeleted, DateTime? LastOpenedAt, DateTime CreatedAt, DateTime UpdatedAt, IEnumerable<TagDto> Tags, IEnumerable<AttachmentDto> Attachments);
public record TagRequest(string Name, string Color);
public record TagDto(string Id, string Name, string Color, DateTime CreatedAt);
public record AttachmentDto(string Id, string? NoteId, string FileName, string FileKey, string FileUrl, string Bucket, long FileSize, string MimeType, DateTime CreatedAt);

public static class Mappers
{
    public static FolderDto ToDto(this Folder f) => new(f.Id, f.ParentId, f.Name, f.Sort, f.CreatedAt, f.UpdatedAt);
    public static TagDto ToDto(this Tag t) => new(t.Id, t.Name, t.Color, t.CreatedAt);
    public static AttachmentDto ToDto(this Attachment a) => new(a.Id, a.NoteId, a.FileName, a.FileKey, a.FileUrl, a.Bucket, a.FileSize, a.MimeType, a.CreatedAt);
}
