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
    public class ProductRepository : Repositary<Product>, IProductRepository
    {
        public ProductRepository(ECommerceContext context) : base(context)
        {
        }
    }
}
