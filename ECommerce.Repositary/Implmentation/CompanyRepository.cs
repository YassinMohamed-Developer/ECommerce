using ECommerce.Data.DatabaseContext;
using ECommerce.Data.Entity;
using ECommerce.Repositary.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Repositary.Implmentation
{
    public class CompanyRepository : Repositary<Company>, ICompanyRepository
    {
        public CompanyRepository(ECommerceContext context) : base(context)
        {
        }
    }
}
