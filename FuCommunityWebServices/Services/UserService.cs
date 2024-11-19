using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuCommunityWebDataAccess.Repositories;
using FuCommunityWebModels.Models;
using Microsoft.EntityFrameworkCore;

namespace FuCommunityWebServices.Services
{
    public class UserService
    {
        private readonly UserRepo _userRepo;

        public UserService(UserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId, bool includeVotes = false)
        {
            return await _userRepo.GetUserByIdAsync(userId, includeVotes);
        }

        public async Task<ApplicationUser> GetUserById(string userId)
        {
            return await _userRepo.GetUserById(userId);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync(bool includeVotes = false)
        {
            return await _userRepo.GetAllUsersAsync(includeVotes);
        }

        public async Task SaveUserAsync(ApplicationUser user)
        {
            await _userRepo.SaveUserAsync(user);
        }

        public async Task<List<Post>> GetUserPostsAsync(string userId)
        {
            return await _userRepo.GetUserPostsAsync(userId);
        }

        public async Task<List<Enrollment>> GetUserEnrollmentsAsync(string userId)
        {
            return await _userRepo.GetUserEnrollmentsAsync(userId);
        }

        public async Task UpdateUserAvatarAsync(string userId, string avatarPath)
        {
            await _userRepo.UpdateUserAvatarAsync(userId, avatarPath);
        }

        public async Task<ApplicationUser> GetUserWithVotesAsync(string userId)
        {
            return await _userRepo.GetUserWithVotesAsync(userId);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userRepo.UpdateUserAsync(user);
        }

        public async Task<bool> IsFollowingAsync(string userId, string followId)
        {
            return await _userRepo.IsFollowingAsync(userId, followId);
        }

        public async Task FollowUserAsync(string userId, string followId)
        {
            await _userRepo.FollowUserAsync(userId, followId);
        }

        public async Task UnfollowUserAsync(string userId, string followId)
        {
            await _userRepo.UnfollowUserAsync(userId, followId);
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string role)
        {
            return await _userRepo.IsUserInRoleAsync(userId, role);
        }

        public async Task UpdateUserBannerAsync(string userId, string bannerPath)
        {
            await _userRepo.UpdateUserBannerAsync(userId, bannerPath);
        }

        public async Task DeleteUserAsync(ApplicationUser user)
        {
            await _userRepo.DeleteUserAsync(user);
        }

        public async Task<List<ApplicationUser>> GetFollowersAsync(string userId)
        {
            return await _userRepo.GetFollowersAsync(userId);
        }

        public async Task<ApplicationUser> GetUserByUsernameAsync(string username)
        {
            return await _userRepo.GetUserByUsernameAsync(username);
        }

        public async Task AddToRoleAsync(ApplicationUser user, string role)
        {
            await _userRepo.AddToRoleAsync(user, role);
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string role)
        {
            await _userRepo.RemoveFromRoleAsync(user, role);
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            return await _userRepo.GetUserRolesAsync(userId);
        }

        public async Task<string> GetPrimaryUserRoleAsync(string userId)
        {
            return await _userRepo.GetPrimaryUserRoleAsync(userId);
        }

        public async Task UpdateSocialMediaLinksAsync(string userId, string instagram, string facebook, string github)
        {
            await _userRepo.UpdateSocialMediaLinksAsync(userId, instagram, facebook, github);
        }
    }
}
