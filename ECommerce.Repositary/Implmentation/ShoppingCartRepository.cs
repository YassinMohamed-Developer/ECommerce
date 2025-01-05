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
    public class ShoppingCartRepository : Repositary<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(ECommerceContext context) : base(context)
        {
        }
    }
}
