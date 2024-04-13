namespace Bookify.Web.Controllers
{
	[Authorize(Roles = AppRoles.Archive)]
	public class AuthorsController : Controller
	{
		private readonly ApplicationDbContext _Context;
		private readonly IMapper _mapper;

		public AuthorsController(ApplicationDbContext context, IMapper mapper)
		{
			_Context = context;
			_mapper = mapper;
		}

		public IActionResult Index()
		{
			var authors = _Context.Authors.AsNoTracking().ToList();

			var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
			return View(viewModel);
		}
		[AjaxOnly]
		public IActionResult Create()
		{
			return PartialView("_Form");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(AuthorFormViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest();


			var author = _mapper.Map<Author>(model);
			author.LastUpdatedById = User.GetUserId();
			_Context.Authors.Add(author);
			_Context.SaveChanges();

			var viewModel = _mapper.Map<AuthorViewModel>(author);

			return PartialView("_AuthorRow", viewModel);
		}
		[AjaxOnly]
		public IActionResult Edit(int id)
		{
			var author = _Context.Authors.Find(id);
			if (author is null)
				return NotFound();

			var viewModel = _mapper.Map<AuthorFormViewModel>(author);
			return PartialView("_Form", viewModel);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(int id, AuthorFormViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest();

			var author = _Context.Authors.Find(id);

			author = _mapper.Map(model, author);
			author.LastUpdatedById = User.GetUserId();
			author.LastUpdatedOn = DateTime.Now;

			_Context.SaveChanges();
			var viewModel = _mapper.Map<AuthorViewModel>(author);

			return PartialView("_AuthorRow", viewModel);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ToggleStatus(int id)
		{
			var author = _Context.Authors.Find(id);

			if (author is null)
				return NotFound();

			author.IsDeleted = !author.IsDeleted;
			author.LastUpdatedById = User.GetUserId();
			author.LastUpdatedOn = DateTime.Now;

			_Context.SaveChanges();

			return Ok(author.LastUpdatedOn.ToString());
		}
		public IActionResult AllowItem(AuthorFormViewModel model)
		{
			var author = _Context.Authors.SingleOrDefault(c => c.Name == model.Name);
			var isAllowed = author is null || author.Id == model.Id;
			return Json(isAllowed);
		}
	}
}
