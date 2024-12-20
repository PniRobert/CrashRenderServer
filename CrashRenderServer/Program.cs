using CrashRenderServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);
var serviceProvider = serviceCollection.BuildServiceProvider();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();

var settings = new AppSettings();
configuration.GetSection("AppSettings").Bind(settings);

var filePath = "TestUrls.txt";
var testUrls = new List<string>();

try
{
    using (StreamReader reader = new StreamReader(filePath))
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            testUrls.Add(line);
        }
    }
}
catch (Exception e)
{
    Console.WriteLine($"An error occurred: {e.Message}");
}

if (settings.NumberOfThread.HasValue && !string.IsNullOrWhiteSpace(settings.TargetRenderServerDomainUri))
{
    StartWork(serviceProvider, settings.NumberOfThread.Value, settings.TargetRenderServerDomainUri, testUrls);
}
else
{
    Console.WriteLine($"Settings have issue/s, target: {settings.TargetRenderServerDomainUri}\tnumber of thread: {settings.NumberOfThread}");
}

Console.WriteLine("Hi enter to exit program");
Console.ReadLine();

void ConfigureServices(IServiceCollection services)
{
    // Add configuration
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();
    services.AddSingleton<IConfiguration>(configuration);
    services.AddHttpClient();
}

void StartWork(ServiceProvider serviceProvider, int threadNumber, string targetDomainUrl, IEnumerable<string> testUrls)
{
    for (var i = 0; i < threadNumber; i++)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                foreach(var url in testUrls)
                {
                    var testUrl = $"{targetDomainUrl}{url}";
                    Console.WriteLine(testUrl);
                    var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
                    var response = await httpClient.GetAsync(testUrl);
                    _ = await response.Content.ReadAsByteArrayAsync();
                }
            }
        });
    }
}
