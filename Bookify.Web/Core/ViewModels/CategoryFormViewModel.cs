using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(50,ErrorMessage = "Max length cannot be more than 50 chr.")]
        [Remote("AllowItem", null,AdditionalFields ="Id", ErrorMessage = "Category with the same name is already exists!")]

        public string Name { get; set; } = null!;

    }
}
