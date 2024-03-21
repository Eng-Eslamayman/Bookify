using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Controllers
{
	public class SubscribersController : Controller
	{
		readonly ApplicationDbContext _context;
        readonly IMapper _mapper;
        readonly IImageService _imageService;

        public SubscribersController(ApplicationDbContext context, IMapper mapper, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
        }

        public IActionResult Index()
		{
			return View();
		}
        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var Subscriber = _mapper.Map<Subscriber>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
            var imagePath = "/images/subscribers";

            var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

            if (!isUploaded)
            {
                ModelState.AddModelError("Image", errorMessage!);
                return View("Form", PopulateViewModel(model));
            }

            Subscriber.ImageUrl = $"{imagePath}/{imageName}";
            Subscriber.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
            Subscriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.Add(Subscriber);
            _context.SaveChanges();

            //TODO: Send welcome email

            return RedirectToAction(nameof(Index), new { id = Subscriber.Id });
        }
        public IActionResult Edit(int id)
        {
            var subscriper = _context.Subscribers
                .SingleOrDefault(s => s.Id == id);

            if (subscriper is null)
                return NotFound();

            var model = _mapper.Map<SubscriberFormViewModel>(subscriper);
            var viewModel = PopulateViewModel(model);

            return View("Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if(!ModelState.IsValid)
                return View("Form",PopulateViewModel(model));

            var subscriber = _context.Subscribers.Find(model.Id);
            if (subscriber is null)
                return NotFound();

            if(model.Image is not null)
            {
                if (!string.IsNullOrEmpty(model.ImageUrl))
                    _imageService.Delete(model.ImageUrl, model.ImageThumbnailUrl);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var imagePath = "/images/subscribers";

                var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

                if (!isUploaded)
                {
                    ModelState.AddModelError("Image", errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }

                model.ImageUrl = $"{imagePath}/{imageName}";
                model.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";

            }
            else if (!string.IsNullOrEmpty(subscriber.ImageUrl))
            {
                model.ImageUrl = subscriber.ImageUrl;
                model.ImageThumbnailUrl = subscriber.ImageThumbnailUrl;
            }

            subscriber = _mapper.Map(model, subscriber);
            subscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            subscriber.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new { id = subscriber.Id });
        }
        [AjaxOnly]
		public IActionResult GetAreas(int governorateId)
		{
			var areas = _context.Areas.Where(a => a.GovernorateId == governorateId)
				.Select(g => new SelectListItem
				{
					Value = g.Id.ToString(),
					Text = g.Name
				}).OrderBy(x => x.Text)
				.ToList();

            return Ok(areas);
		}
        public IActionResult AllowNationalId(SubscriberFormViewModel model)
        {
            var Subscriber = _context.Subscribers.SingleOrDefault(b => b.NationalId == model.NationalId);
            var isAllowed = Subscriber is null || Subscriber.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        public IActionResult AllowMobileNumber(SubscriberFormViewModel model)
        {
            var Subscriber = _context.Subscribers.SingleOrDefault(b => b.MobileNumber == model.MobileNumber);
            var isAllowed = Subscriber is null || Subscriber.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        public IActionResult AllowEmail(SubscriberFormViewModel model)
        {
            var Subscriber = _context.Subscribers.SingleOrDefault(b => b.Email == model.Email);
            var isAllowed = Subscriber is null || Subscriber.Id.Equals(model.Id);

            return Json(isAllowed);
        }
        private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
        {
            SubscriberFormViewModel viewModel = model is null ? new SubscriberFormViewModel() : model;

            var governorates = _context.Governorates.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            if (model?.GovernorateId > 0)
            {
                var areas = _context.Areas
                    .Where(a => a.GovernorateId == model.GovernorateId && !a.IsDeleted)
                    .OrderBy(a => a.Name)
                    .ToList();

                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return viewModel;
        }
    }
}
