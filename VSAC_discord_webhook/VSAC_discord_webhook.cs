using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VSAC_discord_webhook
{
    public static class VSAC_discord_webhook
    {
        // set Here the payload url of the discord webhook
        static string payload_url = "";
        [FunctionName("VSAC_discord_webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string appDisplayName = data?.app_display_name;
            string releaseID = data?.release_id;
            string platform = data?.platform;
            bool mandatory = data?.mandatory_update ?? false;
            string installLink = data?.install_link;
            string iconLink = data?.icon_link;

            string mandatoryStr = mandatory ? "mandatory " : " ";

            string discordPayload = $@"
            {{
                ""avatar_url"": ""{iconLink}"",
                ""content"": ""{"new " + mandatoryStr + "version of [" + appDisplayName + "]("+installLink+") available for " + platform + "("+releaseID+")"}""
            }}
            ";


            if (!sendToDiscord(payload_url,discordPayload))
            {
                return new ObjectResult("failed discord payload");
            }

            string responseMessage = "OK";
            return new OkObjectResult(responseMessage);
        }

        private static bool sendToDiscord(string webhookURL, string payload){
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            var request = WebRequest.Create(webhookURL);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;
            var reqStream = request.GetRequestStream();
            reqStream.Write(bytes, 0, bytes.Length);
            var response = request.GetResponse();
            var code = ((HttpWebResponse)response).StatusCode;
            return code ==HttpStatusCode.NoContent;
        }
    }
}
