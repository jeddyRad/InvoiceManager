using InvoiceManager.Components;
using Microsoft.EntityFrameworkCore;
using InvoiceManager.Data;
using InvoiceManager.Services;
using InvoiceManager.Services.Interfaces;
using System.Globalization;
using System.Text;

namespace InvoiceManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Forcer l'encodage UTF-8 pour toute l'application
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Configuration de la culture pour l'Ariary Malgache
            var cultureInfo = new CultureInfo("mg-MG"); // Culture malgache
            cultureInfo.NumberFormat.CurrencySymbol = "Ar"; // Symbole Ariary
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 0; // L'Ariary n'a pas de centimes
            cultureInfo.NumberFormat.CurrencyPositivePattern = 3; // n Ar (nombre suivi du symbole)
            cultureInfo.NumberFormat.CurrencyNegativePattern = 8; // -n Ar
            cultureInfo.NumberFormat.CurrencyGroupSeparator = "."; // Séparateur de milliers : POINT
            
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var builder = WebApplication.CreateBuilder(args);

            // Configuration de l'encodage pour Kestrel
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.AddServerHeader = false;
            });

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                   options.UseSqlite(
                       builder.Configuration.GetConnectionString("DefaultConnection")
                   ));
            
            // Configuration du logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            
            // Enregistrement des services métier
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IFactureService, FactureService>();
            builder.Services.AddScoped<IAppStateService, AppStateService>();
            
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

            // Configuration des headers pour l'encodage UTF-8
            app.Use(async (context, next) =>
            {
                context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
                await next();
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}

