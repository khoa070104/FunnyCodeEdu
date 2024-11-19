using FuCommunityWebModels.Models;
using System.Collections.Generic;

namespace FuCommunityWebModels.ViewModels
{
    public class CourseVM
    {	
		 public List<Course> Courses { get; set; } = new List<Course>();
        public List<int> EnrolledCourses { get; set; } = new List<int>();
        public CreateCourseVM CreateCourseVM { get; set; } = new CreateCourseVM();
        public EditCourseVM EditCourseVM { get; set; } = new EditCourseVM();
        public bool ShowCreateCourseModal { get; set; }
        public bool ShowEditCourseModal { get; set; }
        public int? EditCourseID { get; set; } 

        public List<Category> Categories { get; set; } = new List<Category>();

        public List<string> AllSubjectCodes { get; set; } = new List<string>();
        public string SelectedSemester { get; set; }
        public string SelectedCategory { get; set; }
        public string SelectedSubjectCode { get; set; }
        public string SelectedRate { get; set; }
        public string SelectedMinPrice { get; set; }
        public int TotalLessons { get; set; }
        public bool IsMentor { get; set; }
        public bool IsStudent { get; set; }
        public decimal UserPoints { get; set; }

        public Dictionary<int, double> AverageRatings { get; set; } = new Dictionary<int, double>();
        public Dictionary<int, int> ReviewCounts { get; set; } = new Dictionary<int, int>();
    }
}
