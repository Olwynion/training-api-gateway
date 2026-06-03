namespace Training.ApiGateway.Middleware;

public class JwtMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
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
        context.Items["AccessToken"] = token;
        await next(context);
    }
}
