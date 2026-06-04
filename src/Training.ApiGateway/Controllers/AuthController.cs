using Microsoft.AspNetCore.Mvc;
using Training.ApiGateway.Services;

namespace Training.ApiGateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterBody body)
    {
        var (user, accessToken, refreshToken, expiresAt) = await auth.RegisterAsync(body.Email, body.Password, body.Name);
        return Ok(FormatAuth(user.Id, user.Email, user.Name, accessToken, refreshToken, expiresAt));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginBody body)
    {
        var (user, accessToken, refreshToken, expiresAt) = await auth.LoginAsync(body.Email, body.Password);
        return Ok(FormatAuth(user.Id, user.Email, user.Name, accessToken, refreshToken, expiresAt));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshBody body)
    {
        var (user, accessToken, refreshToken, expiresAt) = await auth.RefreshTokenAsync(body.RefreshToken);
        return Ok(FormatAuth(user.Id, user.Email, user.Name, accessToken, refreshToken, expiresAt));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutBody body)
    {
        await auth.LogoutAsync(body.UserId);
        return Ok();
    }

    [HttpPost("validate")]
    public IActionResult Validate([FromBody] ValidateBody body)
    {
        var principal = auth.ValidateToken(body.AccessToken);
        if (principal == null)
            return Ok(new { is_valid = false, is_expired = false, user_id = "", email = "" });
        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        return Ok(new { is_valid = true, is_expired = false, user_id = userId, email = email });
    }

    private static object FormatAuth(long userId, string email, string name, string accessToken, string refreshToken, DateTime expiresAt) => new
    {
        access_token = accessToken, refresh_token = refreshToken,
        user_id = userId.ToString(), email, name,
        expires_at = expiresAt.ToString("O")
    };

    public record RegisterBody(string Email, string Password, string Name);
    public record LoginBody(string Email, string Password);
    public record RefreshBody(string RefreshToken);
    public record LogoutBody(long UserId);
    public record ValidateBody(string AccessToken);
}
