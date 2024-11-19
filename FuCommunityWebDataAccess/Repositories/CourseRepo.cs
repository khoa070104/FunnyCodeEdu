using FuCommunityWebDataAccess.Data;
using FuCommunityWebModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FuCommunityWebDataAccess.Repositories
{
    public class CourseRepo
    {
        private readonly ApplicationDbContext _context;

        public CourseRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.User)
                .Include(c => c.Category)
                .Include(c => c.Document)
                .ToListAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.User)
                .Include(c => c.Lessons)
                .Include(c => c.Category)
                .Include(c => c.Document)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);
        }

        public async Task AddCourseAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCourseAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(Course course)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }

        public async Task<Lesson> GetLessonByIdAsync(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonID == lessonId);
        }

        public async Task AddLessonAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLessonAsync(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLessonAsync(Lesson lesson)
        {
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetEnrolledCoursesAsync(string userId)
        {
            return await _context.Enrollment
                .Where(e => e.UserID == userId)
                .Select(e => e.CourseID)
                .ToListAsync();
        }

        public async Task<List<Course>> GetMostPurchasedCoursesAsync(int count)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .OrderByDescending(c => c.Enrollments.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Course>> GetHighestQualityCoursesAsync(int count)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .OrderByDescending(c => c.Price)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<List<string>> GetAllSubjectCodesAsync()
        {
            return await _context.Courses
                .Select(c => c.Title)
                .Distinct()
                .OrderBy(title => title)
                .ToListAsync();
        }

        public async Task<List<Course>> GetFilteredCoursesAsync(string semester, string category, string subjectCode, string rate, string minPrice)
        {
            var filteredCourses = _context.Courses
           .Include(c => c.Document) 
           .AsQueryable();

            if (!string.IsNullOrEmpty(semester) && int.TryParse(semester, out int semesterInt))
            {
                filteredCourses = filteredCourses.Where(c => c.Semester == semesterInt);
            }

            if (!string.IsNullOrEmpty(category) && int.TryParse(category, out int categoryInt))
            {
                filteredCourses = filteredCourses.Where(c => c.CategoryID == categoryInt);
            }

            if (!string.IsNullOrEmpty(subjectCode))
            {
                filteredCourses = filteredCourses.Where(c => c.Title == subjectCode);
            }

            if (!string.IsNullOrEmpty(rate) && int.TryParse(rate, out int rateInt))
            {
                filteredCourses = filteredCourses
                    .Where(c => c.Reviews.Any())
                    .Where(c => c.Reviews.Average(r => r.Rating) >= rateInt);
            }

            if (!string.IsNullOrEmpty(minPrice) && decimal.TryParse(minPrice, out decimal priceDecimal))
            {
                filteredCourses = filteredCourses.Where(c => c.Price <= priceDecimal);
            }

            return await filteredCourses.ToListAsync();
        }

        public async Task<Lesson> GetLessonWithCourseAsync(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonID == lessonId);
        }

        public async Task<List<Lesson>> GetLessonsByCourseIdAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseID == courseId)
                .OrderBy(l => l.LessonID)
                .ToListAsync();
        }

        public async Task<List<int>> GetEnrolledCourseIdsAsync(string userId)
        {
            return await _context.Enrollment
                .Where(e => e.UserID == userId)
                .Select(e => e.CourseID)
                .ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> IsUserEnrolledInCourseAsync(string userId, int courseId)
        {
            return await _context.Enrollment.AnyAsync(e => e.UserID == userId && e.CourseID == courseId);
        }

        public async Task EnrollUserInCourseAsync(Enrollment enrollment)
        {
            _context.Enrollment.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Enrollment>> GetUserEnrollmentsAsync(string userId)
        {
            return await _context.Enrollment
                .Where(e => e.UserID == userId)
                .Include(e => e.Course)
                .ToListAsync();
        }

        public async Task AddReviewAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<int, double>> GetAverageRatingsAsync()
        {
            return await _context.Reviews
                .GroupBy(r => r.CourseID)
                .Select(g => new { CourseID = g.Key, AverageRating = g.Average(r => r.Rating) })
                .ToDictionaryAsync(g => g.CourseID, g => g.AverageRating);
        }

        public async Task<Dictionary<int, int>> GetReviewCountsAsync()
        {
            return await _context.Reviews
                .GroupBy(r => r.CourseID)
                .Select(g => new { CourseID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.CourseID, g => g.Count);
        }

        public async Task<Review> GetUserReviewForCourseAsync(string userId, int courseId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserID == userId && r.CourseID == courseId);
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews.FindAsync(reviewId);
        }

        public async Task UpdateReviewAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Review>> GetReviewsByCourseIdAsync(int courseId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CourseID == courseId)
                .OrderByDescending(r => r.CreateDate)
                .ToListAsync();
        }

        public async Task DeleteReviewAsync(Review review)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            return await _context.Enrollment
                .Where(e => e.CourseID == courseId)
                .ToListAsync();
        }

        public async Task DeleteEnrollmentAsync(Enrollment enrollment)
        {
            _context.Enrollment.Remove(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task AddDocumentAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
        }
    }
}
