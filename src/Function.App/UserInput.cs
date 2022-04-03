using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace lissack.io.function.app;

public class UserInput
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly QueueServiceClient _queueClient;

    public UserInput(ILoggerFactory loggerFactory, IConfiguration configuration, QueueServiceClient queueClient)
    {
        _logger = loggerFactory.CreateLogger<UserInput>();
        _configuration = configuration;
        _queueClient = queueClient;
    }

    [Function("PostUserInput")]
    public async Task<HttpResponseData> Post([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var requestMessage = await JsonSerializer.DeserializeAsync<RequestMessage>(req.Body);

        _logger.LogInformation($"Received message from {requestMessage.Name}");

        var queue = _queueClient.GetQueueClient(_configuration.QueueName());

        await queue.SendMessageAsync(JsonSerializer.Serialize(requestMessage));

        _logger.LogInformation("C# HTTP trigger function processed a request.");

        return req.CreateResponse(HttpStatusCode.OK);
    }

    [Function("GetUserInput")]
    public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, "Get")] HttpRequestData req)
    {
        var queue = _queueClient.GetQueueClient(_configuration.QueueName());

        var topMessage = (await queue.ReceiveMessageAsync()).Value;

        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.Headers.Add("Content-Type", "application/json");

        if(topMessage != null){
            response.Body = topMessage.Body?.ToStream() ?? Stream.Null;

            await queue.DeleteMessageAsync(topMessage.MessageId, topMessage.PopReceipt);
        }

        return response;
    }
}

class RequestMessage
{
    public string Name { get; set; }
    public string Message { get; set; }
}
