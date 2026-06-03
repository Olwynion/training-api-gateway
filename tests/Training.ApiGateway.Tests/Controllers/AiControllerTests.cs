using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Training.AI.Proto;
using Training.ApiGateway.Controllers;

namespace Training.ApiGateway.Tests;

public class AiControllerTests
{
    private static AsyncUnaryCall<T> Call<T>(T response) => new(
        Task.FromResult(response), Task.FromResult(new Metadata()),
        () => Status.DefaultSuccess, () => new Metadata(), () => { });

    [Fact]
    public async Task Generate_ReturnsOk()
    {
        var mock = new Mock<AiService.AiServiceClient>(MockBehavior.Strict);
        mock.Setup(c => c.GeneratePlanAsync(It.IsAny<GeneratePlanRequest>(), null, null, default))
            .Returns(Call(new GeneratePlanResponse { Id = 1, PlanName = "Test", PlanJson = "{}", CreatedAt = "2026-01-01" }));

        var controller = new AiController(mock.Object);
        var result = await controller.Generate(
            new AiController.GenerateBody("user1", "test prompt", []));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetHistory_ReturnsOk()
    {
        var mock = new Mock<AiService.AiServiceClient>(MockBehavior.Strict);
        var resp = new GetGenerationHistoryResponse();
        resp.Items.Add(new GenerationHistoryItem { Id = 1, PlanName = "P", Prompt = "prompt", CreatedAt = "2026-01-01" });
        mock.Setup(c => c.GetGenerationHistoryAsync(It.IsAny<GetGenerationHistoryRequest>(), null, null, default))
            .Returns(Call(resp));

        var controller = new AiController(mock.Object);
        var result = await controller.GetHistory("user1");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
