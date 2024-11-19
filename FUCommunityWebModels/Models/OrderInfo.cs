using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.Models
{
    public class OrderInfo
    {
        [Key]
        public long OrderId { get; set; }

        public long Amount { get; set; }

        public string? OrderDesc { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        public string Status { get; set; }

        public long PaymentTranId { get; set; }

        public string? BankCode { get; set; }

        public string? PayStatus { get; set; }
        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }
    }
}
