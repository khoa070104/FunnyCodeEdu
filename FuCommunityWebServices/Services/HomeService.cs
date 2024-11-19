using FuCommunityWebDataAccess.Repositories;
using FuCommunityWebModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuCommunityWebServices.Services
{
    public class HomeService 
    {
        private readonly HomeRepo _homeRepo;

        public HomeService(HomeRepo homeRepo)
        {
            _homeRepo = homeRepo;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _homeRepo.GetAllCoursesAsync();
        }

        public async Task<List<ApplicationUser>> GetAllUsersWithVotesAsync()
        {
            return await _homeRepo.GetAllUsersWithVotesAsync();
        }

       
    }
}