using NoteVault.Api.DTOs;
using NoteVault.Api.Models;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using SqlSugar;

namespace NoteVault.Api.Services;

public class UploadService(SqlSugarClient db, IConfiguration config)
{
    private readonly QiniuOptions _options = config.GetSection("Qiniu").Get<QiniuOptions>() ?? new();

    public async Task<AttachmentDto> UploadAsync(IFormFile file, string? noteId)
    {
        if (file.Length <= 0 || file.Length > _options.MaxFileSizeMb * 1024 * 1024) throw new InvalidOperationException("File size is not allowed.");
        if (!_options.AllowedMimeTypes.Contains(file.ContentType)) throw new InvalidOperationException("File type is not allowed.");
        if (string.IsNullOrWhiteSpace(_options.AccessKey) || string.IsNullOrWhiteSpace(_options.SecretKey) || string.IsNullOrWhiteSpace(_options.Bucket) || string.IsNullOrWhiteSpace(_options.Domain)) throw new InvalidOperationException("Qiniu is not configured.");

        var key = $"notevault/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var mac = new Mac(_options.AccessKey, _options.SecretKey);
        var policy = new PutPolicy { Scope = $"{_options.Bucket}:{key}" };
        var token = Auth.CreateUploadToken(mac, policy.ToJsonString());
        var uploader = new FormUploader(new Config { Zone = ZoneHelper.QueryZone(_options.AccessKey, _options.Bucket), UseHttps = true });
        HttpResult result;
        await using (var stream = file.OpenReadStream())
        {
            result = uploader.UploadStream(stream, key, token, null);
        }
        if (result.Code < 200 || result.Code >= 300) throw new InvalidOperationException($"Qiniu upload failed: {result.Text}");

        var attachment = new Attachment
        {
            NoteId = noteId,
            FileName = file.FileName,
            FileKey = key,
            FileUrl = $"{NormalizeDomain(_options.Domain)}/{key}",
            Bucket = _options.Bucket,
            FileSize = file.Length,
            MimeType = file.ContentType
        };
        await db.Insertable(attachment).ExecuteCommandAsync();
        return attachment.ToDto();
    }

    private static string NormalizeDomain(string domain)
    {
        var value = domain.Trim().TrimEnd('/');
        return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? value
            : $"https://{value}";
    }
}
