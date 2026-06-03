using Microsoft.AspNetCore.Http;
using Moq;
using Training.ApiGateway.Middleware;

namespace Training.ApiGateway.Tests;

public class JwtMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SkipsAuth_ForAuthEndpoints()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/auth/login";
        var nextCalled = false;

        var middleware = new JwtMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Returns401_WhenNoAuthHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/training/plans";

        var middleware = new JwtMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Returns401_WhenAuthHeaderIsNotBearer()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/training/plans";
        context.Request.Headers.Authorization = "Basic token";

        var middleware = new JwtMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_SetsAccessToken_WhenBearerProvided()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/training/plans";
        context.Request.Headers.Authorization = "Bearer test-token";

        HttpContext captured = null!;
        var middleware = new JwtMiddleware(ctx => { captured = ctx; return Task.CompletedTask; });
        await middleware.InvokeAsync(context);

        Assert.Equal("test-token", captured.Items["AccessToken"]);
    }
}
