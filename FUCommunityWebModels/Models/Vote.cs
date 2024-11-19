using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.Models
{
	public class Vote
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoteID { get; set; }

        public string UserID { get; set; } // Thay đổi kiểu dữ liệu cho UserID
        public int? PostID { get; set; }

        [MaxLength(10)]
        public string VoteType { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? VotedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; } // Thay đổi từ User thành ApplicationUser

        [ForeignKey("PostID")]
        public virtual Post Post { get; set; }
    }
}
