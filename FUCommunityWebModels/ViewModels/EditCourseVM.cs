using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuCommunityWebModels.ViewModels
{
    public class EditCourseVM
    {
        public int CourseID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [StringLength(255)]
        public string? CourseImage { get; set; }

        [NotMapped]
        [Display(Name = "Course Image")]
        public IFormFile? CourseImageFile { get; set; }
        [Required]
        [Range(1, 9, ErrorMessage = "Please select a semester.")]
        public int Semester { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryID { get; set; }

        [NotMapped]
        [Display(Name = "Document File")]
        public IFormFile? DocumentFile { get; set; }

        public Document Document { get; set; }
    }
}