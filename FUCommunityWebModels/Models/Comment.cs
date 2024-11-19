using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuCommunityWebModels.Models
{
	public class Comment
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentID { get; set; }

        public int? PostID { get; set; }

        public int? QuestionID { get; set; }

        public string UserID { get; set; }

        public string Content { get; set; }
        public int ReplyID { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("PostID")]
        public Post Post { get; set; }

        [ForeignKey("QuestionID")]
        public Question Question { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }
    }
}
