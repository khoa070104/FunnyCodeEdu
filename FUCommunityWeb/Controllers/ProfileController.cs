using FuCommunityWebModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FuCommunityWebServices.Services;
using System.Threading.Tasks;
using FuCommunityWebModels.ViewModels;
using Microsoft.EntityFrameworkCore;
using FuCommunityWebDataAccess.Data;
using System.IO;

namespace FUCommunityWeb.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserService _userService;
        private readonly OrderService _orderService;
        private UserVM userVM;

        public ProfileController(UserManager<IdentityUser> userManager, UserService userService, OrderService orderService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _userService = userService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(userId);
            userVM = new()
            {
                User = user,
            };
            ViewData["CurrentPage"] = Url.Action("Index");
            return View(userVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAvatar(IFormFile file, string currentPage)
        {
            if (file != null && file.Length > 0)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                var fileName = Path.GetFileName(file.FileName);
                var encryptedFileName = "avt_" + EncryptAvatarFileName(fileName);
                var path = Path.Combine(uploadsDirectory, encryptedFileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var avatarPath = $"/uploads/{encryptedFileName}";
                await _userService.UpdateUserAvatarAsync(userId, avatarPath);
            }

            return Redirect(currentPage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeBanner(IFormFile file, string currentPage)
        {
            if (file != null && file.Length > 0)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                var fileName = Path.GetFileName(file.FileName);
                var encryptedFileName = "banner_" + EncryptBannerFileName(fileName);
                var path = Path.Combine(uploadsDirectory, encryptedFileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var bannerPath = $"/uploads/{encryptedFileName}";
                await _userService.UpdateUserBannerAsync(userId, bannerPath);
            }

            return Redirect(currentPage);
        }

        private string EncryptAvatarFileName(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string fileNameWithAvatar = "avatar_" + fileNameWithoutExtension;
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(fileNameWithAvatar);
            var encryptedFileName = System.Convert.ToBase64String(plainTextBytes);

            return encryptedFileName + fileExtension;
        }

        private string EncryptBannerFileName(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string fileNameWithBanner = "banner_" + fileNameWithoutExtension;
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(fileNameWithBanner);
            var encryptedFileName = System.Convert.ToBase64String(plainTextBytes);

            return encryptedFileName + fileExtension;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserVM userVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(userId);

            // Kiểm tra username đã tồn tại chưa
            if (user.UserName != userVM.User.UserName)
            {
                var existingUser = await _userService.GetUserByUsernameAsync(userVM.User.UserName);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Username is already taken!" });
                }
            }

            user.FullName = userVM.User.FullName;
            user.Bio = userVM.User.Bio;
            user.Address = userVM.User.Address;
            user.DOB = userVM.User.DOB;
            user.Gender = userVM.User.Gender;
            user.Description = userVM.User.Description;
            user.UserName = userVM.User.UserName;
            user.Instagram = userVM.User.Instagram;
            user.Facebook = userVM.User.Facebook;
            user.Github = userVM.User.Github;

            await _userService.UpdateUserAsync(user);
            return Json(new { success = true, message = "Profile updated successfully!" });
        }

        public async Task<IActionResult> PostHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userService.GetUserWithVotesAsync(userId);
            var posts = await _userService.GetUserPostsAsync(userId);

            userVM = new UserVM()
            {
                User = user,
                Post = posts
            };

            ViewData["CurrentPage"] = Url.Action("PostHistory");
            return View(userVM);
        }

        public async Task<IActionResult> CourseHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userService.GetUserWithVotesAsync(userId);
            var enrollments = await _userService.GetUserEnrollmentsAsync(userId);

            userVM = new UserVM()
            {
                User = user,
                Enrollments = enrollments,
            };

            ViewData["CurrentPage"] = Url.Action("CourseHistory");
            return View(userVM);
        }

        public async Task<IActionResult> PaymentHistory()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(userId);
            var orders = await _orderService.GetAllOrdersAsync();

            var userVM = new UserVM
            {
                User = user,
                Orders = orders.Where(o => o.UserID == userId).ToList()
            };

            return View(userVM);
        }
    }
}
