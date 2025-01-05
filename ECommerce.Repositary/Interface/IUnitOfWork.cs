using ECommerce.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Repositary.Interface
{
    // TODO:Enhancement the UnitOfWork Same AS Project StudGo
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }
        IProductRepository ProductRepository { get; }

        ICompanyRepository CompanyRepository { get; }

        IShoppingCartRepository ShoppingCartRepository { get; }

        IOrderHeaderRepository OrderHeaderRepository { get; }

        IRepositary<OrderDetail> OrderDetail {  get; }

        IRepositary<Applicationuser> ApplicationUserRepository { get; }

        void Save();
    }
}
