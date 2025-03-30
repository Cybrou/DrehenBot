using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DrehenBot
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            IHostBuilder builder = Host.CreateDefaultBuilder(args);

            // Logs
            builder.ConfigureLogging((hb, log) =>
            {
                log.ClearProviders();
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(hb.Configuration).CreateLogger();
                log.AddSerilog(dispose: true);
            });

            // Services
            builder.ConfigureServices((hc, services) =>
            {
                services.AddScoped<Bot>();
                services.AddScoped<Config.AppConfig>();
            });

            // Start
            IHost host = builder.Build();
            using (IServiceScope serviceScope = host.Services.CreateScope())
            {
                IServiceProvider serviceProvider = serviceScope.ServiceProvider;
                Bot bot = serviceProvider.GetRequiredService<Bot>();

                return await bot.Start();
            }
        }
    }
}