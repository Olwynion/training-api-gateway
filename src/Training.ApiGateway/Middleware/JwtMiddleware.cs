using Training.ApiGateway.Services;

namespace Training.ApiGateway.Middleware;

public class JwtMiddleware(RequestDelegate next, JwtTokenService jwtTokenService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Options)
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Missing or invalid Authorization header" });
            return;
        }

        var token = authHeader["Bearer ".Length..];
        var principal = jwtTokenService.ValidateToken(token);
        if (principal == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or expired token" });
            return;
        }

        context.Items["AccessToken"] = token;
        context.Items["UserId"] = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await next(context);
    }
}
