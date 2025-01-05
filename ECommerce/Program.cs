using ECommerce.Data.DatabaseContext;
using ECommerce.Repositary.Implmentation;
using ECommerce.Repositary.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using ECommerce.Utility;
using Stripe;
using ECommerce.Data.DBIntilizer;
using ECommerce.Data.DBInitializer;

namespace ECommerce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region Configuration Service
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddDbContext<ECommerceContext>(options =>
            options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, // Number of retry attempts
                maxRetryDelay: TimeSpan.FromSeconds(30), // Maximum delay between retries
                errorNumbersToAdd: null // Additional SQL error numbers to retry on
            )
            )       
            );

            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

            builder.Services.AddIdentity<IdentityUser,IdentityRole>()
                .AddEntityFrameworkStores<ECommerceContext>().AddDefaultTokenProviders();
            builder.Services.ConfigureApplicationCookie(options => {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            builder.Services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "1512936369395470";
                options.AppSecret = "9c24bb2326c6abda13a2d33d8c5d649a";
                options.CallbackPath = "/signin-facebook";

            });

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(100);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IDBInitializer, DBInitializer>();
            #endregion 

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            #region Configure MiddleWares
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
            app.UseRouting();
            app.UseAuthentication();
            
            app.UseAuthorization();
            app.UseSession();
            SeedData();
            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "/{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.Run(); 

            void SeedData()
            {
                using(var seed = app.Services.CreateScope())
                {
                    var dbinitilzer = seed.ServiceProvider.GetRequiredService<IDBInitializer>();
                    dbinitilzer.Initializer();
                }
            }
            #endregion 
        }
    }
}
