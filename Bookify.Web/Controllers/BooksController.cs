using Bookify.Web.Core.Consts;
using Bookify.Web.Core.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Processing;
using System.Linq.Dynamic.Core;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly IMapper _mapper;
        readonly IWebHostEnvironment _environment;
        readonly Cloudinary _cloudinary;

        readonly List<string> _allowedExtension = new() { ".png", ".jpeg", ".jpg" };
        readonly int _maxAllowedMaxSize = 2097152;

        public BooksController(ApplicationDbContext context, IMapper mapper
            ,IWebHostEnvironment environment, IOptions<CloudinarySettings> cloudinary)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;

            Account account = new()
            {
                Cloud = cloudinary.Value.Cloud,
                ApiKey = cloudinary.Value.ApiKey,
                ApiSecret = cloudinary.Value.ApiSecret
            };
            _cloudinary = new Cloudinary(account);
        }

        public IActionResult Index()
        {
           return View();
        }

        [HttpPost]
        public IActionResult GetBooks()
        {
            var skip = int.Parse(Request.Form["start"]!);
            var pageSize = int.Parse(Request.Form["length"]!);

            var searchValue = Request.Form["search[value]"];
            var sortColumnIndex = Request.Form["order[0][column]"];
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"];
            var sortColumnDirection = Request.Form["order[0][dir]"];

            IQueryable<Book> books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if(!string.IsNullOrEmpty(searchValue))
                books = books.Where(b => b.Title.Contains(searchValue) ||  b.Author!.Name.Contains(searchValue));

            books = books.OrderBy($"{sortColumn} {sortColumnDirection}");
            var data = books.Skip(skip).Take(pageSize).ToList();

            var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);

            var recordsTotal = books.Count();
            var jsonData = new { recordsFilters = recordsTotal, recordsTotal, data = mappedData };
            return Ok(jsonData);
        }
        public IActionResult Details(int id)
        {
            var book = _context.Books
                .Include(a => a.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .SingleOrDefault(b => b.Id == id);

            if (book is null)
                return NotFound();
            var viewModel = _mapper.Map<BookViewModel>(book);

            return View(viewModel);
        }
        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
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
                var path = Path.Combine($"{_environment.WebRootPath}/images/books", imageName);
                var thumbPath = Path.Combine($"{_environment.WebRootPath}/images/books/thumb", imageName);

                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);

                stream.Dispose();  
                book.ImageUrl = $"/images/books/{imageName}";
                book.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";

                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var height = image.Height / ratio;
                image.Mutate(i => i.Resize(200, (int)height));

                image.Save(thumbPath);
                #region save image in cloud
                //using var stream = model.Image.OpenReadStream();

                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(imageName, stream),
                //    UseFilename = true
                //};

                //var result = await _cloudinary.UploadAsync(imageParams);
                //book.ImageUrl = result.SecureUri.ToString();
                //book.ImageThumbailUrl = GetThumbailUrl(book.ImageUrl);
                //book.ImagePublicId = result.PublicId;
                #endregion
            }
            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });
                  
            _context.Books.Add(book);
            _context.SaveChanges();
            return RedirectToAction(nameof(Details),new { id = book.Id });
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
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {

            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _context.Books.Include(c => c.Categories).SingleOrDefault(b => b.Id == model.Id);
            if (book is null)
                return NotFound();

            //string imagePublicId = null;
            if (model.Image is not null)
            {
                if(!string.IsNullOrEmpty(book.ImageUrl))
                {
                    var oldPath = $"{_environment.WebRootPath}{book.ImageUrl}";
                    var oldThumbPath = $"{_environment.WebRootPath}{book.ImageThumbnailUrl}";

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    if (System.IO.File.Exists(oldThumbPath))
                        System.IO.File.Delete(oldThumbPath);
                    //await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
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
                var path = Path.Combine($"{_environment.WebRootPath}/images/books", imageName);
                var thumbPath = Path.Combine($"{_environment.WebRootPath}/images/books/thumb", imageName);

                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                stream.Dispose();

                model.ImageUrl = $"/images/books/{imageName}";
                model.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";

                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var height = image.Height / ratio;
                image.Mutate(i => i.Resize(200, (int)height));

                image.Save(thumbPath);
                #region edit image in cloud
                //using var stream = model.Image.OpenReadStream();

                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(imageName, stream),
                //    UseFilename = true
                //};

                //var result = await _cloudinary.UploadAsync(imageParams);
                //book.ImageUrl = result.SecureUri.ToString();

                //imagePublicId = result.PublicId;
                #endregion 
            }

            else if(!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;
                model.ImageThumbnailUrl = book.ImageThumbnailUrl;
            }
            book = _mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;
            //book.ImageThumbailUrl = GetThumbailUrl(book.ImageUrl);
            //book.ImagePublicId = imagePublicId;

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _context.SaveChanges();
            return RedirectToAction(nameof(Details),new { id = book.Id});
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.Find(id);

            if (book is null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return Ok();
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

        private string GetThumbailUrl(string url)
        {
            var seperator = "image/upload/";
            var urlParts = url.Split(seperator);

            var thumbnailUrl = $"{urlParts[0]}{seperator}c_thumb,w_200,g_face/{urlParts[1]}";
            return thumbnailUrl;
        }
    }
}
