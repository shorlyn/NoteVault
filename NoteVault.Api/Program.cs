using NoteVault.Api.Extensions;
using NoteVault.Api.Repositories;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
builder.AddNoteVault();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SignatureMiddleware>();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<SqlSugarClient>().MigrateAndSeedAsync();
}

app.MapGet("/", () => Results.Ok(new { name = "NoteVault API", status = "ok" })).AllowAnonymous();
app.MapNoteVault();

app.Run();
