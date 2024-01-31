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
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            return View(model);

            var category = new Category { Name = model.Name};
            _Context.Categories.Add(category);
            _Context.SaveChanges();
            return RedirectToAction(nameof(Index));  
        }
    }
}
