using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Threading;

namespace Theatreers.Show
{
    public static class CreationOrchestrator
    {        
        [FunctionName("CreationOrchestrator")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, methods: "post", Route = "show")] HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClientBase starter,
            ILogger log)
        {
           // if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity.IsAuthenticated)
          //  {
                //Initialise the message for transport
                //Generate correllation ID and initial request timestamp
                string CorrelationId = Guid.NewGuid().ToString();
                string showId = Guid.NewGuid().ToString();
                DecoratedShowObject showObjectInput = JsonConvert.DeserializeObject<DecoratedShowObject>(await req.Content.ReadAsStringAsync());
                MessageHeaders messageHeaders = new MessageHeaders();
                messageHeaders.RequestCorrelationId = CorrelationId;
                messageHeaders.RequestCreatedAt = DateTime.Now.ToString();
                showObjectInput.MessageProperties = messageHeaders;
                showObjectInput.showId = showId;
                string eventData = JsonConvert.SerializeObject(showObjectInput);

                //Call the downstream "Activity" functions
                string submitShowInstanceId = await starter.StartNewAsync("CreateShowObject", eventData);
                string submitNewsInstanceId = await starter.StartNewAsync("CreateShowNewsObject", eventData);
                string submitImageInstanceId = await starter.StartNewAsync("CreateShowImageObject", eventData);

                log.LogInformation($"[Request Correlation ID: {messageHeaders.RequestCorrelationId}] :: Begin Orchestration :: SubmitShowAsync instance ID: {submitShowInstanceId}");
                log.LogInformation($"[Request Correlation ID: {messageHeaders.RequestCorrelationId}] :: Begin Orchestration :: SubmitNewsAsync instance ID: {submitNewsInstanceId}");
                log.LogInformation($"[Request Correlation ID: {messageHeaders.RequestCorrelationId}] :: Begin Orchestration :: SubmitImageAsync instance ID: {submitImageInstanceId}");

                return starter.CreateCheckStatusResponse(req, submitShowInstanceId);
           /* } else
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                response.ReasonPhrase = "The user is not logged in";
                return response;
            }*/
        }
    }
}