using NoticeBoard.API.Models;

namespace NoticeBoard.API.Interfaces
{
    public interface IAnnouncementRepository
    {
        Task<List<Announcement>> GetAllAsync();
        Task<Announcement>? GetByIdAsync(Guid id);
        Task InsertAsync(Announcement announcement);
        Task<bool> UpdateAsync(Announcement announcement);
        Task<bool> DeleteAsync(Guid id, string userId);
    }
}
