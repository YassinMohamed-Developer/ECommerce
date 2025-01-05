using ECommerce.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Repositary.Interface
{
    public interface IOrderHeaderRepository : IRepositary<OrderHeader>
    {
        void UpdateStatus(int Id, string orderStatus, string? paymentstatus = null);

        void UpdateStripePayment(int Id,string SessionId ,string paymentIntentId);
    }
}
