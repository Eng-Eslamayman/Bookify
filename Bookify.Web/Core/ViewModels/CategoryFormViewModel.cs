namespace Bookify.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(50,ErrorMessage = "Max length cannot be more than 50 chr.")]
        public string Name { get; set; } = null!;

    }
}
