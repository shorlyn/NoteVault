using SqlSugar;

namespace NoteVault.Api.Models;

public abstract class EntityBase
{
    [SugarColumn(IsPrimaryKey = true)]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
}

public class User : EntityBase
{
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool MustChangePassword { get; set; } = true;
    [SugarColumn(IsNullable = true)]
    public string? RefreshTokenHash { get; set; }
    [SugarColumn(IsNullable = true)]
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Folder : EntityBase
{
    [SugarColumn(IsNullable = true)]
    public string? ParentId { get; set; }
    public string Name { get; set; } = "";
    public int Sort { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Note : EntityBase
{
    [SugarColumn(IsNullable = true)]
    public string? FolderId { get; set; }
    public string Title { get; set; } = "Untitled";
    [SugarColumn(ColumnDataType = "TEXT")]
    public string Content { get; set; } = "";
    public string ContentType { get; set; } = "markdown";
    public bool IsEncrypted { get; set; }
    [SugarColumn(IsNullable = true)]
    public string? Salt { get; set; }
    [SugarColumn(IsNullable = true)]
    public string? Iv { get; set; }
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? CipherText { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }
    [SugarColumn(IsNullable = true)]
    public DateTime? LastOpenedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Tag : EntityBase
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#5B8DEF";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NoteTag
{
    [SugarColumn(IsPrimaryKey = true)]
    public string NoteId { get; set; } = "";
    [SugarColumn(IsPrimaryKey = true)]
    public string TagId { get; set; } = "";
}

public class Attachment : EntityBase
{
    [SugarColumn(IsNullable = true)]
    public string? NoteId { get; set; }
    public string FileName { get; set; } = "";
    public string FileKey { get; set; } = "";
    public string FileUrl { get; set; } = "";
    public string Bucket { get; set; } = "";
    public long FileSize { get; set; }
    public string MimeType { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
