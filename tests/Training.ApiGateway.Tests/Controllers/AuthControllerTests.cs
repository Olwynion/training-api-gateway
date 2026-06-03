using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Training.Auth;
using Training.ApiGateway.Controllers;

namespace Training.ApiGateway.Tests;

public class AuthControllerTests
{
    private static AsyncUnaryCall<T> Call<T>(T response) => new(
        Task.FromResult(response), Task.FromResult(new Metadata()),
        () => Status.DefaultSuccess, () => new Metadata(), () => { });

    [Fact]
    public async Task Register_ReturnsOk()
    {
        var mock = new Mock<AuthService.AuthServiceClient>(MockBehavior.Strict);
        mock.Setup(c => c.RegisterAsync(It.IsAny<global::Training.Auth.RegisterRequest>(), null, null, default))
            .Returns(Call(new RegisterResponse
            {
                AccessToken = "at", RefreshToken = "rt", UserId = 1,
                Email = "test@test.com", Name = "Test", ExpiresAt = 100
            }));

        var controller = new AuthController(mock.Object);
        var result = await controller.Register(new AuthController.RegisterBody("test@test.com", "pass", "Test"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Login_ReturnsOk()
    {
        var mock = new Mock<AuthService.AuthServiceClient>(MockBehavior.Strict);
        mock.Setup(c => c.LoginAsync(It.IsAny<global::Training.Auth.LoginRequest>(), null, null, default))
            .Returns(Call(new LoginResponse { AccessToken = "at", UserId = 1, Email = "t@t.com", Name = "T", ExpiresAt = 100 }));

        var controller = new AuthController(mock.Object);
        var result = await controller.Login(new AuthController.LoginBody("t@t.com", "pass"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Refresh_ReturnsOk()
    {
        var mock = new Mock<AuthService.AuthServiceClient>(MockBehavior.Strict);
        mock.Setup(c => c.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>(), null, null, default))
            .Returns(Call(new RefreshTokenResponse { AccessToken = "new_at" }));

        var controller = new AuthController(mock.Object);
        var result = await controller.Refresh(new AuthController.RefreshBody("old_rt"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        var mock = new Mock<AuthService.AuthServiceClient>(MockBehavior.Strict);
        mock.Setup(c => c.LogoutAsync(It.IsAny<LogoutRequest>(), null, null, default))
            .Returns(Call(new LogoutResponse()));

        var controller = new AuthController(mock.Object);
        var result = await controller.Logout(new AuthController.LogoutBody(1));

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Validate_ReturnsOk()
    {
        var mock = new Mock<AuthService.AuthServiceClient>(MockBehavior.Strict);
        mock.Setup(c => c.ValidateTokenAsync(It.IsAny<ValidateTokenRequest>(), null, null, default))
            .Returns(Call(new ValidateTokenResponse { IsValid = true, UserId = 1, Email = "t@t.com" }));

        var controller = new AuthController(mock.Object);
        var result = await controller.Validate(new AuthController.ValidateBody("token"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
