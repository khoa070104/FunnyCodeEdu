using FuCommunityWebDataAccess.Data;
using FuCommunityWebModels.Models;
using FuCommunityWebModels.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebDataAccess.Repositories
{
    public class ForumRepo
    {
        private readonly ApplicationDbContext _context;
        public ForumRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PostVM> GetPostDetailsAsync(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.Document)
                .FirstOrDefaultAsync(p => p.PostID == postId);

            var comments = await _context.Comments
                .Where(c => c.PostID == postId)
                .Select(c => new Comment
                {
                    Content = c.Content,
                    CreatedDate = c.CreatedDate,
                    UserID = c.UserID,
                    CommentID = c.CommentID,
                    ReplyID = c.ReplyID
                }).ToListAsync();

            var userIds = comments.Select(c => c.UserID).Distinct().ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new UserVM
                {
                    User = u,
                    Post = _context.Posts.Where(p => p.UserID == u.Id).ToList()
                }).ToListAsync();

            var voteCount = await _context.IsVotes.CountAsync(v => v.PostID == postId); 

            return new PostVM
            {
                Post = post,
                Comments = comments,
                Users = users,
                VoteCount = voteCount 
            };
        }

        public async Task<List<Category>> GetAllCategoryAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<(List<Post> posts, int totalItems)> GetPostsByCategory(int categoryID, int page, int pageSize, string searchString)
        {
            var query = _context.Posts
                .Include(p => p.Document)
                .AsQueryable();

            query = query.Where(post => post.CategoryID == categoryID);

            query = query.Where(post => post.Status == PostStatus.Approved.ToString());

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(post => post.Title.Contains(searchString) || post.Content.Contains(searchString));
            }

            var totalItems = await query.CountAsync();

            var posts = await query
                .OrderByDescending(post => post.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (posts, totalItems);
        }
        public async Task<List<Post>> GetPostsByCategory(int categoryID)
        {
            var query = _context.Posts.AsQueryable();

            query = query.Where(post => post.CategoryID == categoryID);

            var posts = await query
                .OrderBy(post => post.PostID)
                .ToListAsync();

            return posts;
        }
        public async Task<(List<Post> posts, int totalItems)> GetPostsAsync(int page, int pageSize, string searchString)
        {
            var query = _context.Posts.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Title.Contains(searchString) || p.Content.Contains(searchString));
            }

            var totalItems = await query.CountAsync();

            var posts = await query
                .OrderBy(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (posts, totalItems);
        }
        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Votes)
                .Where(p => p.Status == PostStatus.Approved.ToString())
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
        public async Task<Post> GetPostByID(int postId)
        {
            return await _context.Posts
                .Include(p => p.Document)
                .FirstOrDefaultAsync(p => p.PostID == postId);
        }

        public async Task UpdatePost(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post != null)
            {
                var comments = _context.Comments.Where(c => c.PostID == postId).ToList();
                _context.Comments.RemoveRange(comments);

                _context.Posts.Remove(post);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Comment>> GetCommentsByPostID(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostID == postId)
                .ToListAsync();
        }

        public async Task DeleteComment(int commentId)
        {
            var childComments = _context.Comments.Where(c => c.ReplyID == commentId);

            _context.Comments.RemoveRange(childComments);

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<Comment> GetCommentByID(int commentId)
        {
            return await _context.Comments.FindAsync(commentId);
        }

        public async Task UpdateComment(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IsVote> GetVoteByUserAndPost(string userId, int postId)
        {
            return await _context.IsVotes
                .FirstOrDefaultAsync(v => v.UserID == userId && v.PostID == postId);
        }

        public async Task AddVote(IsVote vote)
        {
            _context.IsVotes.Add(vote);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVote(IsVote vote)
        {
            _context.IsVotes.Remove(vote);
            await _context.SaveChangesAsync();
        }

        public async Task AddPoint(Point point)
        {
            _context.Points.Add(point);
            await _context.SaveChangesAsync();
        }

        public async Task<ApplicationUser> GetUserById(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateUser(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            return await _context.Posts
            .Where(p => p.UserID == userId)
            .Include(p => p.Comments)
            .Include(p => p.IsVotes)
            .ToListAsync();
        }

        public async Task<int> GetUserPostCountAsync(string userId, int postType)
        {
            return await _context.Posts
                .CountAsync(p => p.UserID == userId && p.Type == postType);
        }

        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories.FindAsync(categoryId);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostsAsync(IEnumerable<Post> posts)
        {
            _context.Posts.RemoveRange(posts);
            await _context.SaveChangesAsync();
        }

        public async Task<Document> GetDocumentByIdAsync(int documentId)
        {
            return await _context.Documents.FindAsync(documentId);
        }

        public async Task<int?> AddDocumentAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document.DocumentID;
        }

        public async Task<Post> GetPostByIdAsync(int? postId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.PostID == postId);
        }
    }
}
