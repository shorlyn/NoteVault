using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using NoteVault.Api.DTOs;
using NoteVault.Api.Models;
using NoteVault.Api.Repositories;
using NoteVault.Api.Services;
using SqlSugar;

namespace NoteVault.Api.Extensions;

public static class ApiExtensions
{
    public static void AddNoteVault(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(_ => Database.Create(builder.Configuration));
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<NoteService>();
        builder.Services.AddScoped<UploadService>();
        builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(origin =>
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
            return (uri.Host is "localhost" or "127.0.0.1" or "::1") && uri.Port is >= 5173 and <= 5179;
        })));
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter("login", x => { x.PermitLimit = 5; x.Window = TimeSpan.FromMinutes(1); }));
    }

    public static void MapNoteVault(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest req, AuthService auth) =>
            await auth.LoginAsync(req.Username, req.Password) is { } res ? Results.Ok(res) : Results.Unauthorized()).RequireRateLimiting("login").AllowAnonymous();
        app.MapPost("/api/auth/refresh", async (RefreshRequest req, AuthService auth) =>
            await auth.RefreshAsync(req.RefreshToken) is { } res ? Results.Ok(res) : Results.Unauthorized()).AllowAnonymous();
        app.MapPost("/api/auth/change-password", async (ChangePasswordRequest req, ClaimsPrincipal user, AuthService auth) =>
            await auth.ChangePasswordAsync(user.UserId(), req.OldPassword, req.NewPassword) ? Results.NoContent() : Results.BadRequest()).RequireAuthorization();

        app.MapGet("/api/folders", async (SqlSugarClient db) => (await db.Queryable<Folder>().OrderBy(x => x.Sort).ToListAsync()).Select(x => x.ToDto())).RequireAuthorization();
        app.MapPost("/api/folders", async (FolderRequest req, SqlSugarClient db) =>
        {
            var folder = new Folder { ParentId = req.ParentId, Name = req.Name.Trim(), Sort = req.Sort };
            await db.Insertable(folder).ExecuteCommandAsync();
            return Results.Ok(folder.ToDto());
        }).RequireAuthorization();
        app.MapPut("/api/folders/{id}", async (string id, FolderRequest req, SqlSugarClient db) =>
        {
            var folder = await db.Queryable<Folder>().FirstAsync(x => x.Id == id);
            if (folder is null) return Results.NotFound();
            folder.ParentId = req.ParentId; folder.Name = req.Name.Trim(); folder.Sort = req.Sort; folder.UpdatedAt = DateTime.UtcNow;
            await db.Updateable(folder).ExecuteCommandAsync();
            return Results.Ok(folder.ToDto());
        }).RequireAuthorization();
        app.MapDelete("/api/folders/{id}", async (string id, SqlSugarClient db) =>
        {
            if (await db.Queryable<Folder>().AnyAsync(x => x.ParentId == id)) return Results.BadRequest("Folder has children.");
            await db.Updateable<Note>().SetColumns(x => x.FolderId == null).Where(x => x.FolderId == id).ExecuteCommandAsync();
            return await db.Deleteable<Folder>().Where(x => x.Id == id).ExecuteCommandAsync() > 0 ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();

        app.MapGet("/api/notes", (string? folderId, string? q, string? tagId, bool? deleted, bool? recent, NoteService notes) => notes.ListAsync(folderId, q, tagId, deleted ?? false, recent ?? false)).RequireAuthorization();
        app.MapGet("/api/notes/{id}", async (string id, NoteService notes) => await notes.DetailAsync(id) is { } note ? Results.Ok(note) : Results.NotFound()).RequireAuthorization();
        app.MapPost("/api/notes", (NoteRequest req, NoteService notes) => notes.CreateAsync(req)).RequireAuthorization();
        app.MapPut("/api/notes/{id}", async (string id, NoteRequest req, NoteService notes) => await notes.UpdateAsync(id, req) is { } note ? Results.Ok(note) : Results.NotFound()).RequireAuthorization();
        app.MapDelete("/api/notes/{id}", async (string id, NoteService notes) => await notes.SoftDeleteAsync(id) ? Results.NoContent() : Results.NotFound()).RequireAuthorization();
        app.MapPost("/api/notes/{id}/restore", async (string id, NoteService notes) => await notes.RestoreAsync(id) ? Results.NoContent() : Results.NotFound()).RequireAuthorization();
        app.MapDelete("/api/notes/{id}/hard", async (string id, NoteService notes) => await notes.HardDeleteAsync(id) ? Results.NoContent() : Results.NotFound()).RequireAuthorization();
        app.MapPost("/api/notes/{id}/open", async (string id, NoteService notes) => await notes.OpenAsync(id) ? Results.NoContent() : Results.NotFound()).RequireAuthorization();

        app.MapGet("/api/tags", async (SqlSugarClient db) => (await db.Queryable<Tag>().OrderBy(x => x.CreatedAt, OrderByType.Desc).ToListAsync()).Select(x => x.ToDto())).RequireAuthorization();
        app.MapPost("/api/tags", async (TagRequest req, SqlSugarClient db) =>
        {
            var tag = new Tag { Name = req.Name.Trim(), Color = req.Color };
            await db.Insertable(tag).ExecuteCommandAsync();
            return Results.Ok(tag.ToDto());
        }).RequireAuthorization();
        app.MapDelete("/api/tags/{id}", async (string id, SqlSugarClient db) =>
        {
            await db.Deleteable<NoteTag>().Where(x => x.TagId == id).ExecuteCommandAsync();
            return await db.Deleteable<Tag>().Where(x => x.Id == id).ExecuteCommandAsync() > 0 ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();

        app.MapPost("/api/upload", async (IFormFile file, string? noteId, UploadService upload) => Results.Ok(await upload.UploadAsync(file, noteId))).DisableAntiforgery().RequireAuthorization();
    }

    public static string UserId(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? throw new UnauthorizedAccessException();
}
