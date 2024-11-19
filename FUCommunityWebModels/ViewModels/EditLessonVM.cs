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
    namespace FuCommunityWebModels.ViewModels
    {
        public class EditLessonVM
        {
            [Required]
            public int LessonID { get; set; }

            [Required]
            public int CourseID { get; set; } // Ensures the lesson is associated with the correct course

            [Required]
            [StringLength(255)]
            public string Title { get; set; }

            public string Content { get; set; }
        }
    }

}
