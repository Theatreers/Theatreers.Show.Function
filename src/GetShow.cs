
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.CognitiveServices.Search;
using System.Collections.Generic;
using Microsoft.Rest;
using System.Threading;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Net.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Theatreers.Show
{
    public static class GetShowOrShows
    {

        public static class GetShow
        {
            [FunctionName("GetShowAsync")]
            public static async Task<IActionResult> GetShowAsync(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "show/{id}")]HttpRequest req,
                [CosmosDB(
                databaseName: "theatreers",
                collectionName: "items",
                ConnectionStringSetting = "cosmosConnectionString",
                Id = "{id}",
                PartitionKey = "{id}")] ShowMessage showMessage,
                ILogger log)
            {
                string CorrelationId = Guid.NewGuid().ToString();
                String requestId = req.HttpContext.Request.Path.ToString().Replace("/api/show/","");

                if (showMessage == null)
                {
                    log.LogInformation($"[Request Correlation ID: {CorrelationId}] :: GetShow API Request failure :: ID {requestId}");
                    return new NotFoundResult();
                }
                else
                {
                    log.LogInformation($"[Request Correlation ID: {CorrelationId}] :: GetShow API Request success :: ID {requestId}");
                    return new OkObjectResult(showMessage);
                }
            }
        }
    }
}