using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuCommunityWebModels.Models
{
    public class Follower
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FollowId { get; set; } // ID của người được follow

        [Required]
        public string UserID { get; set; } // ID của người follow

        [ForeignKey("FollowId")]
        public virtual ApplicationUser FollowedUser { get; set; }

        [ForeignKey("UserID")]
        public virtual ApplicationUser FollowingUser { get; set; }
    }
}
