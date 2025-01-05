using ECommerce.Data.Entity;
using ECommerce.Repositary.Interface;
using ECommerce.Utility;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var Company = await _unitOfWork.CompanyRepository.GetAllAsync();
            return View(Company);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {

            if(id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
               var Company = await _unitOfWork.CompanyRepository.GetAsync(x => x.Id == id);
                return View(Company);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    await _unitOfWork.CompanyRepository.AddAsync(company);
                    _unitOfWork.Save();
                    TempData["success"] = "Company created successfully";
                }
                else
                {
                    _unitOfWork.CompanyRepository.Update(company);
                    _unitOfWork.Save();
                    TempData["success"] = "Company Updated successfully";
                }

                return RedirectToAction("Index");
            }

            return View(company);
        }

        #region Call API


        public async Task<IActionResult> GetAll()
        {
            var company = await _unitOfWork.CompanyRepository.GetAllAsync();
            return Json(new {Data = company});
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var findcompany = await _unitOfWork.CompanyRepository.GetAsync(x => x.Id == id);

            if(findcompany == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            findcompany.IsDeleted = true;
            _unitOfWork.CompanyRepository.Update(findcompany);
            _unitOfWork.Save();
            TempData["success"] = "Company Delete successfully";

            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion
    }
}
