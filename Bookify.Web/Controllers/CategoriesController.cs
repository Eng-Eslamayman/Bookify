using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Data;
using Bookify.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _Context;

        public CategoriesController(ApplicationDbContext context)
        {
            _Context = context;
        }

        public IActionResult Index()
        {
            return View(_Context.Categories.ToList());
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


            var category = new Category { Name = model.Name };
            _Context.Categories.Add(category);
            _Context.SaveChanges();
            return PartialView("_CategoryRow",category);
        }
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _Context.Categories.Find(id);
            if (category is null)
                return NotFound();

            var viewModel = new CategoryFormViewModel
            {
                Id = id,
                Name = category.Name,
            };
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id ,CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _Context.Categories.Find(id);

            category.Name = model.Name;
            category.LastUpdatedOn = DateTime.Now;

            _Context.SaveChanges();
            return PartialView("_CategoryRow", category);
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
    }
}
