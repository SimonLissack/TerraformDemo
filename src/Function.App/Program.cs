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
            TokenCredential tokenCredential;

            if(h.HostingEnvironment.IsDevelopment()){
                // Default was throwing an exception locally, so use CLI creds
                tokenCredential = new AzureCliCredential();
            }else{
                tokenCredential = new DefaultAzureCredential();
            }

            var queueClient = c.AddQueueServiceClient(h.Configuration.GetSection("Queue")).WithCredential(tokenCredential);
            var tableClient = c.AddTableServiceClient(h.Configuration.GetSection("Table")).WithCredential(tokenCredential);
        });
    })
    .Build();

host.Run();
