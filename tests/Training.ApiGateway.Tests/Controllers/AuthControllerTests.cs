using Microsoft.AspNetCore.Mvc;
using Moq;
using Training.ApiGateway.Controllers;
using Training.ApiGateway.Services;

namespace Training.ApiGateway.Tests;

public class AuthControllerTests
{
    private Mock<AuthService> CreateMockAuth()
    {
        var mockUserRepo = new Mock<Repositories.IUserRepository>();
        var jwtService = new JwtTokenService("test-secret-key-at-least-32-characters-long-for-hmac");
        return new Mock<AuthService>(mockUserRepo.Object, jwtService) { CallBase = true };
    }

    [Fact]
    public async Task Register_ReturnsOk()
    {
        var mockAuth = CreateMockAuth();
        mockAuth.Setup(a => a.RegisterAsync("test@test.com", "pass", "Test"))
            .ReturnsAsync((new Models.User { Id = 1, Email = "test@test.com", Name = "Test" }, "at", "rt", DateTime.UtcNow.AddHours(1)));

        var controller = new AuthController(mockAuth.Object);
        var result = await controller.Register(new AuthController.RegisterBody("test@test.com", "pass", "Test"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Login_ReturnsOk()
    {
        var mockAuth = CreateMockAuth();
        mockAuth.Setup(a => a.LoginAsync("t@t.com", "pass"))
            .ReturnsAsync((new Models.User { Id = 1, Email = "t@t.com", Name = "T" }, "at", "rt", DateTime.UtcNow.AddHours(1)));

        var controller = new AuthController(mockAuth.Object);
        var result = await controller.Login(new AuthController.LoginBody("t@t.com", "pass"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        var mockAuth = CreateMockAuth();

        var controller = new AuthController(mockAuth.Object);
        var result = await controller.Logout(new AuthController.LogoutBody(1));

        Assert.IsType<OkResult>(result);
    }
}
