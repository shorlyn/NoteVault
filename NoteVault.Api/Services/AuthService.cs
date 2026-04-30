using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NoteVault.Api.DTOs;
using NoteVault.Api.Models;
using SqlSugar;

namespace NoteVault.Api.Services;

public class AuthService(SqlSugarClient db, IConfiguration config)
{
    private readonly JwtOptions _jwt = config.GetSection("Jwt").Get<JwtOptions>() ?? new();

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var user = await db.Queryable<User>().FirstAsync(x => x.Username == username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
        return await IssueTokensAsync(user);
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken)
    {
        var users = await db.Queryable<User>().Where(x => x.RefreshTokenExpiresAt > DateTime.UtcNow).ToListAsync();
        var user = users.FirstOrDefault(x => x.RefreshTokenHash is not null && BCrypt.Net.BCrypt.Verify(refreshToken, x.RefreshTokenHash));
        return user is null ? null : await IssueTokensAsync(user);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await db.Queryable<User>().FirstAsync(x => x.Id == userId);
        if (user is null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash)) return false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        await db.Updateable(user).ExecuteCommandAsync();
        return true;
    }

    private async Task<LoginResponse> IssueTokensAsync(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id), new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Name, user.Username) };
        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes), signingCredentials: credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
        user.UpdatedAt = DateTime.UtcNow;
        await db.Updateable(user).ExecuteCommandAsync();
        return new LoginResponse(accessToken, refreshToken, user.MustChangePassword, new UserInfo(user.Id, user.Username));
    }
}
