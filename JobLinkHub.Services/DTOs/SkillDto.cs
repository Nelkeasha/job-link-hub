namespace JobLinkHub.Services.DTOs;

public class SkillDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class CreateSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class UpdateSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class SkillCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public List<SkillDto> Skills { get; set; } = new();
}
