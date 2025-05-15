namespace NoticeBoard.API.DTOs
{
    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Status { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorName { get; set; }
    }
}
