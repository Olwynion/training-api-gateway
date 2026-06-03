using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Training.Training.Proto;
using Training.ApiGateway.Controllers;

namespace Training.ApiGateway.Tests;

public class TrainingControllerTests
{
    private static AsyncUnaryCall<T> Call<T>(T response) => new(
        Task.FromResult(response), Task.FromResult(new Metadata()),
        () => Status.DefaultSuccess, () => new Metadata(), () => { });

    [Fact]
    public async Task GetExercises_ReturnsOk()
    {
        var mock = new Mock<TrainingService.TrainingServiceClient>(MockBehavior.Strict);
        var resp = new GetExercisesResponse();
        resp.Exercises.Add(new Exercise { Id = 1, Name = "Жим", DefaultOneRm = 75, MuscleGroup = MuscleGroup.Chest });
        mock.Setup(c => c.GetExercisesAsync(It.IsAny<GetExercisesRequest>(), null, null, default))
            .Returns(Call(resp));

        var controller = new TrainingController(mock.Object);
        var result = await controller.GetExercises("user1");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task CreateExercise_ReturnsOk()
    {
        var mock = new Mock<TrainingService.TrainingServiceClient>(MockBehavior.Strict);
        mock.Setup(c => c.CreateExerciseAsync(It.IsAny<CreateExerciseRequest>(), null, null, default))
            .Returns(Call(new CreateExerciseResponse { Exercise = new Exercise { Id = 1 } }));

        var controller = new TrainingController(mock.Object);
        var result = await controller.CreateExercise(
            new TrainingController.CreateExerciseBody("Жим", 75, MuscleGroup.Chest, "user1"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetPlans_ReturnsOk()
    {
        var mock = new Mock<TrainingService.TrainingServiceClient>(MockBehavior.Strict);
        var resp = new GetUserWorkoutPlansResponse();
        resp.Plans.Add(new WorkoutPlan { Id = 1, Name = "Plan", UserId = "user1" });
        mock.Setup(c => c.GetUserWorkoutPlansAsync(It.IsAny<GetUserWorkoutPlansRequest>(), null, null, default))
            .Returns(Call(resp));

        var controller = new TrainingController(mock.Object);
        var result = await controller.GetPlans("user1");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetCycle_ReturnsOk()
    {
        var mock = new Mock<TrainingService.TrainingServiceClient>(MockBehavior.Strict);
        var resp = new GetCycleDataResponse();
        resp.Data = new CycleData { PlanId = 1, CycleNumber = 1 };
        mock.Setup(c => c.GetCycleDataAsync(It.IsAny<GetCycleDataRequest>(), null, null, default))
            .Returns(Call(resp));

        var controller = new TrainingController(mock.Object);
        var result = await controller.GetCycle(1, "user1");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task DeletePlan_ReturnsOk()
    {
        var mock = new Mock<TrainingService.TrainingServiceClient>(MockBehavior.Strict);
        mock.Setup(c => c.DeleteWorkoutPlanAsync(It.IsAny<DeleteWorkoutPlanRequest>(), null, null, default))
            .Returns(Call(new DeleteWorkoutPlanResponse()));

        var controller = new TrainingController(mock.Object);
        var result = await controller.DeletePlan(1, "user1");

        Assert.IsType<OkResult>(result);
    }
}
