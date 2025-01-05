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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ECommerceContext _context;
        public ICategoryRepository CategoryRepository {  get; private set; }

        public IProductRepository ProductRepository {  get; private set; }

        public ICompanyRepository CompanyRepository {  get; private set; }

        public IShoppingCartRepository ShoppingCartRepository {  get; private set; }

        public IOrderHeaderRepository OrderHeaderRepository {  get; private set; }

        public IRepositary<OrderDetail> OrderDetail {  get; private set; }

        public IRepositary<Applicationuser> ApplicationUserRepository {  get; private set; }

        public UnitOfWork(ECommerceContext context)
        {
            _context = context;
            CategoryRepository = new CategoryRepository(context);
            ProductRepository = new ProductRepository(context);
            CompanyRepository = new CompanyRepository(context);
            ShoppingCartRepository = new ShoppingCartRepository(context);
            OrderHeaderRepository = new OrderHeaderRepository(context);
            OrderDetail = new Repositary<OrderDetail>(context);
            ApplicationUserRepository = new Repositary<Applicationuser>(context);
        }


        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
