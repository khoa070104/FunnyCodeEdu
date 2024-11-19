using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.Models
{
    public class VnPaySettings
    {
        public string Url { get; set; }
        public string Api { get; set; }
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string Returnurl { get; set; }
    }
}
