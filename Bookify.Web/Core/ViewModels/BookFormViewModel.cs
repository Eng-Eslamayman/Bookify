using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(500)]
        public string Title { get; set; } = null!;
        [Display(Name = "Author")]
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
        public IEnumerable<SelectListItem>? Authors { get; set; }
        [MaxLength(200)]
        public string Publisher { get; set; } = null!;
        [Display(Name = "Publishing Date")]
        public DateTime PublishingDate { get; set; } = DateTime.Now;
        public IFormFile? Image { get; set; }
        [MaxLength(200)]
        public string Hall { get; set; } = null!;
        [Display(Name = "Is available for rental?")]

        public bool IsAvailableForRental { get; set; }
        public string Description { get; set; } = null!;
        public IList<int> SelectedCategories { get; set; } = new List<int>();
        public IEnumerable<SelectListItem>? Categories { get; set; }

    }
}
