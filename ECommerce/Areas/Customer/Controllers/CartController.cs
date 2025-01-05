using ECommerce.Data.Entity;
using ECommerce.Repositary.Interface;
using ECommerce.Utility;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;

namespace ECommerce.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;


        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var claims = (ClaimsIdentity)User.Identity;

            var UserId = claims.FindFirst(ClaimTypes.NameIdentifier).Value;

            var Carts = new ShoppingCartVM()
            {
                ShoppingCartList = await _unitOfWork.ShoppingCartRepository.GetAllAsyncWithExpression(
                    x => x.ApplicationuserId == UserId,
                    include: "Product")
            };

            foreach (var cart in Carts.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                Carts.OrderTotal += (cart.Price * cart.Count);
            }

            return View(Carts);
        }

        [HttpGet]
        public async Task<IActionResult> Summary()
        {
            var claims = (ClaimsIdentity)User.Identity;

            var UserId = claims.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = await _unitOfWork.ShoppingCartRepository.GetAllAsyncWithExpression(x => x.ApplicationuserId == UserId
                , include: "Product"),

                OrderHeader = new()
            };


            Applicationuser applicationuser = await _unitOfWork.ApplicationUserRepository.GetAsync(x => x.Id == UserId);
			ShoppingCartVM.OrderHeader.Name = applicationuser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = applicationuser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = applicationuser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = applicationuser.City;
			ShoppingCartVM.OrderHeader.State = applicationuser.State;
			ShoppingCartVM.OrderHeader.PostalCode = applicationuser.PostalCode;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost()
        {
            var claims = (ClaimsIdentity)User.Identity;
            var UserId = claims.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = await _unitOfWork.ShoppingCartRepository.GetAllAsyncWithExpression(x => x.ApplicationuserId == UserId,
                include:"Product");

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = UserId;

            var applicationuser = await _unitOfWork.ApplicationUserRepository.GetAsync(x => x.Id == UserId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationuser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer 
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //it is a company user
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            await _unitOfWork.OrderHeaderRepository.AddAsync(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                await _unitOfWork.OrderDetail.AddAsync(orderDetail);
                _unitOfWork.Save();
            }


            if (applicationuser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment
                //stripe logic
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeaderRepository.UpdateStripePayment(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }


            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }


        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeaderRepository.GetAsync(x => x.Id == id, include: "Applicationuser");

            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePayment(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }

            IEnumerable<ShoppingCart> shoppingCarts = await _unitOfWork.ShoppingCartRepository
                .GetAllAsyncWithExpression(x => x.ApplicationuserId == orderHeader.ApplicationUserId);

            _unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(id);
        }
        public async Task<IActionResult> Remove(int CartId)
        {
            var cart = await _unitOfWork.ShoppingCartRepository.GetAsync(x => x.Id == CartId,Istracked:true);
            _unitOfWork.ShoppingCartRepository.Remove(cart);

            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCartRepository.GetAllWithExpression(x => x.ApplicationuserId == cart.ApplicationuserId).Count() - 1);

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int CartId)
        {
            var cart = await _unitOfWork.ShoppingCartRepository.GetAsync(x => x.Id == CartId);
            cart.Count += 1;
            _unitOfWork.ShoppingCartRepository.Update(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int CartId)
        {
            var cart = await _unitOfWork.ShoppingCartRepository.GetAsync(x => x.Id == CartId);
            if(cart.Count <= 1)
            {
                _unitOfWork.ShoppingCartRepository.Remove(cart);

                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCartRepository.GetAllWithExpression(x => x.ApplicationuserId == cart.ApplicationuserId)
                    .Count() - 1);
            }
            else
            {
                cart.Count -= 1;
                _unitOfWork.ShoppingCartRepository.Update(cart);

            }
             _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }



        #region Private Methdos
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }

            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            } 
            #endregion

        }
    }
}
