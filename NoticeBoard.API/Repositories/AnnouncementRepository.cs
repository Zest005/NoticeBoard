using Microsoft.Data.SqlClient;
using NoticeBoard.API.Models;
using System.Data;

namespace NoticeBoard.API.Repositories
{
    public class AnnouncementRepository
    {
        private readonly string _connectionString;

        public AnnouncementRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Announcement>> GetAllAsync()
        {
            var announcements = new List<Announcement>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("GetAllAnnouncements", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                announcements.Add(new Announcement
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    CreatedDate = reader.GetDateTime(3),
                    Status = reader.GetBoolean(4),
                    Category = reader.GetString(5),
                    SubCategory = reader.GetString(6),
                    UserId = reader.GetString(7)
                });
            }

            return announcements;
        }

        public async Task InsertAsync(Announcement announcement)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("InsertAnnouncement", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Id", announcement.Id);
            cmd.Parameters.AddWithValue("@Title", announcement.Title);
            cmd.Parameters.AddWithValue("@Description", announcement.Description);
            cmd.Parameters.AddWithValue("@CreatedDate", announcement.CreatedDate);
            cmd.Parameters.AddWithValue("@Status", announcement.Status);
            cmd.Parameters.AddWithValue("@Category", announcement.Category);
            cmd.Parameters.AddWithValue("@SubCategory", announcement.SubCategory);
            cmd.Parameters.AddWithValue("@UserId", announcement.UserId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> UpdateAsync(Announcement announcement)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("UpdateAnnouncement", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Id", announcement.Id);
            cmd.Parameters.AddWithValue("@Title", announcement.Title);
            cmd.Parameters.AddWithValue("@Description", announcement.Description);
            cmd.Parameters.AddWithValue("@CreatedDate", announcement.CreatedDate);
            cmd.Parameters.AddWithValue("@Status", announcement.Status);
            cmd.Parameters.AddWithValue("@Category", announcement.Category);
            cmd.Parameters.AddWithValue("@SubCategory", announcement.SubCategory);
            cmd.Parameters.AddWithValue("@UserId", announcement.UserId);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("DeleteAnnouncement", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<Announcement?> GetByIdAsync(Guid id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("GetByIdAnnouncement", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Announcement
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    CreatedDate = reader.GetDateTime(3),
                    Status = reader.GetBoolean(4),
                    Category = reader.GetString(5),
                    SubCategory = reader.GetString(6),
                    UserId = reader.GetString(7)
                };
            }

            return null;
        }
    }
}
