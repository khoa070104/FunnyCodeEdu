using FuCommunityWebModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.ViewModels
{
    public class HomeVM
    {
        public List<Course> MostPurchasedCourses { get; set; }
        public List<Course> HighestQualityCourse { get; set; }
        public List<int> EnrolledCourses { get; set; }
        public decimal UserPoints { get; set; }
        public Dictionary<int, double> AverageRatings { get; set; } = new Dictionary<int, double>();
        public Dictionary<int, int> ReviewCounts { get; set; } = new Dictionary<int, int>();
    }
}
