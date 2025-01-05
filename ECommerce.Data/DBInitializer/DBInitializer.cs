using ECommerce.Data.DatabaseContext;
using ECommerce.Data.DBIntilizer;
using ECommerce.Data.Entity;
using ECommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Data.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly ECommerceContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DBInitializer(ECommerceContext context
            ,UserManager<IdentityUser> userManager
            ,RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void Initializer()
        {
            try
            {
                if(_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
            }

            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();


                _userManager.CreateAsync(new Applicationuser
                {
                    UserName = "YassinMohamed@gmail.com",
                    Email = "ym9807770@gmail.com",
                    Name = "Yassin Moahmed",
                    PhoneNumber = "01060459123",
                    StreetAddress = "15 st",
                    State = "IL",
                    PostalCode = "23422",
                    City = "Cairo"
                },"E123@yassin").GetAwaiter().GetResult();

                Applicationuser applicationuser = _context.Applicationusers.FirstOrDefault(x => x.Email == "ym9807770@gmail.com");
                _userManager.AddToRoleAsync(applicationuser, SD.Role_Admin).GetAwaiter().GetResult();

            }
                return;

        }
    }
}
