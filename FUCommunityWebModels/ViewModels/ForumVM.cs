using FuCommunityWebModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.ViewModels
{
    public class ForumVM
    {
        public List<Post> Posts { get; set; }
        public List<Category> Categories { get; set; }  // Danh sách các danh mục
        public CreateCategoryVM CreateCategory { get; set; }  // ViewModel cho tạo danh mục
        public EditCategoryVM EditCategory { get; set; }  // ViewModel cho chỉnh sửa danh mục
    }
}
