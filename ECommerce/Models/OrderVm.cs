using ECommerce.Data.Entity;

namespace ECommerce.Web.Models
{
    public class OrderVm
    {
        public OrderHeader OrderHeader { get; set; }

        public IEnumerable<OrderDetail> OrderDetail { get; set; }
    }
}
