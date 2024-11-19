using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.Models
{
    public class Enrollment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnrollmentID { get; set; }

        public string UserID { get; set; } // Thay đổi kiểu dữ liệu cho UserID

        public int CourseID { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Status { get; set; }

        public int? DocumentID { get; set; }

        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; } // Thay đổi từ User thành ApplicationUser

        [ForeignKey("CourseID")]
        public virtual Course Course { get; set; }

        [ForeignKey("DocumentID")]
        public virtual Document Document { get; set; }
    }
}
