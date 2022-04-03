using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureHostConfiguration(c =>
    {
        c.AddJsonFile("local.settings.json", true, true);
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((h, s) =>
    {
        s.AddAzureClients(c =>
        {
            // Default was throwing an exception locally, so use CLI creds
            TokenCredential tokenCredential = h.HostingEnvironment.IsDevelopment()
                ? new AzureCliCredential()
                : new DefaultAzureCredential();

            var queueClient = c.AddQueueServiceClient(h.Configuration.GetSection("Queue")).WithCredential(tokenCredential);
        });
    })
    .Build();

host.Run();
