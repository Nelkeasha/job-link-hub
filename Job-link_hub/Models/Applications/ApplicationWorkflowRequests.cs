namespace JobLinkHub.API.Models.Applications;

public class ApplyRequest
{
    public long OpportunityId { get; set; }
    public string? CoverLetter { get; set; }
    public string? ResumeUsed { get; set; }
}

public class UpdateApplicationStatusRequest
{
    public string NewStatus { get; set; } = "PENDING";
    public string? RejectionReason { get; set; }
}
