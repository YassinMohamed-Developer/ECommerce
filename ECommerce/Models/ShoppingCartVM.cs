using ECommerce.Data.Entity;

namespace ECommerce.Web.Models
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; } = [];

        public double OrderTotal {  get; set; }

        public OrderHeader OrderHeader { get; set; }
    }
}
