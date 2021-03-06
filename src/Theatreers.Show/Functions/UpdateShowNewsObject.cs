using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Theatreers.Core.Models;
using Theatreers.Show.Abstractions;
using Theatreers.Show.Models;
using Theatreers.Show.Utils;

namespace Theatreers.Show.Functions
{
    public class UpdateShowNewsObject
    {

        private readonly IShowDomain _showDomain;

        public UpdateShowNewsObject(IShowDomain showDomain)
        {
            _showDomain = showDomain;
        }

        [FunctionAuthorize]
        [FunctionName("UpdateShowNewsObject")]

        public async Task<IActionResult> UpdateShowNewsObjectAsync(
          [HttpTrigger(
        AuthorizationLevel.Anonymous,
        methods: "PUT",
        Route = "show/{showId}/news/{newsId}"
      )] HttpRequestMessage req,
          ILogger log,
          ClaimsPrincipal identity,
          string showId,
          string newsId
        )
        {

            string authorizationStatus = req.Headers.GetValues("AuthorizationStatus").FirstOrDefault();
            if (Convert.ToInt32(authorizationStatus).Equals((int)HttpStatusCode.Accepted))
            {

                // Initialise a message object, based upon the information passed into the Microservice
                MessageObject<NewsObject> message = new MessageObject<NewsObject>()
                {
                    Headers = new MessageHeaders()
                    {
                        RequestCorrelationId = Guid.NewGuid().ToString(),
                        RequestCreatedAt = DateTime.Now
                    },
                    Body = JsonConvert.DeserializeObject<NewsObject>(await req.Content.ReadAsStringAsync())
                };
                message.Body.Partition = showId;
                message.Body.Doctype = DocTypes.News;
                message.Body.Id = newsId;

                // Try the UpdateObject method. If successful, return OK Result. Otherwise, return badrequestresult with an unexpected error.
                // If an exception is generated, throw a BadRequestErrorMessage with the error message.
                try
                {
                    if (await _showDomain.UpdateNewsObject(message))
                    {
                        log.LogInformation($"[Request Correlation ID: {message.Headers.RequestCorrelationId}] :: News Update Success");
                        return new OkResult();
                    }
                    else
                    {
                        log.LogInformation($"[Request Correlation ID: {message.Headers.RequestCorrelationId}] :: News Update Fail");
                        return new BadRequestErrorMessageResult("An unexpected error occured");
                    }
                }
                catch (Exception ex)
                {
                    log.LogInformation($"[Request Correlation ID: {message.Headers.RequestCorrelationId}] :: News Update Fail :: {ex.Message}");
                    return new BadRequestErrorMessageResult(ex.Message);
                }
            }
            else
            {
                return new UnauthorizedResult();
            }
        }
    }
}
