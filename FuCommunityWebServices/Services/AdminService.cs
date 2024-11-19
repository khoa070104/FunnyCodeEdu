using FuCommunityWebDataAccess.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuCommunityWebServices.Services
{
    public class AdminService
    {
        private readonly AdminRepo _adminRepo;

        public AdminService(AdminRepo adminRepo)
        {
            _adminRepo = adminRepo;
        }

        public async Task<int> GetTotalUsersAsync()
        {
            return await _adminRepo.GetTotalUsersAsync();
        }

        public async Task<int> GetTotalCoursesAsync()
        {
            return await _adminRepo.GetTotalCoursesAsync();
        }

        public async Task<int> GetTotalPostsAsync()
        {
            return await _adminRepo.GetTotalPostsAsync();
        }

        public async Task<decimal> GetTotalAmountAsync()
        {
            return await _adminRepo.GetTotalAmountAsync();
        }

        public async Task<List<(int Year, int Month, int Count)>> GetUserRegistrationsAsync()
        {
            return await _adminRepo.GetUserRegistrationsAsync();
        }
    }
}
