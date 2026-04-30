using System.Net;
using System.Security.Cryptography;
using System.Text;
using NoteVault.Api.Models;

namespace NoteVault.Api.Extensions;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API error");
            context.Response.StatusCode = ex is UnauthorizedAccessException ? (int)HttpStatusCode.Unauthorized : (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
    }
}

public class SignatureMiddleware(RequestDelegate next, IConfiguration config)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var options = config.GetSection("ApiSignature").Get<ApiSignatureOptions>() ?? new();
        if (!options.Enabled || context.Request.Path.StartsWithSegments("/api/auth/login"))
        {
            await next(context);
            return;
        }

        var timestamp = context.Request.Headers["x-nv-timestamp"].ToString();
        var bodyHash = context.Request.Headers["x-nv-body-hash"].ToString();
        if (!long.TryParse(timestamp, out var unix) || Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - unix) > options.TimeWindowSeconds)
        {
            context.Response.StatusCode = 401;
            return;
        }

        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        var computed = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        if (!string.Equals(computed, bodyHash, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            return;
        }
        await next(context);
    }
}
