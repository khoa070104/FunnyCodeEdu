using FuCommunityWebModels.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.ViewModels
{
    public class EditPostVM
    {
        public int PostID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int? CategoryID { get; set; }
        public string Tag { get; set; }
        public int Type { get; set; }

        [Display(Name = "Post Image")]
        public IFormFile PostImageFile { get; set; }
        public List<Category> Categories { get; set; }
    }
}
