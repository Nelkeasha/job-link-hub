using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _service;
    public SkillsController(ISkillService service) { _service = service; }

    // GET api/skills
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    // GET api/skills/grouped
    [HttpGet("grouped")]
    public async Task<IActionResult> GetGrouped()
        => Ok(await _service.GetGroupedByCategoryAsync());

    // GET api/skills/categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await _service.GetCategoriesAsync());

    // GET api/skills/category/{category}
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
        => Ok(await _service.GetByCategoryAsync(category));

    // GET api/skills/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var skill = await _service.GetByIdAsync(id);
        if (skill == null) return NotFound(new { message = "Skill not found" });
        return Ok(skill);
    }

    // POST api/skills
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateSkillDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/skills/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateSkillDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound(new { message = "Skill not found" });
        return Ok(updated);
    }

    // DELETE api/skills/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(new { message = "Skill not found" });
        return Ok(new { message = "Skill deleted" });
    }
}
