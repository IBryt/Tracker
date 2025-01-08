using Core;
using Core.Infrastructure;
using Core.Interfaces;
using Core.Interfaces.Infrastructure;
using Core.Interfaces.Monitor;
using Core.Interfaces.Observer;
using Core.Monitor;
using Core.Observer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var application = serviceProvider.GetRequiredService<ITracker>();
        application.Run(args);
        Console.ReadKey();
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
        services.AddSingleton<IWindowMessageProcessor, WindowMessageProcessor>();
        services.AddSingleton<IThreadSyncEvent, ThreadSyncEvent>();
        
        services.AddSingleton<IWindowInfoQueue, WindowInfoQueue>();
        services.AddSingleton<IWindowInfoManager, WindowInfoManager>();

        services.AddSingleton<IWindowObserver, WindowObserver>();
        services.AddSingleton<IWindowObserverCallback, WindowObserverCallback>();

        services.AddSingleton<IWindowMonitorCallback, WindowMonitorCallback>();
        services.AddSingleton<IWindowMonitorEventHandler, WindowMonitorEventHandler>();
        services.AddSingleton<IWindowMonitor, WindowMonitor>();
        
    }
}
