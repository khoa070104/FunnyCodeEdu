using FuCommunityWebDataAccess.Repositories;
using FuCommunityWebModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FuCommunityWebServices.Services
{
    public class CourseService
    {
        private readonly CourseRepo _courseRepo;

        public CourseService(CourseRepo courseRepo)
        {
            _courseRepo = courseRepo;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _courseRepo.GetAllCoursesAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int courseId)
        {
            return await _courseRepo.GetCourseByIdAsync(courseId);
        }

        public async Task AddCourseAsync(Course course)
        {
            await _courseRepo.AddCourseAsync(course);
        }

        public async Task UpdateCourseAsync(Course course)
        {
            await _courseRepo.UpdateCourseAsync(course);
        }

        public async Task DeleteCourseAsync(Course course)
        {
            await _courseRepo.DeleteCourseAsync(course);
        }

        public async Task<Lesson> GetLessonByIdAsync(int lessonId)
        {
            return await _courseRepo.GetLessonByIdAsync(lessonId);
        }

        public async Task AddLessonAsync(Lesson lesson)
        {
            await _courseRepo.AddLessonAsync(lesson);
        }

        public async Task UpdateLessonAsync(Lesson lesson)
        {
            await _courseRepo.UpdateLessonAsync(lesson);
        }

        public async Task DeleteLessonAsync(Lesson lesson)
        {
            await _courseRepo.DeleteLessonAsync(lesson);
        }

        public async Task<List<int>> GetEnrolledCoursesAsync(string userId)
        {
            return await _courseRepo.GetEnrolledCoursesAsync(userId);
        }

        public async Task<List<Course>> GetMostPurchasedCoursesAsync(int count)
        {
            return await _courseRepo.GetMostPurchasedCoursesAsync(count);
        }

        public async Task<List<Course>> GetHighestQualityCoursesAsync(int count)
        {
            return await _courseRepo.GetHighestQualityCoursesAsync(count);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _courseRepo.GetAllCategoriesAsync();
        }

        public async Task<List<string>> GetAllSubjectCodesAsync()
        {
            return await _courseRepo.GetAllSubjectCodesAsync();
        }

        public async Task<List<Course>> GetFilteredCoursesAsync(string semester, string category, string subjectCode, string rate, string minPrice)
        {
            return await _courseRepo.GetFilteredCoursesAsync(semester, category, subjectCode, rate, minPrice);
        }

        public async Task<Lesson> GetLessonWithCourseAsync(int lessonId)
        {
            return await _courseRepo.GetLessonWithCourseAsync(lessonId);
        }

        public async Task<List<Lesson>> GetLessonsByCourseIdAsync(int courseId)
        {
            return await _courseRepo.GetLessonsByCourseIdAsync(courseId);
        }

        public async Task<List<int>> GetEnrolledCourseIdsAsync(string userId)
        {
            return await _courseRepo.GetEnrolledCourseIdsAsync(userId);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _courseRepo.GetUserByIdAsync(userId);
        }

        public async Task<bool> IsUserEnrolledInCourseAsync(string userId, int courseId)
        {
            return await _courseRepo.IsUserEnrolledInCourseAsync(userId, courseId);
        }

        public async Task EnrollUserInCourseAsync(Enrollment enrollment)
        {
            await _courseRepo.EnrollUserInCourseAsync(enrollment);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _courseRepo.UpdateUserAsync(user);
        }

        public async Task<List<Enrollment>> GetUserEnrollmentsAsync(string userId)
        {
            return await _courseRepo.GetUserEnrollmentsAsync(userId);
        }

        public async Task AddReviewAsync(Review review)
        {
            await _courseRepo.AddReviewAsync(review);
        }

        public async Task<Dictionary<int, double>> GetAverageRatingsAsync()
        {
            return await _courseRepo.GetAverageRatingsAsync();
        }

        public async Task<Dictionary<int, int>> GetReviewCountsAsync()
        {
            return await _courseRepo.GetReviewCountsAsync();
        }

        public async Task<Review> GetUserReviewForCourseAsync(string userId, int courseId)
        {
            return await _courseRepo.GetUserReviewForCourseAsync(userId, courseId);
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            return await _courseRepo.GetReviewByIdAsync(reviewId);
        }

        public async Task UpdateReviewAsync(Review review)
        {
            await _courseRepo.UpdateReviewAsync(review);
        }

        public async Task<List<Review>> GetReviewsByCourseIdAsync(int courseId)
        {
            return await _courseRepo.GetReviewsByCourseIdAsync(courseId);
        }

        public async Task DeleteReviewAsync(Review review)
        {
            await _courseRepo.DeleteReviewAsync(review);
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            return await _courseRepo.GetEnrollmentsByCourseIdAsync(courseId);
        }

        public async Task DeleteEnrollmentAsync(Enrollment enrollment)
        {
            await _courseRepo.DeleteEnrollmentAsync(enrollment);
        }

        public async Task<int> AddDocumentAsync(Document document)
        {
            await _courseRepo.AddDocumentAsync(document);
            return document.DocumentID;
        }
    }
}
