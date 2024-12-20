using CrashRenderServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HttpClientFactoryDemo
{
    class Program
    {
        static void Main(string[] args)
        {            
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var settings = new AppSettings();
            configuration.GetSection("AppSettings").Bind(settings);

            if (settings.NumberOfThread.HasValue && !string.IsNullOrWhiteSpace(settings.TargetRenderServerDomainUri))
            {
                StartWork(serviceProvider, settings.NumberOfThread.Value, settings.TargetRenderServerDomainUri);
            }
            else
            {
                Console.WriteLine($"Settings have issue/s, target: {settings.TargetRenderServerDomainUri}\tnumber of thread: {settings.NumberOfThread}");
            }

            Console.WriteLine("Hi enter to exit program");
            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Add configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) 
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)                 
                .Build(); 
            services.AddSingleton<IConfiguration>(configuration);
            services.AddHttpClient();
        }

        private static void StartWork(ServiceProvider serviceProvider, int threadNumber, string targetDomainUrl)
        {
            for (var i = 0; i < threadNumber; i++)
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        var testUrl = $"{targetDomainUrl}/api/project/t/1evOX30uKcR2f86ypwQt9dxJewZGdNLLhpX4A7FQTw782VrBLboBpFn1,pVPaDEcm6PkeygjRFQUUQfuTl,a1oST_zvaHjk6tV8O,IJIM154-/hires/19.jpg?checkSum=1";
                        Console.WriteLine(testUrl);
                        var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
                        var response = await httpClient.GetAsync(testUrl);
                        _ = await response.Content.ReadAsByteArrayAsync();
                    }
                });
            }
        }
    }
}
