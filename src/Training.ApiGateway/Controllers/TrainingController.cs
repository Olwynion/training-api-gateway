using Microsoft.AspNetCore.Mvc;
using Training.Training.Proto;

namespace Training.ApiGateway.Controllers;

[ApiController]
[Route("api/training")]
public class TrainingController : ControllerBase
{
    private readonly TrainingService.TrainingServiceClient _training;

    public TrainingController(TrainingService.TrainingServiceClient training)
    {
        _training = training;
    }

    [HttpGet("exercises")]
    public async Task<IActionResult> GetExercises([FromQuery] string user_id)
    {
        var response = await _training.GetExercisesAsync(new GetExercisesRequest { UserId = user_id });
        return Ok(response.Exercises.Select(e => new
        {
            id = e.Id, name = e.Name, default_one_rm = e.DefaultOneRm,
            muscle_group = e.MuscleGroup, user_id = e.UserId, is_built_in = e.IsBuiltIn
        }));
    }

    [HttpPost("exercises")]
    public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseBody body)
    {
        var response = await _training.CreateExerciseAsync(new CreateExerciseRequest
        {
            Name = body.Name, DefaultOneRm = body.DefaultOneRm,
            MuscleGroup = body.MuscleGroup, UserId = body.UserId
        });
        return Ok(new { id = response.Exercise.Id });
    }

    [HttpDelete("exercises/{id:long}")]
    public async Task<IActionResult> DeleteExercise(long id, [FromQuery] string user_id)
    {
        await _training.DeleteExerciseAsync(new DeleteExerciseRequest { Id = id, UserId = user_id });
        return Ok();
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans([FromQuery] string user_id)
    {
        var response = await _training.GetUserWorkoutPlansAsync(new GetUserWorkoutPlansRequest { UserId = user_id });
        return Ok(response.Plans.Select(p => new
        {
            id = p.Id, name = p.Name, cycle_number = p.CycleNumber,
            progress_counter = p.ProgressCounter, user_id = p.UserId
        }));
    }

    [HttpGet("plans/{id:long}")]
    public async Task<IActionResult> GetPlan(long id, [FromQuery] string user_id)
    {
        var response = await _training.GetWorkoutPlanAsync(new GetWorkoutPlanRequest { Id = id, UserId = user_id });
        var plan = response.Plan;
        return Ok(new
        {
            id = plan.Id, name = plan.Name, cycle_number = plan.CycleNumber,
            progress_counter = plan.ProgressCounter, user_id = plan.UserId,
            days = plan.Days.Select(d => new
            {
                id = d.Id, day_name = d.DayName, focus_group = d.FocusGroup, sort_order = d.SortOrder,
                exercises = d.Exercises.Select(e => new
                {
                    id = e.Id, exercise_id = e.ExerciseId,
                    exercise_name = e.ExerciseName, sets = e.Sets, sort_order = e.SortOrder
                })
            })
        });
    }

    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanBody body)
    {
        var response = await _training.CreateWorkoutPlanAsync(new CreateWorkoutPlanRequest
        {
            UserId = body.UserId, Name = body.Name
        });
        return Ok(new { id = response.Plan.Id, name = response.Plan.Name });
    }

    [HttpDelete("plans/{id:long}")]
    public async Task<IActionResult> DeletePlan(long id, [FromQuery] string user_id)
    {
        await _training.DeleteWorkoutPlanAsync(new DeleteWorkoutPlanRequest { Id = id, UserId = user_id });
        return Ok();
    }

    [HttpGet("cycle/{planId:long}")]
    public async Task<IActionResult> GetCycle(long planId, [FromQuery] string user_id)
    {
        var response = await _training.GetCycleDataAsync(new GetCycleDataRequest { PlanId = planId, UserId = user_id });
        var data = response.Data;
        return Ok(new
        {
            plan_id = data.PlanId, cycle_number = data.CycleNumber,
            is_light_week = data.IsLightWeek, progress_counter = data.ProgressCounter,
            days = data.Days.Select(d => new
            {
                day_label = d.DayLabel, focus_group = d.FocusGroup,
                sets = d.Sets.Select(s => new
                {
                    exercise_name = s.ExerciseName, one_rm = s.OneRm, sets = s.Sets,
                    percentage = s.Percentage, reps = s.Reps, working_weight = s.WorkingWeight, is_focus = s.IsFocus
                })
            })
        });
    }

    [HttpPost("cycle/{planId:long}")]
    public async Task<IActionResult> SetCycle(long planId, [FromBody] SetCycleBody body)
    {
        await _training.SetCycleAsync(new SetCycleRequest { PlanId = planId, CycleNumber = body.CycleNumber, UserId = body.UserId });
        return Ok();
    }

    [HttpPost("progress/{planId:long}")]
    public async Task<IActionResult> IncrementProgress(long planId, [FromBody] ProgressBody body)
    {
        await _training.IncrementProgressAsync(new IncrementProgressRequest { PlanId = planId, UserId = body.UserId });
        return Ok();
    }

    public record CreateExerciseBody(string Name, double DefaultOneRm, MuscleGroup MuscleGroup, string UserId);
    public record CreatePlanBody(string UserId, string Name);
    public record SetCycleBody(int CycleNumber, string UserId);
    public record ProgressBody(string UserId);
}
