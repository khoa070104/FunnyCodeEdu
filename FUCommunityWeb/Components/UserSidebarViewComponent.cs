using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FuCommunityWebModels.Models;
using System.Security.Claims;

public class UserSidebarViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserSidebarViewComponent(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var claimsPrincipal = User as ClaimsPrincipal;
        var userId = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        return View(user);
    }
}