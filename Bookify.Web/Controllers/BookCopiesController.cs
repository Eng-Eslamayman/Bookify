namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly IMapper _mapper;

        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }
        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _context.Books.Find(bookId);

            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                BookId = bookId,
                ShowRentalInput = book.IsAvailableForRental
            };

            return PartialView("Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _context.Books.Find(model.BookId);

            if (book is null)
                return NotFound();

            BookCopy copy = new()
            {
                EditionNumber = model.EditionNumber,
                LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                IsAvailableForRental = book.IsAvailableForRental && model.IsAvailableForRental
            };

            book.Copies.Add(copy);
            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            viewModel.ShowRentalInput = copy.Book!.IsAvailableForRental;

            return PartialView("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && model.IsAvailableForRental;
            copy.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _context.BookCopies.Find(id);

            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            copy.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return Ok();
        }
    }
}
