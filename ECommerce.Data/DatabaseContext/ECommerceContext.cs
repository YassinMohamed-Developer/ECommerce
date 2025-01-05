using ECommerce.Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Data.DatabaseContext
{
    public class ECommerceContext : IdentityDbContext<IdentityUser>
    {
        public ECommerceContext(DbContextOptions<ECommerceContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region Seed Company
            modelBuilder.Entity<Company>().HasData(
        new Company
        {
            Id = 1,
            Name = "Tech Solution",
            StreetAddress = "123 Tech St",
            City = "Tech City",
            PostalCode = "12121",
            State = "IL",
            PhoneNumber = "6669990000"
        },
        new Company
        {
            Id = 2,
            Name = "Vivid Books",
            StreetAddress = "999 Vid St",
            City = "Vid City",
            PostalCode = "66666",
            State = "IL",
            PhoneNumber = "7779990000"
        },
        new Company
        {
            Id = 3,
            Name = "Readers Club",
            StreetAddress = "999 Main St",
            City = "Lala land",
            PostalCode = "99999",
            State = "NY",
            PhoneNumber = "1113335555"
        }); 
            #endregion 

            #region Seed Category
            modelBuilder.Entity<Category>().HasData(
           new Category { Id = 1, Name = "Mobile", DisplayOrder = 1 },
           new Category { Id = 2, Name = "Electronics", DisplayOrder = 2 },
           new Category { Id = 3, Name = "Laptops&Accessories", DisplayOrder = 3 },
           new Category { Id = 4, Name = "SmartWatch", DisplayOrder = 4 }
           );
            #endregion



            #region Seed Product
            modelBuilder.Entity<Product>().HasData(
                    new Product
                    {
                        Id = 1,
                        Title = "Galaxy S23 Ultra",
                        Author = "Samsung",
                        Description = "Experience epic performance with the Galaxy S23 Ultra. Features a 200MP camera, long battery life, and sleek design.",
                        ISBN = "MOB123456",
                        ListPrice = 1200,
                        Price = 1100,
                        Price50 = 1050,
                        Price100 = 1000,
                        CategoryId = 1
                    },
                    new Product
                    {
                        Id = 2,
                        Title = "Sony WH-1000XM5",
                        Author = "Sony",
                        Description = "Industry-leading noise-canceling headphones with superior sound quality, 30-hour battery life, and a comfortable design.",
                        ISBN = "ELEC789012",
                        ListPrice = 400,
                        Price = 350,
                        Price50 = 340,
                        Price100 = 330,
                        CategoryId = 2
                    },
                    new Product
                    {
                        Id = 3,
                        Title = "Dell XPS 15",
                        Author = "Dell",
                        Description = "The Dell XPS 15 offers a stunning InfinityEdge display, powerful performance with Intel i9, and sleek aluminum chassis.",
                        ISBN = "LAP345678",
                        ListPrice = 2000,
                        Price = 1900,
                        Price50 = 1850,
                        Price100 = 1800,
                        CategoryId = 3
                    },
                    new Product
                    {
                        Id = 4,
                        Title = "Apple Watch Series 9",
                        Author = "Apple",
                        Description = "Track your health and fitness with the Apple Watch Series 9. Features an always-on display, advanced sensors, and seamless integration with iPhone.",
                        ISBN = "SWATCH901234",
                        ListPrice = 500,
                        Price = 470,
                        Price50 = 450,
                        Price100 = 430,
                        CategoryId = 4
                    }
                ); 
            #endregion

            modelBuilder.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Company>().HasQueryFilter(x => !x.IsDeleted);

            base.OnModelCreating(modelBuilder);

        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Applicationuser> Applicationusers { get; set; }

        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        public DbSet<OrderHeader> OrderHeaders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
