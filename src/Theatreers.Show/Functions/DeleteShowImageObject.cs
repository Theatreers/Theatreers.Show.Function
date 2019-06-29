
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Theatreers.Show.Models;
using Theatreers.Show.Utils;

namespace Theatreers.Show.Functions
{
  public static class DeleteShowImageObject
  {
    [FunctionName("DeleteShowImageObject")]

    public static async Task<IActionResult> DeleteShowImageObjectAsync(
      [HttpTrigger(
        AuthorizationLevel.Anonymous,
        methods: "DELETE",
        Route = "show/{id}/image/{imageid}"
      )] HttpRequestMessage req,
      ILogger log,
      [CosmosDB(
        databaseName: "theatreers",
        collectionName: "shows",
        ConnectionStringSetting = "cosmosConnectionString"
      )]  IDocumentClient documentClient,
      ClaimsPrincipal identity
    )
    {
      if (identity != null && identity.Identity.IsAuthenticated)
      {
        Uri showCollectionUri = UriFactory.CreateDocumentCollectionUri("theatreers", "shows");
        string correlationId = Guid.NewGuid().ToString();
        CosmosBaseObject<Models.ImageObject> submitObject = null;
        String requestId = req.RequestUri.AbsolutePath.Replace($"/api/show/", "").Replace($"/image/", "::");
        String[] ids = requestId.Split("::");
        Models.ImageObject message = new Models.ImageObject();

        var docExists = documentClient.CreateDocumentQuery<CosmosBaseObject<ShowObject>>(showCollectionUri, new FeedOptions { PartitionKey = new PartitionKey(ids[0]) })
                           .Where(doc => doc.Id == ids[1] && doc.Doctype == "image")
                           .AsEnumerable()
                           .Any();

        if (docExists)
        {
          try
          {

            submitObject = new CosmosBaseObject<Models.ImageObject>()
            {
              Id = ids[1],
              Doctype = "image",
              ShowId = ids[0],
              Ttl = 10
            };

            await documentClient.UpsertDocumentAsync(showCollectionUri, submitObject);
            log.LogInformation($"[Request Correlation ID: {correlationId}] :: Image Deletion Success :: Object ID: {submitObject.Id} ");
          }
          catch (Exception ex)
          {
            log.LogInformation($"[Request Correlation ID: {correlationId}] :: Image Deletion Fail ::  :: Object ID: {submitObject.Id} - {ex.Message}");
            return new BadRequestResult();
          }
          return new OkResult();
        }
        else
        {
          return new NotFoundResult();
        }
      }
      else
      {
        return new UnauthorizedResult();
      }
    }
  }
}