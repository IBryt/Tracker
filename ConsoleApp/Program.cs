using Core;
using Core.Interfaces;
using Core.Interfaces.Native;
using Core.Native;
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
        services.AddSingleton<IEventProcessor, EventProcessor>();
        services.AddSingleton<IMessageLoop, MessageLoop>();
        services.AddSingleton<IWindowActionQueue, WindowActionQueue>();
        services.AddSingleton<IWindowHandler, WindowHandler>();
        services.AddSingleton<IWindowManager, WindowManager>();
    }
}
