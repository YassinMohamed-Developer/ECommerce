using ECommerce.Data.Entity;
using ECommerce.Models;
using ECommerce.Repositary.Interface;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ECommerce.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync(include:"Category");
            return View(products);
        }

        public async Task<IActionResult> Details(int productId)
        {

            var ShoppingCart = new ShoppingCart
            {
                Product = await _unitOfWork.ProductRepository.GetAsync(x => x.Id == productId, include: "Category"),
                ProductId = productId,
                Count = 1,
            };
            return View(ShoppingCart);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            var claimidentity = (ClaimsIdentity)User.Identity;
            var UserId = claimidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationuserId = UserId;

            var CartFromDb = await _unitOfWork.ShoppingCartRepository.GetAsync(x => x.ApplicationuserId == UserId && 
            x.ProductId == shoppingCart.ProductId);


            if(CartFromDb != null)
            {
                CartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCartRepository.Update(CartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                await _unitOfWork.ShoppingCartRepository.AddAsync(shoppingCart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCartRepository.GetAllWithExpression(x => x.ApplicationuserId == UserId).Count());
            }
            TempData["Success"] = "ShoppingCart Updated Successfully";
            _unitOfWork.Save();
            return RedirectToAction((nameof(Index)));

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
