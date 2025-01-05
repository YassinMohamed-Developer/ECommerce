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
    public class CategoryRepository : Repositary<Category>,ICategoryRepository
    {
        public CategoryRepository(ECommerceContext context) : base(context)
        {
        }
    }
}
