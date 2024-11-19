using FuCommunityWebModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.ViewModels
{
    public class ManageUserVM
    {
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
