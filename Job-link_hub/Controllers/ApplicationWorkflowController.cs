using JobLinkHub.API.Models.Applications;
using JobLinkHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/application-workflow")]
public class ApplicationWorkflowController(
    IApplicationWorkflowService applicationWorkflowService,
    IUserContextService userContextService) : ControllerBase
{
    [HttpPost("apply")]
    [Authorize(Roles = "CANDIDATE")]
    public async Task<IActionResult> Apply(ApplyRequest request)
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await applicationWorkflowService.ApplyAsync(user.Id, request.OpportunityId, request.CoverLetter, request.ResumeUsed);
        if (!result.Success) return BadRequest(result.Message);
        return Ok(result.Application);
    }

    [HttpPut("{applicationId:long}/withdraw")]
    [Authorize(Roles = "CANDIDATE")]
    public async Task<IActionResult> Withdraw(long applicationId)
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await applicationWorkflowService.WithdrawAsync(user.Id, applicationId);
        return result.Success ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPut("{applicationId:long}/status")]
    [Authorize(Roles = "EMPLOYER")]
    public async Task<IActionResult> UpdateStatus(long applicationId, UpdateApplicationStatusRequest request)
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await applicationWorkflowService.UpdateStatusAsync(user.Id, applicationId, request.NewStatus, request.RejectionReason);
        return result.Success ? Ok(result.Application) : BadRequest(result.Message);
    }
}
