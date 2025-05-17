using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NoticeBoard.Web.Models
{
    public class CreateAnnouncementViewModel
    {
        [Required(ErrorMessage = "Поле 'Назва' є обов'язковим.")]
        [MaxLength(150, ErrorMessage = "Максимальна довжина назви — 150 символів.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Поле 'Опис' є обов'язковим.")]
        [MaxLength(1000, ErrorMessage = "Максимальна довжина назви — 1000 символів.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Поле 'Статус' є обов'язковим.")]
        public bool Status { get; set; }

        [Required(ErrorMessage = "Поле 'Категорія' є обов'язковим.")]
        [MaxLength(50, ErrorMessage = "Максимальна довжина назви — 50 символів.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Поле 'Підкатегорія' є обов'язковим.")]
        [MaxLength(50, ErrorMessage = "Максимальна довжина назви — 50 символів.")]
        public string SubCategory { get; set; }


        [BindNever]
        public List<SelectListItem>? Categories { get; set; }

        [BindNever]
        public List<SelectListItem>? SubCategories { get; set; }
    }
}
