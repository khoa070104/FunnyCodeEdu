using FuCommunityWebModels.Models;
using FuCommunityWebModels.ViewModels.FuCommunityWebModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.ViewModels
{
    public class CourseDetailVM
    {
        public Course Course { get; set; } // Course information
        public List<int> EnrolledCourses { get; set; } // List of enrolled course IDs
        public decimal UserPoints { get; set; }

        // Lesson Management
        public List<Lesson> Lessons { get; set; } = new List<Lesson>();
        public CreateLessonVM CreateLessonVM { get; set; } = new CreateLessonVM();
        public EditLessonVM EditLessonVM { get; set; } = new EditLessonVM();
        public bool ShowCreateLessonModal { get; set; }
        public bool ShowEditLessonModal { get; set; }
        public int? EditLessonID { get; set; } 
        public string CategoryName => Course?.Category?.CategoryName;
        public List<Review> Reviews { get; set; } = new List<Review>(); 
        public bool HasReviewed { get; set; }
        public bool IsEnrolled { get; set; }
    }
}
