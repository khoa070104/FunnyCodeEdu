using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.Models
{
    public class Deposit
    {
        [Key]
        public int DepositID { get; set; }  // Khóa chính tự tăng

        public string UserID { get; set; }  // ID của người dùng

        public decimal Money { get; set; }  // Số tiền gửi

        public DateTime CreatedDate { get; set; } = DateTime.Now;  // Ngày tạo

        [ForeignKey("UserID")] // Khóa ngoại tham chiếu đến ApplicationUser
        public virtual ApplicationUser User { get; set; }  // Tạo mối quan hệ với ApplicationUser
    }
}
