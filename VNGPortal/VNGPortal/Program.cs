using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using VNGPortal.Services;

namespace VNGPortal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var isLinuxPlatform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            var FileStorage = isLinuxPlatform ? "FileStorageLinux" : "FileStorage";

            //
            // Create folders for settings, models, and logs if they don't exist
            // 
            var settingsDir = builder.Configuration[$"{FileStorage}:SettingsDir"];
            if (string.IsNullOrEmpty(settingsDir))
            {
                throw new InvalidOperationException("Settings path is not configured.");
            }
            Directory.CreateDirectory(settingsDir);

            var modelsDir = builder.Configuration[$"{FileStorage}:ModelsDir"];
            if (string.IsNullOrEmpty(modelsDir))
            {
                throw new InvalidOperationException("Models path is not configured.");
            }
            Directory.CreateDirectory(modelsDir);

            var workflowsDir = builder.Configuration[$"{FileStorage}:WorkflowsDir"];
            if (string.IsNullOrEmpty(workflowsDir))
            {
                throw new InvalidOperationException("Workflows path is not configured.");
            }
            Directory.CreateDirectory(workflowsDir);

            // Copy seed workflow XML files from the SeedData/Workflows directory to the configured workflows directory if they don't already exist
            var seedSourceDir = Path.Combine(AppContext.BaseDirectory, "SeedData", "Workflows");
            if (Directory.Exists(seedSourceDir))
            {
                foreach (var sourceFile in Directory.EnumerateFiles(seedSourceDir, "*.xml"))
                {
                    var destFile = Path.Combine(workflowsDir, Path.GetFileName(sourceFile));
                    File.Copy(sourceFile, destFile, true);
                }
            }

            var logsDir = builder.Configuration[$"{FileStorage}:LogsDir"];
            if (string.IsNullOrEmpty(logsDir))
            {
                throw new InvalidOperationException("Logs path is not configured.");
            }
            Directory.CreateDirectory(logsDir);

            // Serilog
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .WriteTo.File(Path.Combine(logsDir, "log.txt"), rollingInterval: RollingInterval.Day)
            );

            // SignalR Services
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.HandshakeTimeout = TimeSpan.FromSeconds(30);
                options.MaximumReceiveMessageSize = null; // Unlimited message size
            });
            builder.Services.AddSingleton<IConnectionStateService, ConnectionStateService>();
            builder.Services.AddSingleton<ISignalRStatusService, SignalRStatusService>();

            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 2_147_483_648; // 2GB
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 2_147_483_648; // 2GB
            });

            // For form uploads specifically
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 2_147_483_648; // 2GB
                options.ValueLengthLimit = 500 * 1024 * 1024;     // 1GB
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();

            var app = builder.Build();

            // SignalR Hubs
            app.MapHub<StatusHub>("/statusHub");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            // Custom middleware to handle API errors with JSON responses
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    // Check if this is an API request (has a handler query parameter)
                    if (context.Request.Query.ContainsKey("handler"))
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
                    }
                    else
                    {
                        throw; // Let the default error handler deal with it
                    }
                }
            });

            // Don't redirect to HTTPS on Linux (Docker) - requires certificates to be set up, which is not common in development environments.
            // In production, HTTPS should be handled by a reverse proxy like Nginx or Traefik.
            if (!isLinuxPlatform)
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
