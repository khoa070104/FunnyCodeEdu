using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FuCommunityWebModels.Models;

namespace FUCommunityWeb.Components
{
    public class ChatButtonViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatButtonViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return View(user);
        }
    }
} 