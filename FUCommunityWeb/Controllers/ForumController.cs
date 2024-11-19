using FuCommunityWebDataAccess.Data;
using FuCommunityWebModels.Models;
using FuCommunityWebModels.ViewModels;
using FuCommunityWebServices.Services;
using FuCommunityWebUtility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FUCommunityWeb.Controllers
{
    public class ForumController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ForumService _forumService;
        private readonly HomeService _homeService;
        private readonly UserService _userService;
        private readonly NotificationService _notificationService;

        public ForumController(ILogger<HomeController> logger, ApplicationDbContext context, ForumService forumService, HomeService homeService, UserService userService, NotificationService notificationService)
        {
            _logger = logger;
            _context = context;
            _forumService = forumService;
            _homeService = homeService;
            _userService = userService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _forumService.GetAllPostsAsync();
            posts = posts.Where(p => p.Status == PostStatus.Approved.ToString()).ToList();
            var category = await _forumService.GetAllCategoryAsync();

            var forumViewModel = new ForumVM
            {
                Posts = posts,
                Categories = category
            };

            return View(forumViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Comment(PostVM postVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var replyID = postVM.Comment.ReplyID;

            var comment = new Comment
            {
                Content = WebUtility.HtmlEncode(postVM.Comment.Content),
                PostID = postVM.Comment.PostID,
                ReplyID = replyID,
                UserID = userId,
                CreatedDate = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var existingPost = await _forumService.GetPostByIdAsync(postVM.Comment.PostID);
            
            if (existingPost != null && existingPost.UserID != userId)
            {
                var currentUser = await _userService.GetUserByIdAsync(userId);
                if (currentUser != null)
                {
                    await _notificationService.CreateCommentNotification(
                        fromUserId: userId,
                        toUserId: existingPost.UserID,
                        postId: existingPost.PostID,
                        message: $"{currentUser.FullName} commented on your post \"{existingPost.Title}\"."
                    );
                }
            }

            return RedirectToAction("PostDetail", new { postId = postVM.Comment.PostID });
        }


        [HttpGet]
        public async Task<IActionResult> GetPosts(int categoryID, int page = 1, int pageSize = 2, string searchString = "")
        {
            var (posts, totalItems) = await _forumService.GetPostsByCategory(categoryID, page, pageSize, searchString);

            var postData = new List<object>();

            foreach (var post in posts)
            {
                var user = await _userService.GetUserById(post.UserID);

                postData.Add(new
                {
                    postID = post.PostID,
                    title = post.Title,
                    createdDate = post.CreatedDate.ToString(),
                    content = post.Content,
                    tag = post.Tag,
                    type = post.Type == 1 ? "Blog" : "Question",
                    userAvatar = user?.AvatarImage ?? "/img/default-avatar.png",
                    userId = post.UserID,
                    document = post.Document != null ? new { post.Document.FileUrl, post.Document.Name } : null // Thêm thông tin tài liệu
                });
            }

            return Json(new
            {
                posts = postData,
                totalItems = totalItems,
                pageSize = pageSize,
                currentPage = page
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostVM postVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isMentor = User.IsInRole(SD.Role_User_Mentor);
            bool isStudent = User.IsInRole(SD.Role_User_Student);

            if (isStudent && postVM.CreatePostVM.Type == 1)
            {
                TempData["Error"] = "Students can only post questions.";
                return RedirectToAction("Post", new
                {
                    CategoryName = postVM.CategoryVM.CategoryName,
                    CategoryID = postVM.CategoryVM.CategoryID
                });
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
                Status = PostStatus.Pending.ToString(),
                Tag = WebUtility.HtmlEncode(postVM.CreatePostVM.Tag),
                Type = postVM.CreatePostVM.Type,
                PostImage = postVM.CreatePostVM.PostImage,
                DocumentID = documentId
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Post", new
            {
                CategoryName = postVM.CategoryVM.CategoryName,
                CategoryID = postVM.CategoryVM.CategoryID
            });
        }

        [HttpGet]
        public async Task<IActionResult> Post(CategoryVM categoryVM)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var modal = new PostVM
            {
                CategoryVM = new CategoryVM
                {
                    CategoryName = categoryVM.CategoryName,
                    CategoryID = categoryVM.CategoryID
                }
            };

            ViewBag.IsMentor = User.IsInRole(SD.Role_User_Mentor);
            ViewBag.IsStudent = User.IsInRole(SD.Role_User_Student);
            ViewBag.IsAdmin = User.IsInRole(SD.Role_User_Admin);
            return View(modal);
        }

        [HttpGet]
        public async Task<IActionResult> PostDetail(int postId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                var modal = await _forumService.GetComments(postId);
                if (modal == null || modal.Post == null)
                {
                    TempData["Error"] = "Post not found";
                    return RedirectToAction("Index");
                }
                
                var postUser = await _userService.GetUserById(modal.Post.UserID);
                if (postUser != null)
                {
                    modal.Post.User = postUser;
                }

                // Lấy thông tin user cho mỗi comment
                if (modal.Comments != null)
                {
                    foreach (var comment in modal.Comments)
                    {
                        var commentUser = await _userService.GetUserById(comment.UserID);
                        if (commentUser != null)
                        {
                            comment.User = commentUser;
                        }
                    }
                }

                return View(modal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading post details for postId: {PostId}", postId);
                TempData["Error"] = "An error occurred while loading the post";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(PostVM postVM)
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
                    UserID = currentUserId,
                    UploadedAt = DateTime.Now
                };

                existingPost.DocumentID = await _forumService.AddDocumentAsync(document);
            }

            existingPost.Title = postVM.Post.Title;
            existingPost.Content = WebUtility.HtmlEncode(postVM.Post.Content);
            existingPost.Tag = postVM.Post.Tag;

            await _forumService.UpdatePost(existingPost);

            return RedirectToAction("Post", new
            {
                CategoryName = postVM.CategoryVM.CategoryName,
                CategoryID = postVM.CategoryVM.CategoryID
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

            return RedirectToAction("Post", new
            {
                CategoryName = postVM.CategoryVM.CategoryName,
                CategoryID = postVM.CategoryVM.CategoryID
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(PostVM postVM)
        {
            var existingComment = await _forumService.GetCommentByID(postVM.Comment.CommentID);
            if (existingComment == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != existingComment.UserID)
            {
                return Forbid();
            }

            await _forumService.DeteleComment(existingComment.CommentID);

            return RedirectToAction("PostDetail", new
            {
                postId = postVM.Post.PostID
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(PostVM postVM)
        {
            var existingComment = await _forumService.GetCommentByID(postVM.Comment.CommentID);
            if (existingComment == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != existingComment.UserID)
            {
                return Forbid();
            }

            existingComment.Content = WebUtility.HtmlEncode(postVM.Comment.Content);

            await _forumService.UpdateComment(existingComment);

            return RedirectToAction("PostDetail", new
            {
                postId = postVM.Post.PostID
            });
        }

        [HttpPost]
        public async Task<IActionResult> Vote([FromBody] PostVM postVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingVote = await _forumService.GetVoteByUserAndPost(userId, postVM.Post.PostID);

            var post = await _forumService.GetPostByID(postVM.Post.PostID);
            if (post == null)
            {
                return NotFound();
            }

            var postOwnerId = post.UserID;

            if (existingVote != null)
            {
                await _forumService.DeleteVote(existingVote);
                await _forumService.UpdateUserPoints(postOwnerId, -10, PointSource.Vote);
                return Ok(new { message = "Vote removed successfully!" });
            }
            else
            {
                var newVote = new IsVote
                {
                    UserID = userId,
                    PostID = postVM.Post.PostID,
                    Point = postVM.Point.PointValue
                };
                await _forumService.AddVote(newVote);
                await _forumService.UpdateUserPoints(postOwnerId, 10, PointSource.Vote);
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> CheckVote(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasVoted = await _context.IsVotes
            .AnyAsync(v => v.PostID == postId && v.UserID == userId);

            return Ok(new { hasVoted });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(CommentVM model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var comment = new Comment
                {
                    Content = WebUtility.HtmlEncode(model.Content),
                    PostID = model.PostID,
                    ReplyID = model.ReplyID,
                    UserID = userId,
                    CreatedDate = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                var currentUser = await _userService.GetUserByIdAsync(userId);
                
                if (model.ReplyID != null && model.ReplyID > 0)
                {
                    var originalComment = await _context.Comments
                        .Include(c => c.User)
                        .Include(c => c.Post)
                        .FirstOrDefaultAsync(c => c.CommentID == model.ReplyID);

                    if (originalComment != null)
                    {
                        if (originalComment.UserID != userId) 
                        {
                            await _notificationService.CreateReplyNotification(
                                fromUserId: userId,
                                toUserId: originalComment.UserID,
                                postId: model.PostID,
                                message: $"{currentUser.FullName} đã trả lời bình luận của bạn trong bài viết \"{originalComment.Post?.Title}\""
                            );
                        }
                    }
                }
                else
                {
                    var existingPost = await _forumService.GetPostByIdAsync(model.PostID);
                    if (existingPost != null && existingPost.UserID != userId)
                    {
                        await _notificationService.CreateCommentNotification(
                            fromUserId: userId,
                            toUserId: existingPost.UserID,
                            postId: existingPost.PostID,
                            message: $"{currentUser.FullName} đã bình luận về bài viết của bạn: {existingPost.Title}"
                        );
                    }
                }

                return RedirectToAction("PostDetail", new { postId = model.PostID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment: {Error}", ex.Message);
                TempData["Error"] = "Có lỗi xảy ra khi thêm bình luận";
                return RedirectToAction("PostDetail", new { postId = model.PostID });
            }
        }
    }
}
