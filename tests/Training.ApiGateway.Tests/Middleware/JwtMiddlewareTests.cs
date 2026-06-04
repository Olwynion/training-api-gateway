using Microsoft.AspNetCore.Http;
using Training.ApiGateway.Middleware;
using Training.ApiGateway.Services;

namespace Training.ApiGateway.Tests;

public class JwtMiddlewareTests
{
    private static readonly JwtTokenService Jwt = new("test-secret-key-at-least-32-characters-long-for-hmac");

    [Fact]
    public async Task InvokeAsync_SkipsAuth_ForAuthEndpoints()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/auth/login";
        var nextCalled = false;

        var middleware = new JwtMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, Jwt);
        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_Returns401_WhenNoAuthHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/training/plans";

        var middleware = new JwtMiddleware(_ => Task.CompletedTask, Jwt);
        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Returns401_WhenAuthHeaderIsNotBearer()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/training/plans";
        context.Request.Headers.Authorization = "Basic token";

        var middleware = new JwtMiddleware(_ => Task.CompletedTask, Jwt);
        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Returns401_WhenTokenIsInvalid()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/training/plans";
        context.Request.Headers.Authorization = "Bearer invalid-token";

        var middleware = new JwtMiddleware(_ => Task.CompletedTask, Jwt);
        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_SetsAccessToken_WhenBearerProvided()
    {
        var (token, _) = Jwt.GenerateAccessToken(new Models.User { Id = 1, Email = "t@t.com", Name = "T" });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/training/plans";
        context.Request.Headers.Authorization = $"Bearer {token}";

        HttpContext captured = null!;
        var middleware = new JwtMiddleware(ctx => { captured = ctx; return Task.CompletedTask; }, Jwt);
        await middleware.InvokeAsync(context);

        Assert.Equal(token, captured.Items["AccessToken"]);
        Assert.Equal("1", captured.Items["UserId"]);
    }
}
