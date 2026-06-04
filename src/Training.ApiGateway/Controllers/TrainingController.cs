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

    [HttpPut("exercises/{id:long}")]
    public async Task<IActionResult> UpdateExercise(long id, [FromBody] UpdateExerciseBody body)
    {
        await _training.UpdateExerciseAsync(new UpdateExerciseRequest
        {
            Id = id, Name = body.Name, DefaultOneRm = body.DefaultOneRm,
            MuscleGroup = body.MuscleGroup, UserId = body.UserId
        });
        return Ok();
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

    [HttpGet("preferences/{userId}")]
    public async Task<IActionResult> GetPreferences(string userId)
    {
        var response = await _training.GetUserPreferencesAsync(new GetUserPreferencesRequest { UserId = userId });
        if (response.Preferences == null)
            return NotFound();
        return Ok(response.Preferences);
    }

    [HttpPost("preferences")]
    public async Task<IActionResult> SavePreferences([FromBody] SavePreferencesBody body)
    {
        await _training.SavePreferencesAsync(new SavePreferencesRequest
        {
            UserId = body.UserId, DaysPerWeek = body.DaysPerWeek,
            ProgramType = body.ProgramType, FocusGroup = body.FocusGroup
        });
        return Ok();
    }

    [HttpPost("one-rm")]
    public async Task<IActionResult> SaveOneRm([FromBody] SaveOneRmBody body)
    {
        var request = new SaveOneRmsRequest { UserId = body.UserId };
        foreach (var e in body.Entries)
            request.Entries.Add(new OneRmEntry { ExerciseId = e.ExerciseId, OneRm = e.OneRm });
        await _training.SaveOneRmsAsync(request);
        return Ok();
    }

    [HttpGet("exercises/built-in")]
    public async Task<IActionResult> GetBuiltInExercises()
    {
        var response = await _training.GetBuiltInExercisesAsync(new GetBuiltInExercisesRequest());
        return Ok(response.Exercises.Select(e => new
        {
            id = e.Id, name = e.Name, default_one_rm = e.DefaultOneRm,
            muscle_group = e.MuscleGroup, is_built_in = e.IsBuiltIn
        }));
    }

    [HttpPut("plans/{planId:long}/days")]
    public async Task<IActionResult> UpdatePlanDays(long planId, [FromBody] UpdatePlanDaysBody body)
    {
        var request = new UpdatePlanDaysRequest { PlanId = planId, UserId = body.UserId };
        foreach (var d in body.Days)
        {
            var day = new PlanDayUpdate
            {
                Id = d.Id,
                DayName = d.DayName,
                FocusGroup = d.FocusGroup,
                SortOrder = d.SortOrder
            };
            if (d.Exercises != null)
                foreach (var e in d.Exercises)
                    day.Exercises.Add(new DayExerciseUpdate
                    {
                        Id = e.Id,
                        ExerciseId = e.ExerciseId,
                        Sets = e.Sets,
                        SortOrder = e.SortOrder
                    });
            request.Days.Add(day);
        }
        var response = await _training.UpdatePlanDaysAsync(request);
        return Ok(new { id = response.Plan.Id, days = response.Plan.Days });
    }

    public record CreateExerciseBody(string Name, double DefaultOneRm, MuscleGroup MuscleGroup, string UserId);
    public record UpdateExerciseBody(string Name, double DefaultOneRm, MuscleGroup MuscleGroup, string UserId);
    public record CreatePlanBody(string UserId, string Name);
    public record SetCycleBody(int CycleNumber, string UserId);
    public record ProgressBody(string UserId);
    public record SavePreferencesBody(string UserId, int DaysPerWeek, string ProgramType, MuscleGroup FocusGroup);
    public record SaveOneRmBody(string UserId, List<OneRmEntryDto> Entries);
    public record OneRmEntryDto(long ExerciseId, double OneRm);
    public record UpdatePlanDaysBody(string UserId, List<PlanDayDto> Days);
    public record PlanDayDto(long Id, string DayName, MuscleGroup FocusGroup, int SortOrder, List<ExerciseDto>? Exercises);
    public record ExerciseDto(long Id, long ExerciseId, int Sets, int SortOrder);
}
