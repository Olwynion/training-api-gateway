using Microsoft.AspNetCore.Mvc;
using AuthProto = global::Training.Auth;

namespace Training.ApiGateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthProto.AuthService.AuthServiceClient _auth;

    public AuthController(AuthProto.AuthService.AuthServiceClient auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterBody body)
    {
        var response = await _auth.RegisterAsync(new global::Training.Auth.RegisterRequest
        {
            Email = body.Email, Password = body.Password, Name = body.Name
        });
        return Ok(FormatAuth(response));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginBody body)
    {
        var response = await _auth.LoginAsync(new global::Training.Auth.LoginRequest
        {
            Email = body.Email, Password = body.Password
        });
        return Ok(FormatAuth(response));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshBody body)
    {
        var response = await _auth.RefreshTokenAsync(new global::Training.Auth.RefreshTokenRequest { RefreshToken = body.RefreshToken });
        return Ok(FormatAuth(response));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutBody body)
    {
        await _auth.LogoutAsync(new global::Training.Auth.LogoutRequest { UserId = body.UserId });
        return Ok();
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateBody body)
    {
        var response = await _auth.ValidateTokenAsync(new global::Training.Auth.ValidateTokenRequest { AccessToken = body.AccessToken });
        return Ok(new { is_valid = response.IsValid, is_expired = response.IsExpired, user_id = response.UserId, email = response.Email });
    }

    private static object FormatAuth(dynamic r) => new
    {
        access_token = r.AccessToken, refresh_token = r.RefreshToken,
        user_id = r.UserId, email = r.Email, name = r.Name, expires_at = r.ExpiresAt
    };

    public record RegisterBody(string Email, string Password, string Name);
    public record LoginBody(string Email, string Password);
    public record RefreshBody(string RefreshToken);
    public record LogoutBody(long UserId);
    public record ValidateBody(string AccessToken);
}
