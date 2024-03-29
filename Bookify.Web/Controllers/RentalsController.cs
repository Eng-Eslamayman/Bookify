﻿using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Controllers
{
    public class RentalsController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly IMapper _mapper;

        public RentalsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies
                .Include(c => c.Book)
                .SingleOrDefault(c => c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted);

            if (copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvilableRental);

            //TODO: check that copy is not in rental

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_CopyDetails", viewModel);
        }
    }
}
