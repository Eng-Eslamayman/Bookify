﻿using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Controllers
{
	public class SubscribersController : Controller
	{
		readonly ApplicationDbContext _context;
        readonly IDataProtector _dataProtector;
        readonly IMapper _mapper;
        readonly IImageService _imageService;

		public SubscribersController(ApplicationDbContext context, IDataProtectionProvider dataProtector ,IMapper mapper, IImageService imageService)
		{
			_context = context;
			_dataProtector = dataProtector.CreateProtector("MySecureKey");
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

            var subscriber = _mapper.Map<Subscriber>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
            var imagePath = "/images/subscribers";

            var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

            if (!isUploaded)
            {
                ModelState.AddModelError("Image", errorMessage!);
                return View("Form", PopulateViewModel(model));
            }

            subscriber.ImageUrl = $"{imagePath}/{imageName}";
            subscriber.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
            subscriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.Add(subscriber);
            _context.SaveChanges();

			//TODO: Send welcome email
			var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());

			return RedirectToAction(nameof(Details), new { id = subscriberId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = _context.Subscribers
                            .SingleOrDefault(s =>
                                    s.Email == model.Value
                                || s.NationalId == model.Value
                                || s.MobileNumber == model.Value);

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            if (subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }
        public IActionResult Edit(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = _context.Subscribers
                .Find(subscriberId);

            if (subscriber is null)
                return NotFound();

            var model = _mapper.Map<SubscriberFormViewModel>(subscriber);
            var viewModel = PopulateViewModel(model);
            viewModel.Key = id;

            return View("Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if(!ModelState.IsValid)
                return View("Form",PopulateViewModel(model));

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.Find(subscriberId);
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

            return RedirectToAction(nameof(Details), new { id = model.Key });
        }

        [HttpGet]
        public IActionResult Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));

			var subscriber = _context.Subscribers
                .Include(g => g.Governorate)
                .Include(a => a.Area)
                .SingleOrDefault(s => s.Id == subscriberId);
            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;
            return View(viewModel);
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
            var subscriberId = 0;
            if(!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(b => b.NationalId == model.NationalId);
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isAllowed);
        }

        public IActionResult AllowMobileNumber(SubscriberFormViewModel model)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(b => b.MobileNumber == model.MobileNumber);
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isAllowed);
        }

        public IActionResult AllowEmail(SubscriberFormViewModel model)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(b => b.Email == model.Email);
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

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
