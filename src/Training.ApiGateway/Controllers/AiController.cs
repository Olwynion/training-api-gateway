using Microsoft.AspNetCore.Mvc;
using Training.AI.Proto;
using Training.Training.Proto;

namespace Training.ApiGateway.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly AiService.AiServiceClient _ai;

    public AiController(AiService.AiServiceClient ai)
    {
        _ai = ai;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateBody body)
    {
        var request = new GeneratePlanRequest
        {
            UserId = body.UserId, Prompt = body.Prompt,
            DaysPerWeek = body.DaysPerWeek, ProgramType = body.ProgramType,
            FocusGroup = body.FocusGroup
        };
        request.Exercises.AddRange(body.Exercises);

        var response = await _ai.GeneratePlanAsync(request);
        return Ok(new
        {
            id = response.Id, plan_name = response.PlanName,
            plan_json = response.PlanJson, created_at = response.CreatedAt
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] string user_id)
    {
        var response = await _ai.GetGenerationHistoryAsync(new GetGenerationHistoryRequest { UserId = user_id });
        return Ok(response.Items.Select(i => new
        {
            id = i.Id, plan_name = i.PlanName,
            prompt = i.Prompt, created_at = i.CreatedAt
        }));
    }

    public record GenerateBody(string UserId, string Prompt, List<ExerciseTemplate> Exercises, int DaysPerWeek = 3, string ProgramType = "fullbody", MuscleGroup FocusGroup = MuscleGroup.Unspecified);
}
