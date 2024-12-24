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
CancellationTokenSource source = new CancellationTokenSource();

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

    if (settings.NumberOfThread.HasValue && !string.IsNullOrWhiteSpace(settings.TargetRenderServerDomainUri))
    {
        StartWork(serviceProvider, settings.NumberOfThread.Value, settings.TargetRenderServerDomainUri, testUrls, source.Token);
    }
    else
    {
        Console.WriteLine($"Settings have issue/s, target: {settings.TargetRenderServerDomainUri}\tnumber of thread: {settings.NumberOfThread}");
    }
}
catch (Exception e)
{
    Console.WriteLine($"An error occurred: {e.Message}");
}

Console.WriteLine("Hi enter to exit program");
Console.ReadLine();
source.Cancel();
source.Dispose();

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

void StartWork(ServiceProvider serviceProvider, int threadNumber, string targetDomainUrl, IEnumerable<string> testUrls, CancellationToken token)
{
    for (var i = 0; i < threadNumber; i++)
    {
        Task.Run(async () =>
        {
            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
            while (true)
            {
                foreach(var url in testUrls)
                {
                    var testUrl = $"{targetDomainUrl}{url}";
                    Console.WriteLine(testUrl);
                    try
                    {
                        var response = await httpClient.GetAsync(testUrl, token);
                        _ = await response.Content.ReadAsByteArrayAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Run into error: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                    }
                    Thread.Sleep(500);
                }
            }
        });
    }
}
