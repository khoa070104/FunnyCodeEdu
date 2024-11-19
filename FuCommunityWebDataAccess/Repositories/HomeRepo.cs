using FuCommunityWebDataAccess.Data;
using FuCommunityWebModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FuCommunityWebDataAccess.Repositories
{
    public class HomeRepo 
    {
        private readonly ApplicationDbContext _context;

        public HomeRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }
        public async Task<List<ApplicationUser>> GetAllUsersWithVotesAsync()
        {
            return await _context.Users
                .Include(u => u.IsVotes)
                .ToListAsync();
        }
        

    }
}