using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FikirHavuzu.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<FikirHavuzu.DataAccess.Context.AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

          
            builder.Services.AddScoped(typeof(FikirHavuzu.DataAccess.Repositories.IGenericRepository<>), typeof(FikirHavuzu.DataAccess.Repositories.GenericRepository<>));
            builder.Services.AddScoped<FikirHavuzu.DataAccess.UnitOfWork.IUnitOfWork, FikirHavuzu.DataAccess.UnitOfWork.UnitOfWork>();
            builder.Services.AddScoped<FikirHavuzu.Business.Services.IUserService, FikirHavuzu.Business.Services.UserService>();
            builder.Services.AddScoped<FikirHavuzu.Business.Services.IIdeaService, FikirHavuzu.Business.Services.IdeaService>();
            builder.Services.AddScoped<FikirHavuzu.Business.Services.IEmailService, FikirHavuzu.Business.Services.EmailService>();



            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.Name = "FikirHavuzuAuthCookie";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;

                    options.Cookie.HttpOnly = true; 
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
                    options.Cookie.SameSite = SameSiteMode.Lax; 
                });

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<FikirHavuzu.DataAccess.Context.AppDbContext>();

                context.Database.Migrate();
            }

            app.Run();
        }
    }
}
