using FuCommunityWebDataAccess.Data;
using FuCommunityWebModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FuCommunityWebDataAccess.Repositories
{
    public class AdminRepo
    {
        private readonly ApplicationDbContext _context;

        public AdminRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalUsersAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetTotalCoursesAsync()
        {
            return await _context.Courses.CountAsync();
        }

        public async Task<int> GetTotalPostsAsync()
        {
            return await _context.Posts.CountAsync();
        }

        public async Task<decimal> GetTotalAmountAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == "1")
                .SumAsync(o => o.Amount);
        }

        public async Task<List<(int Year, int Month, int Count)>> GetUserRegistrationsAsync()
        {
            return await _context.Users
                .GroupBy(u => new { u.CreatedDate.Year, u.CreatedDate.Month })
                .Select(g => new ValueTuple<int, int, int>(
                    g.Key.Year,
                    g.Key.Month,
                    g.Count()
                ))
                .ToListAsync();
        }
    }
}