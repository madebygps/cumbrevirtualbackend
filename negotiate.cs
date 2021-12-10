using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class negotiate
    {
        private readonly ILogger _logger;

        public negotiate(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<negotiate>();
        }

        [Function("negotiate")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = "coins")] string connectionInfo)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            response.WriteString(connectionInfo);

            return response;
        }
    }
}
