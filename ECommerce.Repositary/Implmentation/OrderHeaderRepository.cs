using ECommerce.Data.DatabaseContext;
using ECommerce.Data.Entity;
using ECommerce.Repositary.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Repositary.Implmentation
{
    public class OrderHeaderRepository : Repositary<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ECommerceContext _context;

        public OrderHeaderRepository(ECommerceContext context) : base(context)
        {
            _context = context;
        }

        public void UpdateStatus(int Id, string orderStatus, string? paymentstatus = null)
        {
            var orderfromDb = _context.OrderHeaders.FirstOrDefault(x => x.Id == Id);

            if (orderfromDb != null)
            {
                orderfromDb.OrderStatus = orderStatus;

                if (!string.IsNullOrEmpty(paymentstatus))
                {
                    orderfromDb.PaymentStatus = paymentstatus;
                }
            }
        }

        public void UpdateStripePayment(int Id, string SessionId, string paymentIntentId)
        {
            var orderfromDb = _context.OrderHeaders.FirstOrDefault(x => x.Id == Id);
            if(!string.IsNullOrEmpty(paymentIntentId))
            {
                orderfromDb.PaymentIntentId = paymentIntentId;
                orderfromDb.PaymentDate = DateTime.Now;
            }
            if (!string.IsNullOrEmpty(SessionId))
            {
                orderfromDb.SessionId = SessionId;
            }
        }
    }
}
