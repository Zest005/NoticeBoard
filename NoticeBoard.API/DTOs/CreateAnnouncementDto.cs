using System.ComponentModel.DataAnnotations;

namespace NoticeBoard.API.DTOs
{
    public class CreateAnnouncementDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public bool Status { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Category { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string SubCategory { get; set; }
    }
}
