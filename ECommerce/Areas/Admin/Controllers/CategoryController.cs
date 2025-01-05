using ECommerce.Data.DatabaseContext;
using ECommerce.Data.Entity;
using ECommerce.Repositary.Interface;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var listOfCategory = await _unitOfWork.CategoryRepository.GetAllAsync();
            return View(listOfCategory);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Display Order Doesn't exactly match Name");
            }
            if (ModelState.IsValid)
            {
                await _unitOfWork.CategoryRepository.AddAsync(category);
                _unitOfWork.Save();
                TempData["Success"] = "Category Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var FindCategory = await _unitOfWork.CategoryRepository.GetAsync(u => u.Id == id);

            if (FindCategory == null)
                return NotFound();

            return View(FindCategory);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepository.Update(category);
                _unitOfWork.Save();
                TempData["Success"] = "Category Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var FindCategory = await _unitOfWork.CategoryRepository.GetAsync(u => u.Id == id);

            if (FindCategory == null)
                return NotFound();

            return View(FindCategory);
        }

        [HttpPost, ActionName("Delete")]

        public async Task<IActionResult> DeletePost(int? Id)
        {
            var FindCategory = await _unitOfWork.CategoryRepository.GetAsync(u => u.Id == Id);

            if (FindCategory == null)
            {
                return NotFound();
            }
            FindCategory.IsDeleted = true;

            _unitOfWork.CategoryRepository.Update(FindCategory);
            TempData["Success"] = "Category Deleted Successfully";
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

    }
}
