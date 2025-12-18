using InvoiceManager.Components;
using Microsoft.EntityFrameworkCore;
using InvoiceManager.Data;
using InvoiceManager.Services;
using InvoiceManager.Services.Interfaces;
namespace InvoiceManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                   options.UseSqlite(
                       builder.Configuration.GetConnectionString("DefaultConnection")
                   ));
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Debug);
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IFactureService, FactureService>();
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Ensure database is created on first run
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}

