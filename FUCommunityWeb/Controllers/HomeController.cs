using FuCommunityWebDataAccess.Data;
using FuCommunityWebModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FuCommunityWebServices.Services;
using FuCommunityWebModels.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using FuCommunityWebModels.ViewModels.FuCommunityWebModels.ViewModels;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace FUCommunityWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserService _userService;
        private readonly HomeService _homeService;
        private readonly CourseService _courseService;
        private readonly ForumService _forumService;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserService userService, HomeService homeService, CourseService courseService, ForumService forumService, ApplicationDbContext context)
        {
            _logger = logger;
            _userService = userService;
            _homeService = homeService;
            _courseService = courseService;
            _forumService = forumService;
            _context = context;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrolledCourses = _courseService.GetEnrolledCoursesAsync(userId).Result;
            var mostPurchasedCourses = _courseService.GetMostPurchasedCoursesAsync(3).Result
                .Where(c => c.Status == "active").ToList();
            var highestQualityCourse = _courseService.GetHighestQualityCoursesAsync(3).Result
                .Where(c => c.Status == "active").ToList();

            var user = _userService.GetUserByIdAsync(userId).Result;
            var userPoints = user?.Point ?? 0;

            var averageRatings = _courseService.GetAverageRatingsAsync().Result;
            var reviewCounts = _courseService.GetReviewCountsAsync().Result;

            var homeViewModel = new HomeVM
            {
                MostPurchasedCourses = mostPurchasedCourses,
                HighestQualityCourse = highestQualityCourse,
                EnrolledCourses = enrolledCourses,
                UserPoints = userPoints,
                AverageRatings = averageRatings,
                ReviewCounts = reviewCounts
            };

            return View(homeViewModel);
        }

        //public IActionResult About()
        //{
        //    return View();
        //}

        //public IActionResult Cart()
        //{
        //    return View();
        //}
        public IActionResult ContactUs()
        {
            return View();
        }

        //public IActionResult CourseHistory()
        //{
        //    return View();
        //}
        //public IActionResult Deposit()
        //{
        //    return View();
        //}
        //public IActionResult EditProfile()
        //{
        //    return View();
        //}

        public IActionResult Home()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrolledCourses = _courseService.GetEnrolledCoursesAsync(userId).Result;
            var mostPurchasedCourses = _courseService.GetMostPurchasedCoursesAsync(3).Result
                .Where(c => c.Status == "active").ToList();
            var highestQualityCourse = _courseService.GetHighestQualityCoursesAsync(3).Result
                .Where(c => c.Status == "active").ToList();

            var homeViewModel = new HomeVM
            {
                MostPurchasedCourses = mostPurchasedCourses,
                HighestQualityCourse = highestQualityCourse,
                EnrolledCourses = enrolledCourses
            };

            return View(homeViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyCourse(int courseId, string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
            {
                TempData["Error"] = "Course not found.";
                return RedirectToAction("Index");
            }

            var alreadyEnrolled = await _courseService.IsUserEnrolledInCourseAsync(userId, courseId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "You are already enrolled in this course.";
                return RedirectToAction("Index");
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            if (user.Point < course.Price)
            {
                TempData["Error"] = "You do not have enough points to purchase this course.";
                return RedirectToAction("Index");
            }

            user.Point -= course.Price.Value;

            var enrollment = new Enrollment
            {
                UserID = userId,
                CourseID = courseId,
                EnrollmentDate = DateTime.Now,
                Status = "Active"
            };

            await _courseService.EnrollUserInCourseAsync(enrollment);
            await _userService.UpdateUserAsync(user);

            TempData["Success"] = "Enrollment successful!";

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult ViewUser(string searchTerm = "")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            List<ApplicationUser> users = new List<ApplicationUser>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = _userService.GetAllUsersAsync().Result
                    .Where(u => u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewData["searchTerm"] = searchTerm;
            return View(users);
        }
        public async Task<IActionResult> ViewUserProfile(string userId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("ViewUser");
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var followers = await _userService.GetFollowersAsync(userId);
            var primaryRole = _userService.GetPrimaryUserRoleAsync(userId).Result;

            var userViewModel = new UserVM
            {
                User = user,
                Enrollments = await _courseService.GetUserEnrollmentsAsync(userId),
                Post = await _forumService.GetUserPostsAsync(userId),
                IsCurrentUser = (userId == currentUserId),
                IsFollowing = await _userService.IsFollowingAsync(currentUserId, userId),
                Followers = followers,
                PrimaryRole = primaryRole,
                TotalPosts = await _forumService.GetUserPostCountAsync(userId, 1),
                TotalQuestions = await _forumService.GetUserPostCountAsync(userId, 2)
            };

            return View(userViewModel);
        }

        public async Task<IActionResult> ToggleFollow(string followId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(followId))
            {
                return BadRequest("Invalid user or follow ID.");
            }

            if (await _userService.IsFollowingAsync(userId, followId))
            {
                await _userService.UnfollowUserAsync(userId, followId);
            }
            else
            {
                await _userService.FollowUserAsync(userId, followId);
            }
            return RedirectToAction("ViewUserProfile", new { userId = followId });
        }

        [HttpGet]
        public IActionResult Search(string keyword)
        {
            //if (!User.Identity.IsAuthenticated)
            //{
            //    return RedirectToPage("/Account/Login", new { area = "Identity" });
            //}
            var homeVM = new SearchVM();

            if(keyword == null)
            {
                return View();
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                homeVM.Posts = _context.Posts
                    .Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword))
                    .Select(p => new Post
                    {
                        PostID = p.PostID,
                        Title = p.Title,
                        Content = p.Content
                    })
                    .ToList();

                homeVM.Categories = _context.Categories
                    .Where(c => c.CategoryName.Contains(keyword) || c.Description.Contains(keyword))
                    .Select(c => new Category
                    {
                        CategoryID = c.CategoryID,
                        CategoryName = c.CategoryName,
                        Description = c.Description
                    })
                    .ToList();

                homeVM.Courses = _context.Courses
                    .Where(c => c.Title.Contains(keyword) || c.Description.Contains(keyword))
                    .Select(c => new Course
                    {
                        CourseID = c.CourseID,
                        Title = c.Title,
                        Description = c.Description
                    })
                    .ToList();
            }

            return View(homeVM);
        }

        public IActionResult Banned()
        {
            return View();
        }

        public IActionResult MentorHall()
        {
            var topMentors = _userService.GetAllUsersAsync().Result
                .OrderByDescending(u => u.Point)
                .Take(3)
                .ToList();

            var otherMentors = _userService.GetAllUsersAsync().Result
                .OrderByDescending(u => u.Point)
                .Skip(3)
                .ToList();

            var mentorViewModel = new MentorVM
            {
                TopMentors = topMentors,
                OtherMentors = otherMentors
            };

            return View(mentorViewModel);
        }
    }
}
