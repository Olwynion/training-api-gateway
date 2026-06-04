using Training.ApiGateway.Models;
using Training.ApiGateway.Repositories;

namespace Training.ApiGateway.Services;

public class AuthService(
    IUserRepository userRepository,
    JwtTokenService jwtTokenService)
{
    public async Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)> RegisterAsync(string email, string password, string name)
    {
        var existing = await userRepository.GetByEmailAsync(email);
        if (existing != null)
            throw new InvalidOperationException("User with this email already exists");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        user.Id = await userRepository.CreateAsync(user);

        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        await userRepository.CreateRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        });

        return (user, accessToken, refreshToken, expiresAt);
    }

    public async Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)> LoginAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email)
            ?? throw new UnauthorizedAccessException("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        await userRepository.CreateRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        });

        return (user, accessToken, refreshToken, expiresAt);
    }

    public async Task<(User user, string accessToken, string refreshToken, DateTime expiresAt)> RefreshTokenAsync(string refreshToken)
    {
        var stored = await userRepository.GetRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        if (stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired or revoked");

        var user = await userRepository.GetByIdAsync(stored.UserId)
            ?? throw new UnauthorizedAccessException("User not found");

        await userRepository.RevokeRefreshTokenAsync(user.Id);

        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        await userRepository.CreateRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        });

        return (user, accessToken, newRefreshToken, expiresAt);
    }

    public async Task LogoutAsync(long userId)
    {
        await userRepository.RevokeRefreshTokenAsync(userId);
    }

    public System.Security.Claims.ClaimsPrincipal? ValidateToken(string token)
        => jwtTokenService.ValidateToken(token);
}
