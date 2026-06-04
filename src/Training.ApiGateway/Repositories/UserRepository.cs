using Dapper;
using Npgsql;
using Training.ApiGateway.Models;

namespace Training.ApiGateway.Repositories;

public class UserRepository(string connectionString) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT id, email, password_hash as PasswordHash, name, created_at as CreatedAt FROM users WHERE email = @Email",
            new { Email = email });
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT id, email, password_hash as PasswordHash, name, created_at as CreatedAt FROM users WHERE id = @Id",
            new { Id = id });
    }

    public async Task<long> CreateAsync(User user)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return await conn.QuerySingleAsync<long>(
            "INSERT INTO users (email, password_hash, name, created_at) VALUES (@Email, @PasswordHash, @Name, @CreatedAt) RETURNING id",
            user);
    }

    public async Task CreateRefreshTokenAsync(RefreshToken token)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.ExecuteAsync(
            "INSERT INTO refresh_tokens (user_id, token, expires_at, created_at, is_revoked) VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt, @IsRevoked)",
            token);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        return await conn.QueryFirstOrDefaultAsync<RefreshToken>(
            "SELECT id, user_id as UserId, token, expires_at as ExpiresAt, created_at as CreatedAt, is_revoked as IsRevoked FROM refresh_tokens WHERE token = @Token",
            new { Token = token });
    }

    public async Task RevokeRefreshTokenAsync(long userId)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.ExecuteAsync(
            "UPDATE refresh_tokens SET is_revoked = true WHERE user_id = @UserId",
            new { UserId = userId });
    }
}
