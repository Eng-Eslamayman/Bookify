using Bookify.Web.Core.Consts;
using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly IMapper _mapper;
        readonly IWebHostEnvironment _environment;

        readonly List<string> _allowedExtension = new() { ".png", ".jpeg", ".jpg" };
        readonly int _maxAllowedMaxSize = 2097152;

        public BooksController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }

        public IActionResult Index()
        {
           return View();
        } 
        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form",PopulateViewModel(model));

            var book = _mapper.Map<Book>(model);


            if(model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);

                if (!_allowedExtension.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
                    return View("Form", PopulateViewModel(model));
                }
                if(model.Image.Length > _maxAllowedMaxSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }
                var imageName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine($"{_environment.WebRootPath}/Images/Books",imageName);

                using var stream = System.IO.File.Create(path);
                model.Image.CopyTo(stream);

                book.ImageUrl = imageName;
            }
            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });
                  
            _context.Books.Add(book);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);
            var isAllowed = book is null || book.Id == model.Id;
            return Json(isAllowed);
        }

        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(c => c.Categories).SingleOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateViewModel(model);

            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookFormViewModel model)
        {

            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _context.Books.Include(c => c.Categories).SingleOrDefault(b => b.Id == model.Id);
            if (book is null)
                return NotFound();

            if (model.Image is not null)
            {
                if(!string.IsNullOrEmpty(book.ImageUrl))
                {
                    var oldPath = Path.Combine($"{_environment.WebRootPath}/Images/Books", book.ImageUrl);

                    if(System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);


                }
                var extension = Path.GetExtension(model.Image.FileName);

                if (!_allowedExtension.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
                    return View("Form", PopulateViewModel(model));
                }
                if (model.Image.Length > _maxAllowedMaxSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }
                var imageName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine($"{_environment.WebRootPath}/Images/Books", imageName);

                using var stream = System.IO.File.Create(path);
                model.Image.CopyTo(stream);

                model.ImageUrl = imageName;
            }
           
            else if(model.Image is null && !string.IsNullOrEmpty(book.ImageUrl))
                model.ImageUrl = book.ImageUrl;
            book = _mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;
            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            return viewModel;
        }

        
    }
}
