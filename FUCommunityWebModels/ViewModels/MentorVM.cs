using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuCommunityWebModels.Models;

namespace FuCommunityWebModels.ViewModels
{
    public class MentorVM
    {
        public List<ApplicationUser> TopMentors { get; set; }
        public List<ApplicationUser> OtherMentors { get; set; }
    }
}
