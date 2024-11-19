using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.ViewModels
{
    public class CreateLessonVM
    {
        [Required]
        public int CourseID { get; set; } // Associates the lesson with a course

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Content { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active";
    }

}
