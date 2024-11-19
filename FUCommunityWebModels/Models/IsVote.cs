using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.Models
{
    public class IsVote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IsVoteID { get; set; }

        public string UserID { get; set; } // Thay đổi kiểu dữ liệu cho UserID

        public int PostID { get; set; }

        public int Point { get; set; }

        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; } // Thay đổi từ User thành ApplicationUser

        [ForeignKey("PostID")]
        public virtual Post Post { get; set; }
    }
}
