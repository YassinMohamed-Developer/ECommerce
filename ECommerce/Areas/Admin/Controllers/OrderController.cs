using ECommerce.Data.Entity;
using ECommerce.Repositary.Interface;
using ECommerce.Utility;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Security.Claims;

namespace ECommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVm OrderVm { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int orderId)
        {
            OrderVm = new()
            {
                OrderHeader = await _unitOfWork.OrderHeaderRepository.GetAsync(x => x.Id == orderId
                ,include: "Applicationuser"),

                OrderDetail = await _unitOfWork.OrderDetail.GetAllAsyncWithExpression(x => x.OrderHeaderId == orderId
                , include: "Product")
            };

            return View(OrderVm);
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public async Task<IActionResult> UpdateOrderDetail()
        {

            var orderheaderformdb = await _unitOfWork.OrderHeaderRepository.GetAsync(x => x.Id == OrderVm.OrderHeader.Id);

            orderheaderformdb.Name = OrderVm.OrderHeader.Name;
            orderheaderformdb.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
            orderheaderformdb.City = OrderVm.OrderHeader.City;
            orderheaderformdb.PostalCode = OrderVm.OrderHeader.PostalCode;
            orderheaderformdb.State = OrderVm.OrderHeader.State;
            orderheaderformdb.StreetAddress = OrderVm.OrderHeader.StreetAddress;

            if (!string.IsNullOrEmpty(OrderVm.OrderHeader.Carrier))
            {
                orderheaderformdb.Carrier = OrderVm.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVm.OrderHeader.TrackingNumber))
            {
                orderheaderformdb.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
            }

                _unitOfWork.OrderHeaderRepository.Update(orderheaderformdb);
                _unitOfWork.Save();
                TempData["Success"] = "Order Details Updated Successfully.";

                return RedirectToAction(nameof(Details), new { orderId = orderheaderformdb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public  IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVm.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            var orderheaderfromdb = await _unitOfWork.OrderHeaderRepository.GetAsync(x => x.Id == OrderVm.OrderHeader.Id);

            orderheaderfromdb.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
            orderheaderfromdb.Carrier = OrderVm.OrderHeader.Carrier;
            orderheaderfromdb.OrderStatus = SD.StatusShipped;
            orderheaderfromdb.ShippingDate = DateTime.Now;
            if(orderheaderfromdb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderheaderfromdb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeaderRepository.Update(orderheaderfromdb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> CancelOrder()
        {
            var orderheaderfromdb = await _unitOfWork.OrderHeaderRepository.GetAsync(x => x.Id == OrderVm.OrderHeader.Id);

            if(orderheaderfromdb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderheaderfromdb.PaymentIntentId,
                };

                var service = new RefundService();

                Refund refund = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderheaderfromdb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderheaderfromdb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public async Task<IActionResult> Details_PAY_NOW()
        {

            OrderVm = new()
            {
                OrderHeader = await _unitOfWork.OrderHeaderRepository.GetAsync(x => x.Id == OrderVm.OrderHeader.Id
                , include: "Applicationuser"),

                OrderDetail = await _unitOfWork.OrderDetail.GetAllAsyncWithExpression(x => x.OrderHeaderId == OrderVm.OrderHeader.Id
                , include: "Product")
            };

            //STRIPE LOGIC
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVm.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVm.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVm.OrderDetail)
            {
                var sessionlineitem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency="usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        }
                    },
                    Quantity = item.Count,
                };

                options.LineItems.Add(sessionlineitem);
            }

            var service = new SessionService();
             Session session = service.Create(options);
            _unitOfWork.OrderHeaderRepository.UpdateStripePayment(OrderVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentConfirmation(int orderHeaderId)
        {
            var orderheaderfromdb = await _unitOfWork.OrderHeaderRepository.GetAsync(x => x.Id == orderHeaderId);

            if(orderheaderfromdb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderheaderfromdb.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePayment(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderId, orderheaderfromdb.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

            #region Api Calls

            [HttpGet]
        public async Task<IActionResult> GetAllAsync(string status)
        {
            IEnumerable<OrderHeader> orderheader;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderheader = await _unitOfWork.OrderHeaderRepository.GetAllAsync(include: "Applicationuser");
            }
            else
            {
                var claimsIdetity = (ClaimsIdentity)User.Identity;
                var userid = claimsIdetity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderheader = await _unitOfWork.OrderHeaderRepository.GetAllAsyncWithExpression(x => x.ApplicationUserId == userid, 
                    include: "Applicationuser");
            }


            switch (status)
            {
                case "pending":
                    orderheader = orderheader.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment);
                        break;
                case "inprocess":
                    orderheader = orderheader.Where(x => x.OrderStatus == SD.StatusInProcess);
                        break;
                case "completed":
                    orderheader = orderheader.Where(x => x.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderheader = orderheader.Where(x => x.OrderStatus == SD.StatusApproved);
                    break;

                default:
                    break;
            }

            return Json(new {data = orderheader});
        }

        #endregion
    }
}
