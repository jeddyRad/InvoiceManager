using InvoiceManager.Components;
using Microsoft.EntityFrameworkCore;
using InvoiceManager.Data;
using InvoiceManager.Services;
using InvoiceManager.Services.Interfaces;
using InvoiceManager.Data.Repositories;
using InvoiceManager.Data.Repositories.Interfaces;
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
            var cultureInfo = new CultureInfo("mg-MG");
            cultureInfo.NumberFormat.CurrencySymbol = "Ar";
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 0;
            cultureInfo.NumberFormat.CurrencyPositivePattern = 3;
            cultureInfo.NumberFormat.CurrencyNegativePattern = 8;
            cultureInfo.NumberFormat.CurrencyGroupSeparator = ".";
            
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var builder = WebApplication.CreateBuilder(args);

            // 🔒 Configuration sécurisée du chemin de la base de données
            var connectionString = GetSecureConnectionString(builder);

            // Configuration de l'encodage pour Kestrel
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.AddServerHeader = false;
            });

            // 🔒 Configuration DbContext avec options de sécurité
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(connectionString, sqliteOptions =>
                {
                    sqliteOptions.CommandTimeout(30);
                    sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });
            
            // Configuration du logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            
            if (builder.Environment.IsDevelopment())
            {
                builder.Logging.SetMinimumLevel(LogLevel.Information);
            }
            else
            {
                builder.Logging.SetMinimumLevel(LogLevel.Warning);
            }
            
            builder.Logging.AddFilter("Microsoft.AspNetCore.Components", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            
            // ✅ Enregistrement du pattern Repository
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IFactureRepository, FactureRepository>();
            
            // Enregistrement des services métier
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IFactureService, FactureService>();
            builder.Services.AddScoped<IAppStateService, AppStateService>();
            
            // 🔒 Configuration de sécurité Blazor
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // 🔒 Configuration des circuits Blazor Server
            builder.Services.AddServerSideBlazor(options =>
            {
                options.DetailedErrors = builder.Environment.IsDevelopment();
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
                options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
                options.MaxBufferedUnacknowledgedRenderBatches = 10;
            });

            var app = builder.Build();

            // 🔒 Initialisation sécurisée de la base de données
            InitializeDatabase(app, builder.Environment);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // 🔒 Headers de sécurité
            app.Use(async (context, next) =>
            {
                context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                
                if (!app.Environment.IsDevelopment())
                {
                    context.Response.Headers["Content-Security-Policy"] = 
                        "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline';";
                }
                
                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

        /// <summary>
        /// 🔒 Détermine le chemin sécurisé de la base de données selon l'environnement
        /// </summary>
        private static string GetSecureConnectionString(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La chaîne de connexion 'DefaultConnection' est manquante.");
            }

            // En production, utiliser un chemin absolu sécurisé
            if (!builder.Environment.IsDevelopment())
            {
                return connectionString;
            }

            // En développement, utiliser un chemin relatif au projet
            var projectRoot = Directory.GetCurrentDirectory();
            var dbPath = Path.Combine(projectRoot, "App_Data", "invoice_manager.db");
            
            // Créer le dossier si nécessaire
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            return $"Data Source={dbPath}";
        }

        /// <summary>
        /// 🔒 Initialise la base de données avec gestion d'erreurs et migrations
        /// </summary>
        private static void InitializeDatabase(WebApplication app, IHostEnvironment environment)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var dbContext = services.GetRequiredService<AppDbContext>();
                var dbPath = dbContext.Database.GetDbConnection().DataSource;
                
                logger.LogInformation("📁 Chemin de la base de données : {DbPath}", dbPath);

                // En production, utiliser les migrations
                if (!environment.IsDevelopment())
                {
                    logger.LogInformation("🔄 Application des migrations...");
                    dbContext.Database.Migrate();
                }
                else
                {
                    // En développement, créer la base si elle n'existe pas
                    dbContext.Database.EnsureCreated();
                }

                logger.LogInformation("✅ Base de données initialisée avec succès");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Erreur critique lors de l'initialisation de la base de données");
                throw;
            }
        }
    }
}

