using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NoticeBoard.API.DTOs;
using NoticeBoard.API.Interfaces;
using NoticeBoard.API.Models;
using System.Security.Claims;

namespace NoticeBoard.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementRepository _repo;
    private readonly UserManager<IdentityUser> _userManager;

    public AnnouncementsController(IAnnouncementRepository repo, UserManager<IdentityUser> userManager)
    {
        _repo = repo;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAll()
    {
        var announcements = await _repo.GetAllAsync();
        var result = new List<AnnouncementDto>();

        foreach (var announcement in announcements)
        {
            var user = await _userManager.FindByIdAsync(announcement.UserId);
            result.Add(new AnnouncementDto
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Description = announcement.Description,
                CreatedDate = announcement.CreatedDate,
                Status = announcement.Status,
                Category = announcement.Category,
                SubCategory = announcement.SubCategory,
                AuthorEmail = user?.Email ?? string.Empty,
                AuthorName = user?.UserName ?? string.Empty
            });
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var announcement = await _repo.GetByIdAsync(id);

        if (announcement == null)
            return NotFound();

        var user = await _userManager.FindByIdAsync(announcement.UserId);

        var result = new AnnouncementDto
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Description = announcement.Description,
            CreatedDate = announcement.CreatedDate,
            Status = announcement.Status,
            Category = announcement.Category,
            SubCategory = announcement.SubCategory,
            AuthorEmail = user?.Email ?? string.Empty,
            AuthorName = user?.UserName ?? string.Empty
        };

        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Category = dto.Category,
            SubCategory = dto.SubCategory,
            CreatedDate = DateTime.UtcNow.AddHours(3),
            UserId = userId
        };

        await _repo.InsertAsync(announcement);

        return Ok(new { id = announcement.Id });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var existAnn = await _repo.GetByIdAsync(id);
        if (existAnn == null)
            return NotFound();

        var announcement = new Announcement
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Category = dto.Category,
            SubCategory = dto.SubCategory,
            CreatedDate = existAnn.CreatedDate,
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
