using ECommerce.Data.Entity;
using ECommerce.Repositary.Interface;
using ECommerce.Utility;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> products = await _unitOfWork.ProductRepository.GetAllAsync(include: "Category");
            return View(products);
        }
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            ProductViewModel productViewModel = new()
            {
                CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create
                return View(productViewModel);
            }
            else
            {
                //update
                productViewModel.Product = await _unitOfWork.ProductRepository.GetAsync(u => u.Id == id);
                return View(productViewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(ProductViewModel productVm,IFormFile? files)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                        //Naming File That Store in the WWWROOT
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(files.FileName);
                        //Get Path Of the Image in the product
                        string productPath = Path.Combine(wwwRootPath, @"Images\Product");

                    if (!string.IsNullOrEmpty(productVm.Product.ImageUrl))
                    {
                        //Delete Image
                        var oldimagePath = Path.Combine(wwwRootPath, productVm.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldimagePath))
                        {
                            System.IO.File.Delete(oldimagePath);
                        }
                    }
                        using(var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            files.CopyTo(fileStream);
                        }
                        productVm.Product.ImageUrl = @"\Images\Product\" + fileName;
                }

                if(productVm.Product.Id == 0)
                {
                    await _unitOfWork.ProductRepository.AddAsync(productVm.Product);
                    _unitOfWork.Save();
                    TempData["Success"] = "Product Created Successfully";
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(productVm.Product);
                    _unitOfWork.Save();
                    TempData["Success"] = "Product Updated Successfully";
                }
                    return RedirectToAction(nameof(Index));
            }
            return View();
        }
        #region Delete Action but Commented
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //        return NotFound();

        //    var FindProduct = await _unitOfWork.ProductRepository.GetAsync(x => x.Id == id, include: "Category");

        //    if(FindProduct == null)
        //        return NotFound();

        //    return View(FindProduct);
        //}
        //[HttpPost,ActionName("Delete")]
        //public async Task<IActionResult> DeletePost(int? id)
        //{
        //    var FindProduct = await _unitOfWork.ProductRepository.GetAsync(x => x.Id == id, include: "Category");

        //    if(FindProduct == null)
        //        return NotFound();

        //    FindProduct.IsDeleted = true;
        //    _unitOfWork.ProductRepository.Update(FindProduct);
        //    _unitOfWork.Save();
        //    TempData["Success"] = "Category Deleted Successfully";
        //    return RedirectToAction(nameof(Index));
        //} 
        #endregion

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            IEnumerable<Product> products = await _unitOfWork.ProductRepository.GetAllAsync(include: "Category");
            return Json(new { data = products });
        }
        public async Task<IActionResult> Delete(int? id)
        {
            var product = await _unitOfWork.ProductRepository.GetAsync(x => x.Id == id);

            if(product == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var oldimagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldimagePath))
                {
                    System.IO.File.Delete(oldimagePath);
                }
            }

            product.IsDeleted = true;
            _unitOfWork.ProductRepository.Update(product);
            _unitOfWork.Save();
            TempData["Success"] = "Product Deleted Successfully";
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
                
    }
}
