using FuCommunityWebDataAccess.Data;
using FuCommunityWebModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebDataAccess.Repositories
{
    public class UserRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepo(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId, bool includeVotes = false)
        {
            var query = _context.Users.AsQueryable();

            if (includeVotes)
            {
                query = query.Include(u => u.IsVotes);
            }

            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser> GetUserById(string userId)
        {
            var query = _context.Users.AsQueryable();

            return await query.FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<List<ApplicationUser>> GetAllUsersAsync(bool includeVotes = false)
        {
            var query = _context.Users.AsQueryable();

            if (includeVotes)
            {
                query = query.Include(u => u.IsVotes);
            }

            return await query.ToListAsync();
        }

        public async Task SaveUserAsync(ApplicationUser user)
        {
            if (_context.Users.Any(u => u.Id == user.Id))
            {
                _context.Users.Update(user);
            }
            else
            {
                await _context.Users.AddAsync(user);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            return await _context.Posts
                                 .Where(p => p.UserID == userId)
                                 .Include(p => p.Comments)
                                 .Include(p => p.Votes)
                                 .Include(p => p.User)
                                 .OrderByDescending(p => p.CreatedDate)
                                 .ToListAsync();
        }

        public async Task<List<Enrollment>> GetUserEnrollmentsAsync(string userId)
        {
            return await _context.Enrollment
                                 .Where(e => e.UserID == userId)
                                 .Include(e => e.Course)
                                 .OrderByDescending(e => e.EnrollmentDate)
                                 .ToListAsync();
        }

        public async Task UpdateUserAvatarAsync(string userId, string avatarPath)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.AvatarImage = avatarPath;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ApplicationUser> GetUserWithVotesAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.IsVotes)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ApplicationUser>> GetFollowersAsync(string userId)
        {
            return await _context.Followers
                .Where(f => f.FollowId == userId)
                .Select(f => f.FollowingUser)
                .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(string userId, string followId)
        {
            return await _context.Followers.AnyAsync(f => f.UserID == userId && f.FollowId == followId);
        }

        public async Task FollowUserAsync(string userId, string followId)
        {
            var follower = new Follower
            {
                UserID = userId,
                FollowId = followId
            };
            _context.Followers.Add(follower);
            await _context.SaveChangesAsync();
        }

        public async Task UnfollowUserAsync(string userId, string followId)
        {
            var follower = await _context.Followers.FirstOrDefaultAsync(f => f.UserID == userId && f.FollowId == followId);
            if (follower != null)
            {
                _context.Followers.Remove(follower);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task UpdateUserBannerAsync(string userId, string bannerPath)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.BannerImage = bannerPath;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteUserAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User not found.");
            }

            // Delete all related data before deleting the user
            var roles = _context.UserRoles.Where(r => r.UserId == user.Id);
            var enrollments = _context.Enrollment.Where(e => e.UserID == user.Id);
            var posts = _context.Posts.Where(p => p.UserID == user.Id);
            var reviews = _context.Reviews.Where(r => r.UserID == user.Id);
            var documents = _context.Documents.Where(d => d.UserID == user.Id);
            var votes = _context.Votes.Where(v => v.UserID == user.Id);
            var isVotes = _context.IsVotes.Where(iv => iv.UserID == user.Id);
            var points = _context.Points.Where(p => p.UserID == user.Id);
            var comments = _context.Comments.Where(c => c.UserID == user.Id);
            var orders = _context.Orders.Where(o => o.UserID == user.Id);

            // Remove the related entities
            _context.UserRoles.RemoveRange(roles);
            _context.Enrollment.RemoveRange(enrollments);
            _context.Posts.RemoveRange(posts);
            _context.Reviews.RemoveRange(reviews);
            _context.Documents.RemoveRange(documents);
            _context.Votes.RemoveRange(votes);
            _context.IsVotes.RemoveRange(isVotes);
            _context.Points.RemoveRange(points);
            _context.Comments.RemoveRange(comments);
            _context.Orders.RemoveRange(orders);

            // Finally, remove the user itself
            _context.Users.Remove(user);

            // Save all changes
            await _context.SaveChangesAsync();
        }

        public async Task<ApplicationUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task AddToRoleAsync(ApplicationUser user, string role)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id);
            if (identityUser != null && !await _userManager.IsInRoleAsync(identityUser, role))
            {
                var result = await _userManager.AddToRoleAsync(identityUser, role);
            }
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string role)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id);
            if (identityUser != null && await _userManager.IsInRoleAsync(identityUser, role))
            {
                await _userManager.RemoveFromRoleAsync(identityUser, role);
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        public async Task<string> GetPrimaryUserRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
                return "Admin";
            if (roles.Contains("Mentor"))
                return "Mentor";
            if (roles.Contains("Student"))
                return "Student";

            return "User";
        }

        public async Task UpdateSocialMediaLinksAsync(string userId, string instagram, string facebook, string github)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Instagram = instagram;
                user.Facebook = facebook;
                user.Github = github;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
