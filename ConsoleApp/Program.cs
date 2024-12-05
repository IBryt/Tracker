using Core;
using Core.ExternalLibs;
using Core.Interfaces;
using Core.Interfaces.ExternalLibs;
using Core.Interfaces.Monitors;
using Core.Monitors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var application = serviceProvider.GetRequiredService<ITracker>();
        await application.RunAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(configure =>
        {
            configure
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole();
        });

        services.AddTransient<ITracker, Tracker>();
        services.AddSingleton<IAppMonitor,AppMonitor>();
        services.AddSingleton<INotepadMonitor, NotepadMonitor>();
        services.AddSingleton<ITelegramMonitor, TelegramMonitor>();
        services.AddSingleton<IWinUser32Api, WinUser32Api>();
    }
}
