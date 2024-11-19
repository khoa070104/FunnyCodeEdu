namespace FuCommunityWebModels.ViewModels
{
    public class DashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalPosts { get; set; }
        public decimal TotalAmount { get; set; }
        public int[] MonthlyUserRegistrations { get; set; } // Thêm thuộc tính này
    }
}
