using FuCommunityWebModels.Models;
using FuCommunityWebModels.ViewModels;
using FuCommunityWebModels.ViewModels.FuCommunityWebModels.ViewModels;
using FuCommunityWebServices.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using FuCommunityWebDataAccess.Data;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Drawing;

namespace FUCommunityWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly CourseService _courseService;
        private readonly ILogger<AdminController> _logger;
        private readonly UserService _userService;
        private readonly OrderService _orderService;
        private readonly ForumService _forumService;
        private readonly AdminService _adminService;

        public AdminController(CourseService courseService, UserService userService, ILogger<AdminController> logger, OrderService orderService, ForumService forumService, AdminService adminService)
        {
            _courseService = courseService;
            _userService = userService;
            _logger = logger;
            _orderService = orderService;
            _forumService = forumService;
            _adminService = adminService;
        }

        public async Task<IActionResult> ManageCourse(string search, string sortOrder)
        {
            var courses = await _courseService.GetAllCoursesAsync();

            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewData["SearchQuery"] = search;
            ViewData["IDSortParm"] = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["UserIDSortParm"] = sortOrder == "UserID" ? "userid_desc" : "UserID";
            ViewData["TitleSortParm"] = sortOrder == "Title" ? "title_desc" : "Title";

            switch (sortOrder)
            {
                case "id_desc":
                    courses = courses.OrderByDescending(c => c.CourseID).ToList();
                    break;
                case "UserID":
                    courses = courses.OrderBy(c => c.UserID).ToList();
                    break;
                case "userid_desc":
                    courses = courses.OrderByDescending(c => c.UserID).ToList();
                    break;
                case "Title":
                    courses = courses.OrderBy(c => c.Title).ToList();
                    break;
                case "title_desc":
                    courses = courses.OrderByDescending(c => c.Title).ToList();
                    break;
                default:
                    courses = courses.OrderBy(c => c.CourseID).ToList();
                    break;
            }

            var viewModel = new CourseVM
            {
                Courses = courses,
                Categories = await _courseService.GetAllCategoriesAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CreateCourseVM createCourseVM)
        {
            if (ModelState.IsValid)
            {
                createCourseVM.CourseImage = await UploadCourseImage(createCourseVM.CourseImageFile);

                int? documentId = null;
                if (createCourseVM.DocumentFile != null && createCourseVM.DocumentFile.Length > 0)
                {
                    var document = await UploadDocument(createCourseVM.DocumentFile);
                    documentId = await _courseService.AddDocumentAsync(document);
                }

                try
                {
                    var course = new Course
                    {
                        Title = createCourseVM.Title,
                        Description = createCourseVM.Description,
                        Price = createCourseVM.Price,
                        CourseImage = createCourseVM.CourseImage,
                        Status = "active",
                        UserID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        Semester = createCourseVM.Semester,
                        CategoryID = createCourseVM.CategoryID,
                        CreatedDate = DateTime.Now,
                        DocumentID = documentId
                    };

                    await _courseService.AddCourseAsync(course);

                }
                catch (Exception ex)
                {
                    // Log error
                }
            }
            else
            {
            }

            return RedirectToAction("ManageCourse");
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var editCourseVM = new EditCourseVM
            {
                CourseID = course.CourseID,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price ?? 0,
                CourseImage = course.CourseImage,
                Semester = course.Semester,
                CategoryID = course.CategoryID,
                Document = course.Document
            };

            return View("ManageCourse", editCourseVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(EditCourseVM editCourseVM)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = await _courseService.GetCourseByIdAsync(editCourseVM.CourseID);
                if (courseToUpdate == null)
                {
                    return RedirectToAction("ManageCourse");
                }

                courseToUpdate.Title = editCourseVM.Title;
                courseToUpdate.Description = editCourseVM.Description;
                courseToUpdate.Price = editCourseVM.Price;
                courseToUpdate.Semester = editCourseVM.Semester;
                courseToUpdate.CategoryID = editCourseVM.CategoryID;

                if (editCourseVM.CourseImageFile != null && editCourseVM.CourseImageFile.Length > 0)
                {
                    courseToUpdate.CourseImage = await UploadCourseImage(editCourseVM.CourseImageFile);
                }

                if (editCourseVM.DocumentFile != null && editCourseVM.DocumentFile.Length > 0)
                {
                    var document = await UploadDocument(editCourseVM.DocumentFile);
                    courseToUpdate.DocumentID = await _courseService.AddDocumentAsync(document);
                }

                courseToUpdate.UpdatedDate = DateTime.Now;

                try
                {
                    await _courseService.UpdateCourseAsync(courseToUpdate);
                }
                catch (Exception ex)
                {
                    // Log error
                }
            }

            return RedirectToAction("ManageCourse");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course == null)
            {
                return RedirectToAction("ManageCourse");
            }

            try
            {
                var enrollments = await _courseService.GetEnrollmentsByCourseIdAsync(courseId);
                foreach (var enrollment in enrollments)
                {
                    await _courseService.DeleteEnrollmentAsync(enrollment);
                }

                var reviews = await _courseService.GetReviewsByCourseIdAsync(courseId);
                foreach (var review in reviews)
                {
                    await _courseService.DeleteReviewAsync(review);
                }

                await _courseService.DeleteCourseAsync(course);
            }
            catch (Exception)
            {
            }

            return RedirectToAction("ManageCourse");
        }

        private async Task<string> UploadCourseImage(IFormFile courseImageFile)
        {
            if (courseImageFile != null && courseImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(courseImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await courseImageFile.CopyToAsync(fileStream);
                }

                return "/uploads/" + uniqueFileName;
            }
            return "/img/Logo_FunnyCode.jpg";
        }

        public async Task<IActionResult> ManageLesson(int courseId, string search, string sortOrder)
        {
            var lessons = await _courseService.GetLessonsByCourseIdAsync(courseId);

            if (lessons == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(search))
            {
                lessons = lessons.Where(l => l.Title.Contains(search) || l.LessonID.ToString().Contains(search)).ToList();
            }
            ViewData["IDSortParm"] = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["TitleSortParm"] = sortOrder == "Title" ? "title_desc" : "Title";

            switch (sortOrder)
            {
                case "id_desc":
                    lessons = lessons.OrderByDescending(l => l.LessonID).ToList();
                    break;
                case "Title":
                    lessons = lessons.OrderBy(l => l.Title).ToList();
                    break;
                case "title_desc":
                    lessons = lessons.OrderByDescending(l => l.Title).ToList();
                    break;
                default:
                    lessons = lessons.OrderBy(l => l.LessonID).ToList();
                    break;
            }

            var course = await _courseService.GetCourseByIdAsync(courseId); 

            if (course == null)
            {
                return NotFound();
            }

            var reviews = await _courseService.GetReviewsByCourseIdAsync(courseId);

            var viewModel = new CourseDetailVM
            {
                Course = course,
                Lessons = lessons,
                Reviews = reviews,
                CreateLessonVM = new CreateLessonVM { CourseID = courseId }, 
                ShowCreateLessonModal = false,
                ShowEditLessonModal = false
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(CreateLessonVM createLessonVM)
        {
            var course = await _courseService.GetCourseByIdAsync(createLessonVM.CourseID); 

            if (course == null)
            {
                return RedirectToAction("ManageLesson", new { courseId = createLessonVM.CourseID });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var lesson = new Lesson
                    {
                        Title = createLessonVM.Title,
                        Content = createLessonVM.Content,
                        Status = "Active",
                        CourseID = createLessonVM.CourseID, 
                        UserID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        CreatedDate = DateTime.Now
                    };

                    await _courseService.AddLessonAsync(lesson);

                }
                catch (Exception ex)
                {
                }
            }

            return RedirectToAction("ManageLesson", "Admin", new { courseId = createLessonVM.CourseID });
        }

        [HttpGet]
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _courseService.GetLessonByIdAsync(id);
            if (lesson == null)
            {
                return RedirectToAction("ManageLesson", new { courseId = lesson?.CourseID });
            }
            var editLessonVM = new EditLessonVM
            {
                LessonID = lesson.LessonID,
                CourseID = lesson.CourseID,
                Title = lesson.Title,
                Content = lesson.Content
            };

            var viewModel = new CourseDetailVM
            {
                EditLessonVM = editLessonVM
            };
            return PartialView("ManageLesson", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(EditLessonVM editLessonVM)
        {
            var lessonToUpdate = await _courseService.GetLessonByIdAsync(editLessonVM.LessonID);

            if (lessonToUpdate == null)
            {
                return RedirectToAction("ManageLesson", new { courseId = editLessonVM.CourseID });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    lessonToUpdate.Title = editLessonVM.Title;
                    lessonToUpdate.Content = editLessonVM.Content;
                    lessonToUpdate.Status = "Active";
                    lessonToUpdate.UpdatedDate = DateTime.Now;

                    await _courseService.UpdateLessonAsync(lessonToUpdate);

                }
                catch (Exception ex)
                {
                    // Log error
                }
            }

            return RedirectToAction("ManageLesson", new { courseId = lessonToUpdate.CourseID });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var lesson = await _courseService.GetLessonByIdAsync(lessonId);

            if (lesson == null)
            {
                return RedirectToAction("ManageLesson", new { courseId = lesson.CourseID });
            }

            try
            {
                await _courseService.DeleteLessonAsync(lesson);
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("ManageLesson", new { courseId = lesson.CourseID });
        }


        public async Task<IActionResult> Index()
        {
            var totalUsers = await _adminService.GetTotalUsersAsync();
            var totalCourses = await _adminService.GetTotalCoursesAsync();
            var totalPosts = await _adminService.GetTotalPostsAsync();
            var totalAmount = await _adminService.GetTotalAmountAsync();
            var userRegistrations = await _adminService.GetUserRegistrationsAsync();

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var monthlyRegistrations = new int[12];

            for (int i = 0; i < 12; i++)
            {
                var month = (currentMonth - i + 11) % 12 + 1;
                var year = currentMonth - i <= 0 ? currentYear - 1 : currentYear;

                var registration = userRegistrations.FirstOrDefault(r => r.Year == year && r.Month == month);
                monthlyRegistrations[11 - i] = registration.Count;
            }

            var dashboardVM = new DashboardVM
            {
                TotalUsers = totalUsers,
                TotalCourses = totalCourses,
                TotalPosts = totalPosts,
                TotalAmount = totalAmount,
                MonthlyUserRegistrations = monthlyRegistrations
            };

            return View(dashboardVM);
        }

        public IActionResult ManageForumGroup()
        {
            return View();
        }
        public async Task<IActionResult> ManagePost(CategoryVM categoryVM)
        {
            var modal = new PostVM
            {
                CategoryVM = new CategoryVM
                {
                    CategoryID = categoryVM.CategoryID
                }
            };
            var posts = await _forumService.GetPostsByCategory(int.Parse(categoryVM.CategoryID));
            modal.Posts = posts;
            return View(modal);
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts(int categoryID, int page = 1, int pageSize = 2, string searchString = "")
        {
            var (posts, totalItems) = await _forumService.GetPostsByCategory(categoryID, page, pageSize, searchString);

            return Json(new
            {
                posts = posts,
                totalItems = totalItems,
                pageSize = pageSize,
                currentPage = page
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(PostVM postVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (postVM.CreatePostVM.PostImageFile != null && postVM.CreatePostVM.PostImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(postVM.CreatePostVM.PostImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await postVM.CreatePostVM.PostImageFile.CopyToAsync(fileStream);
                }
                postVM.CreatePostVM.PostImage = "/uploads/" + uniqueFileName;
            }
            else
            {
                postVM.CreatePostVM.PostImage = "/img/Logo_FunnyCode.jpg";
            }

            int? documentId = null;
            if (postVM.CreatePostVM.DocumentFile != null && postVM.CreatePostVM.DocumentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(postVM.CreatePostVM.DocumentFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await postVM.CreatePostVM.DocumentFile.CopyToAsync(fileStream);
                }

                var document = new Document
                {
                    Name = postVM.CreatePostVM.DocumentFile.FileName,
                    FileUrl = "/documents/" + uniqueFileName,
                    UserID = userId,
                    UploadedAt = DateTime.Now
                };

                documentId = await _forumService.AddDocumentAsync(document);
            }

            var encodedContent = WebUtility.HtmlEncode(postVM.CreatePostVM.Content);

            var post = new Post
            {
                Title = WebUtility.HtmlEncode(postVM.CreatePostVM.Title),
                Content = encodedContent,
                CategoryID = postVM.CreatePostVM.CategoryID,
                UserID = userId,
                CreatedDate = DateTime.Now,
                Status = PostStatus.Approved.ToString(),
                Tag = WebUtility.HtmlEncode(postVM.CreatePostVM.Tag),
                Type = postVM.CreatePostVM.Type,
                PostImage = postVM.CreatePostVM.PostImage,
                DocumentID = documentId
            };

            await _forumService.AddPostAsync(post);

            return RedirectToAction("ManagePost", new
            {
                CategoryID = postVM.CreatePostVM.CategoryID
            });
        }

        public IActionResult Post(CategoryVM categoryVM)
        {
            var modal = new PostVM
            {
                CategoryVM = new CategoryVM
                {
                    CategoryName = categoryVM.CategoryName,
                    CategoryID = categoryVM.CategoryID
                }
            };
            return View(modal);
        }

        [HttpGet]
        public async Task<IActionResult> PostDetail(int postId)
        {
            var modal = new PostVM();
            modal = await _forumService.GetComments(postId);
            return View(modal);
        }

        [HttpGet]
        public async Task<IActionResult> GetPost(int postId)
        {
            var post = await _forumService.GetPostByID(postId);
            if (post == null)
            {
                return NotFound();
            }

            var postData = new
            {
                postID = post.PostID,
                title = post.Title,
                tag = post.Tag,
                type = post.Type,
                postImage = post.PostImage,
                content = post.Content,
                document = post.Document != null ? new { post.Document.FileUrl, post.Document.Name } : null
            };

            return Json(postData);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(PostVM postVM)
        {
            var existingPost = await _forumService.GetPostByID(postVM.Post.PostID);
            if (existingPost == null)
            {
                return NotFound();
            }

            if (postVM.CreatePostVM.PostImageFile != null && postVM.CreatePostVM.PostImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(postVM.CreatePostVM.PostImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await postVM.CreatePostVM.PostImageFile.CopyToAsync(fileStream);
                }
                existingPost.PostImage = "/uploads/" + uniqueFileName;
            }

            if (postVM.CreatePostVM.DocumentFile != null && postVM.CreatePostVM.DocumentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(postVM.CreatePostVM.DocumentFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await postVM.CreatePostVM.DocumentFile.CopyToAsync(fileStream);
                }

                var document = new Document
                {
                    Name = postVM.CreatePostVM.DocumentFile.FileName,
                    FileUrl = "/documents/" + uniqueFileName,
                    UserID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UploadedAt = DateTime.Now
                };

                existingPost.DocumentID = await _forumService.AddDocumentAsync(document);
            }

            existingPost.Title = postVM.Post.Title;
            existingPost.Content = WebUtility.HtmlEncode(postVM.Post.Content);
            existingPost.Tag = postVM.Post.Tag;
            existingPost.Type = postVM.CreatePostVM.Type;

            await _forumService.UpdatePost(existingPost);

            return RedirectToAction("ManagePost", new
            {
                CategoryID = postVM.CreatePostVM.CategoryID
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(PostVM postVM)
        {
            var existingPost = await _forumService.GetPostByID(postVM.Post.PostID);
            if (existingPost == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != existingPost.UserID)
            {
                return Forbid();
            }

            await _forumService.DeletePost(existingPost.PostID);

            return RedirectToAction("ManagePost", new
            {
                categoryId = postVM.CategoryVM.CategoryID
            });
        }

        public IActionResult ManageForumPost()
        {
            return View();
        }
        public IActionResult ManageForumUser()
        {
            return View();
        }
        public async Task<IActionResult> ManagePayment()
        {
            var orders = await _orderService.GetAllOrdersAsync(); 
            return View(orders);
        }
        public IActionResult ManageStudent()
        {
            return View();
        }
        public async Task<IActionResult> ManageUser(string search, string sortOrder)
        {
            var users = await _userService.GetAllUsersAsync();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.FullName.Contains(search) || u.Email.Contains(search)).ToList();
            }

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
            ViewData["PointSortParm"] = sortOrder == "Point" ? "point_desc" : "Point";

            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(u => u.FullName).ToList();
                    break;
                case "Email":
                    users = users.OrderBy(u => u.Email).ToList();
                    break;
                case "email_desc":
                    users = users.OrderByDescending(u => u.Email).ToList();
                    break;
                case "Point":
                    users = users.OrderBy(u => u.Point).ToList();
                    break;
                case "point_desc":
                    users = users.OrderByDescending(u => u.Point).ToList();
                    break;
                default:
                    users = users.OrderBy(u => u.FullName).ToList();
                    break;
            }

            var viewModel = new ManageUserVM
            {
                Users = users
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("ManageUser");
            }

            try
            {
                await _userService.DeleteUserAsync(user);
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("ManageUser");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(long orderId)
        {
            try
            {
                await _orderService.DeleteOrderAsync(orderId); 
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("ManagePayment");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("ManageUser");
            }

            try
            {
                user.Ban = true;
                await _userService.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("ManageUser");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("ManageUser");
            }

            try
            {
                user.Ban = false;
                await _userService.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("ManageUser");
        }

        public async Task<IActionResult> ManageForumCategory()
        {
            var categories = await _forumService.GetAllCategoryAsync();
            var viewModel = new ForumVM
            {
                Categories = categories
            };
            return View(viewModel);
        }

        public async Task<IActionResult> ManagePostDetail(int postId)
        {
            var post = await _forumService.GetPostByID(postId);
            if (post == null)
            {
                return NotFound();
            }

            var comments = await _forumService.GetCommentsByPostID(postId);
            var viewModel = new PostVM
            {
                Post = post,
                Comments = comments
            };
            return View(viewModel);
        }

        public async Task<IActionResult> ManageCategory()
        {
            var categories = await _forumService.GetAllCategoryAsync();
            var viewModel = new ForumVM
            {
                Categories = categories
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory([Bind(Prefix = "CreateCategory")] CreateCategoryVM createCategoryVM)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    CategoryName = createCategoryVM.CategoryName,
                    Description = createCategoryVM.Description
                };

                try
                {
                    await _forumService.AddCategoryAsync(category);
                }
                catch (Exception ex)
                {
                }
            }
            return RedirectToAction("ManageCategory");
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int categoryId)
        {
            var category = await _forumService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            var editCategoryVM = new EditCategoryVM
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                Description = category.Description
            };

            return View(editCategoryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory([Bind(Prefix = "EditCategory")] EditCategoryVM editCategoryVM)
        {
            if (ModelState.IsValid)
            {
                var category = await _forumService.GetCategoryByIdAsync(editCategoryVM.CategoryID);
                if (category == null)
                {
                    return RedirectToAction("ManageForumCategory");
                }

                category.CategoryName = editCategoryVM.CategoryName;
                category.Description = editCategoryVM.Description;

                await _forumService.UpdateCategoryAsync(category);
            }
            else
            {
            }
            return RedirectToAction("ManageCategory");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = await _forumService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return RedirectToAction("ManageForumCategory");
            }

            var relatedPosts = await _forumService.GetPostsByCategory(categoryId);

            if (relatedPosts.Any())
            {
                await _forumService.DeletePostsAsync(relatedPosts); ;
            }

            await _forumService.DeleteCategoryAsync(category);
            return RedirectToAction("ManageCategory");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(PostVM postVM)
        {
            var existingComment = await _forumService.GetCommentByID(postVM.Comment.CommentID);
            if (existingComment == null)
            {
                return NotFound();
            }

            await _forumService.DeteleComment(existingComment.CommentID);

            return RedirectToAction("ManagePostDetail", new
            {
                postId = postVM.Post.PostID
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToMentor(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("ManageUser");
            }

            await _userService.RemoveFromRoleAsync(user, "Student");
            await _userService.AddToRoleAsync(user, "Mentor");

            return RedirectToAction("ManageUser");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoteToStudent(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("ManageUser");
            }

            await _userService.RemoveFromRoleAsync(user, "Mentor");
            await _userService.AddToRoleAsync(user, "Student");

            return RedirectToAction("ManageUser");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePost(int postId)
        {
            var post = await _forumService.GetPostByID(postId);
            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Approved.ToString();
            await _forumService.UpdatePost(post);

            return RedirectToAction("ManagePost", new { CategoryID = post.CategoryID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPost(int postId)
        {
            var post = await _forumService.GetPostByID(postId);
            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Rejected.ToString();
            await _forumService.UpdatePost(post);

            return RedirectToAction("ManagePost", new { CategoryID = post.CategoryID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateCourse(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return RedirectToAction("ManageCourse");
            }

            course.Status = "active";
            await _courseService.UpdateCourseAsync(course);

            return RedirectToAction("ManageCourse");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateCourse(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return RedirectToAction("ManageCourse");
            }

            course.Status = "inactive";
            await _courseService.UpdateCourseAsync(course);

            return RedirectToAction("ManageCourse");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var review = await _courseService.GetReviewByIdAsync(reviewId);
            if (review == null)
            {
                return NotFound();
            }

            try
            {
                await _courseService.DeleteReviewAsync(review);
            }
            catch (Exception ex)
            {
                // Log error
            }

            return RedirectToAction("ManageLesson", new { courseId = review.CourseID });
        }

        private async Task<Document> UploadDocument(IFormFile documentFile)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/documents");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(documentFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await documentFile.CopyToAsync(fileStream);
            }

            return new Document
            {
                Name = documentFile.FileName,
                FileUrl = "/documents/" + uniqueFileName,
                UserID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                UploadedAt = DateTime.Now
            };
        }
    }
}
