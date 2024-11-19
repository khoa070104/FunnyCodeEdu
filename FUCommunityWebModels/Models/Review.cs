using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }  // Khóa chính tự tăng

        public string UserID { get; set; }  // ID của người dùng

        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }  // Tạo mối quan hệ với bảng ApplicationUser

        public int CourseID { get; set; }  // ID của khóa học
        [ForeignKey("CourseID")]
        public Course Course { get; set; }

        public string Content { get; set; }  // Nội dung đánh giá

        [Range(1, 5)]
        public int Rating { get; set; }  // Ràng buộc rating chỉ từ 1 đến 5

        public DateTime CreateDate { get; set; } = DateTime.Now;  // Ngày tạo đánh giá
    }
}
