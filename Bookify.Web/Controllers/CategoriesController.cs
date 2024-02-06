using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModels;
using Bookify.Web.Data;
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

        public IActionResult Create()
        {
            return View("Form");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = new Category { Name = model.Name };
            _Context.Categories.Add(category);
            _Context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

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
            return View("Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id ,CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
              return View("Form",model);

            var category = _Context.Categories.Find(id);

            category.Name = model.Name;
            category.LastUpdatedOn = DateTime.Now;

            _Context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
