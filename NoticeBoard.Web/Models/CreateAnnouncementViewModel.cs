using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NoticeBoard.Web.Models
{
    public class CreateAnnouncementViewModel
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


        [BindNever]
        public List<SelectListItem>? Categories { get; set; }

        [BindNever]
        public List<SelectListItem>? SubCategories { get; set; }
    }
}
