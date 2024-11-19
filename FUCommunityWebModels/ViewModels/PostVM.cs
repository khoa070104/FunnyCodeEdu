using FuCommunityWebModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuCommunityWebModels.ViewModels
{
    public class PostVM
    {
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<UserVM> Users { get; set; } = new List<UserVM>();
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public CreatePostVM CreatePostVM { get; set; } = new CreatePostVM();
        public CategoryVM CategoryVM { get; set; } = new CategoryVM();
        public Post Post { get; set; } = new Post();
        public Comment Comment { get; set; } = new Comment();
        public Point Point { get; set; } = new Point();
        public int VoteCount { get; set; }
        public UserVM UserVM { get; set; } = new UserVM();
    }
}
