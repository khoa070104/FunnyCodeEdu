using FuCommunityWebModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.ViewModels
{
    public class SearchVM
    {
        public List<Post> Posts { get; set; }
        public List<Category> Categories { get; set; }
        public List<Course> Courses { get; set; }
        public string searchKeyword { get; set; }
    }
}
