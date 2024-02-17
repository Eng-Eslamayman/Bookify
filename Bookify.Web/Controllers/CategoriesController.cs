namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _Context;
        private readonly IMapper _mapper;

        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _Context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var categories = _Context.Categories.AsNoTracking().ToList();

            var viewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            return View(viewModel);
        }
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();


            var category = _mapper.Map<Category>(model);
            _Context.Categories.Add(category);
            _Context.SaveChanges();

            var viewModel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow",viewModel);
        }
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _Context.Categories.Find(id);
            if (category is null)
                return NotFound();

            var viewModel = _mapper.Map<CategoryFormViewModel>(category);
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id ,CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _Context.Categories.Find(id);

            category = _mapper.Map(model, category);
            category.LastUpdatedOn = DateTime.Now;

            _Context.SaveChanges();
            var viewModel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var category = _Context.Categories.Find(id);

            if (category is null)
                return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;

            _Context.SaveChanges();

            return Ok(category.LastUpdatedOn.ToString());
        }
        public IActionResult AllowItem(CategoryFormViewModel model)
        {
            var category = _Context.Categories.SingleOrDefault(c => c.Name == model.Name);
            var isAllowed = category is null || category.Id == model.Id; 
            return Json(isAllowed);
        }
    }
}
