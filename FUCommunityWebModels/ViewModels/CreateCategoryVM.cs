using System.ComponentModel.DataAnnotations;

namespace FuCommunityWebModels.ViewModels
{
    public class CreateCategoryVM
    {
        [Required(ErrorMessage = "Vui lòng nhập title")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập description")]
        public string Description { get; set; }
    }
}
