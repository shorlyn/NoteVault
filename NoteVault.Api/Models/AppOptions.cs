namespace NoteVault.Api.Models;

public class JwtOptions
{
    public string Issuer { get; set; } = "NoteVault";
    public string Audience { get; set; } = "NoteVault";
    public string Secret { get; set; } = "change-me-to-a-very-long-secret-at-least-32-bytes";
    public int AccessTokenMinutes { get; set; } = 120;
    public int RefreshTokenDays { get; set; } = 30;
}

public class DatabaseOptions
{
    public string Provider { get; set; } = "Sqlite";
    public string Path { get; set; } = "data/notevault.db";
    public string ConnectionString { get; set; } = "";
}

public class QiniuOptions
{
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string Bucket { get; set; } = "";
    public string Domain { get; set; } = "";
    public string Zone { get; set; } = "auto";
    public long MaxFileSizeMb { get; set; } = 20;
    public string[] AllowedMimeTypes { get; set; } = ["image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf", "text/plain"];
}

public class ApiSignatureOptions
{
    public bool Enabled { get; set; }
    public int TimeWindowSeconds { get; set; } = 300;
}
