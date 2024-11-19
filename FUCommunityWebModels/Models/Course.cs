using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace FuCommunityWebModels.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseID { get; set; }

        public string UserID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(255)]
        public string CourseImage { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
        public int Semester { get; set; }

        public int CategoryID { get; set; }
        public int? DocumentID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }
        
        [ForeignKey("DocumentID")]
        public virtual Document Document { get; set; }

        public virtual ICollection<Lesson> Lessons { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        [NotMapped]
        public double Rate
        {
            get
            {
                if (Reviews == null || !Reviews.Any())
                    return 0;
                return Reviews.Average(r => r.Rating);
            }
        }
    }
}
