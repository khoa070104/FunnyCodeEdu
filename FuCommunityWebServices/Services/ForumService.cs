using FuCommunityWebDataAccess.Repositories;
using FuCommunityWebModels.Models;
using FuCommunityWebModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebServices.Services
{
    public class ForumService
    {
        private readonly ForumRepo _forumRepo;

        public ForumService(ForumRepo forumRepo)
        {
            _forumRepo = forumRepo;
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _forumRepo.GetAllPostsAsync();
        }
        public async Task<List<Category>> GetAllCategoryAsync()
        {
            return await _forumRepo.GetAllCategoryAsync();
        }
        public async Task<(List<Post> posts, int totalItems)> GetPostsAsync(int page, int pageSize, string searchString)
        {
            return await _forumRepo.GetPostsAsync(page, pageSize, searchString);

        }
        public async Task<(List<Post> posts, int totalItems)> GetPostsByCategory(int categoryID, int page, int pageSize, string searchString)
        {
            return await _forumRepo.GetPostsByCategory(categoryID, page, pageSize, searchString);
        }
        public async Task<List<Post>> GetPostsByCategory(int categoryID)
        {
            return await _forumRepo.GetPostsByCategory(categoryID);
        }


        public async Task<Post> GetPostByID(int id)
        {
            return await _forumRepo.GetPostByID(id);
        }

        public async Task<PostVM> GetComments(int postID)
        {
            return await _forumRepo.GetPostDetailsAsync(postID);
        }

        public async Task<List<Comment>> GetCommentsByPostID(int postID)
        {
            return await _forumRepo.GetCommentsByPostID(postID);
        }

        public async Task UpdatePost(Post post)
        {
            await _forumRepo.UpdatePost(post);
        }

        public async Task DeletePost(int id)
        {
            await _forumRepo.DeletePost(id);
        }

        public async Task DeteleComment(int id)
        {
            await _forumRepo.DeleteComment(id);
        }

        public async Task<Comment> GetCommentByID(int id)
        {
            return await _forumRepo.GetCommentByID(id);
        }

        public async Task UpdateComment(Comment comment)
        {
            await _forumRepo.UpdateComment(comment);
        }

        public async Task<IsVote> GetVoteByUserAndPost(string userId, int postID)
        {
            return await _forumRepo.GetVoteByUserAndPost(userId, postID);
        }

        public async Task DeleteVote(IsVote vote)
        {
            await _forumRepo.DeleteVote(vote);
        }

        public async Task AddVote(IsVote vote)
        {
            await _forumRepo.AddVote(vote);
        }

        public async Task UpdateUserPoints(string userId, int pointValue, PointSource source)
        {
            var point = new Point
            {
                UserID = userId,
                PointValue = pointValue,
                From = source,
                Status = true,
                CreateDate = DateTime.Now
            };

            await _forumRepo.AddPoint(point);

            var user = await _forumRepo.GetUserById(userId);
            if (user != null)
            {
                user.Point += pointValue;
                await _forumRepo.UpdateUser(user);
            }
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            return await _forumRepo.GetUserPostsAsync(userId);
        }

        public async Task<int> GetUserPostCountAsync(string userId, int postType)
        {
            return await _forumRepo.GetUserPostCountAsync(userId, postType);
        }

        public async Task AddPostAsync(Post post)
        {
            await _forumRepo.AddPostAsync(post);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _forumRepo.AddCategoryAsync(category);
        }

        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            return await _forumRepo.GetCategoryByIdAsync(categoryId);
        }
        public async Task UpdateCategoryAsync(Category category)
        {
            await _forumRepo.UpdateCategoryAsync(category);
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            await _forumRepo.DeleteCategoryAsync(category);
        }

        public async Task DeletePostsAsync(IEnumerable<Post> posts)
        {
            await _forumRepo.DeletePostsAsync(posts);
        }

        public async Task<Document> GetDocumentByIdAsync(int documentId)
        {
            return await _forumRepo.GetDocumentByIdAsync(documentId);
        }

        public async Task<int?> AddDocumentAsync(Document document)
        {
            return await _forumRepo.AddDocumentAsync(document);
        }

        public async Task<Post> GetPostByIdAsync(int? postId)
        {
            return await _forumRepo.GetPostByIdAsync(postId);
        }
    }
}
