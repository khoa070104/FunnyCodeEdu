using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.Models
{
    public class Point
    {
        [Key]
        public int PointId { get; set; }
        public string UserID { get; set; }        

        public int PointValue { get; set; }  // Lưu giá trị điểm

        public DateTime CreateDate { get; set; } = DateTime.Now;  // Ngày tạo điểm

        public PointSource From { get; set; }  // Lưu nguồn điểm (Vote hoặc Nạp)

        public bool Status { get; set; }  // Trạng thái điểm đã cộng hay chưa

        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }  // Tạo mối quan hệ với bảng ApplicationUser
    }
    public enum PointSource
    {
        Vote = 1,
        Nap = 2
    }
}
