using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoticeBoard.API.DTOs;
using NoticeBoard.API.Models;
using NoticeBoard.API.Repositories;
using System.Security.Claims;

namespace NoticeBoard.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly AnnouncementRepository _repo;

    public AnnouncementsController(AnnouncementRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Announcement>>> GetAll()
    {
        var result = await _repo.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var announcement = await _repo.GetByIdAsync(id);

        if (announcement == null)
            return NotFound();

        return Ok(announcement);
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Category = dto.Category,
            SubCategory = dto.SubCategory,
            CreatedDate = DateTime.UtcNow,
            UserId = userId
        };

        await _repo.InsertAsync(announcement);

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var announcement = new Announcement
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Category = dto.Category,
            SubCategory = dto.SubCategory,
            CreatedDate = DateTime.UtcNow,
            UserId = userId
        };

        var result = await _repo.UpdateAsync(announcement);

        if (!result)
            return Forbid();

        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var result = await _repo.DeleteAsync(id, userId);

        if (!result)
            return Forbid();

        return Ok();
    }

}
