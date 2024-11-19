using FuCommunityWebModels.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Document
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DocumentID { get; set; }
    public string UserID { get; set; }

    public int? CourseID { get; set; }

    public int? PostID { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(255)]
    public string FileUrl { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.Now;

    [ForeignKey("UserID")]
    public virtual ApplicationUser User { get; set; }

    [ForeignKey("CourseID")]
    public virtual Course Course { get; set; }

    [ForeignKey("PostID")]
    public virtual Post Post { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; }
}
