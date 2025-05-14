using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Create([FromBody] Announcement announcement)
    {
        announcement.Id = Guid.NewGuid();
        announcement.CreatedDate = DateTime.UtcNow;

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        announcement.UserId = userId;

        await _repo.InsertAsync(announcement);

        if (announcement.UserId != userId)
            return Forbid();

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] Announcement updatedAnnouncement)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        updatedAnnouncement.Id = id;
        updatedAnnouncement.UserId = userId;
        updatedAnnouncement.CreatedDate = DateTime.UtcNow;

        var result = await _repo.UpdateAsync(updatedAnnouncement);

        if (!result)
            return Forbid();

        if (updatedAnnouncement.UserId != userId)
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
