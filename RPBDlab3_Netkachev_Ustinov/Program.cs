using RPBDlab3_Netkachev_Ustinov.Models;
using RPBDlab3_Netkachev_Ustinov.Controllers;

namespace RPBDlab3_Netkachev_Ustinov
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();
            builder.Services.AddDbContext<DatabaseContext>();
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}"
                );
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ObserverUpdater>("/updateHub");
            });
            app.MapRazorPages();
            app.Run();
        }
    }
}