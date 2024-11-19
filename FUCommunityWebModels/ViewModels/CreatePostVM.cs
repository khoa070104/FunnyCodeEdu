using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuCommunityWebModels.Models;

namespace FuCommunityWebModels.ViewModels
{
    public class CreatePostVM
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }

        public int? CategoryID { get; set; }
        public string UserID { get; set; }
        public int PostID { get; set; }
        public string Tag { get; set; }
        public int Type { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string PostImage { get; set; }
        [NotMapped]
        [Display(Name = "Post Image")]
        public IFormFile PostImageFile { get; set; }
        [NotMapped]
        [Display(Name = "Document File")]
        public IFormFile DocumentFile { get; set; }
        public List<Category> Categories { get; set; }
    }
}
