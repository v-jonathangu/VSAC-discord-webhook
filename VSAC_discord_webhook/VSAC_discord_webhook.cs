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
        static string payload_url = "";
        [FunctionName("VSAC_discord_webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string appDisplayName = data?.app_display_name;
            string releaseID = data?.release_id;
            string platform = data?.platform;
            bool mandatory = data?.mandatory_update;
            string installLink = data?.install_link;
            string iconLink = data?.icon_link;

            string mandatoryStr = mandatory ? "mandatory " : " ";

            string discordPayload = $@"
            {{
                ""avatar_url"": ""{iconLink}"",
                ""content"": ""{"new " + mandatoryStr + "version of [" + appDisplayName + "]("+installLink+") available for " + platform + "("+releaseID+")"}""
            }}
            ";

            string responseMessage = discordPayload;

            if (!sendToDiscord(payload_url,discordPayload))
            {
                return new ObjectResult("failed discord payload");
            }

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
            return code ==HttpStatusCode.OK;
        }
    }
}

/*
 {
  "username": "Webhook",
  "avatar_url": "https://i.imgur.com/4M34hi2.png",
  "content": "Text message. Up to 2000 characters.",
  "embeds": [
    {
      "author": {
        "name": "Birdie♫",
        "url": "https://www.reddit.com/r/cats/",
        "icon_url": "https://i.imgur.com/R66g1Pe.jpg"
      },
      "title": "Title",
      "url": "https://google.com/",
      "description": "Text message. You can use Markdown here. *Italic* **bold** __underline__ ~~strikeout~~ [hyperlink](https://google.com) `code`",
      "color": 15258703,
      "fields": [
        {
          "name": "Text",
          "value": "More text",
          "inline": true
        },
        {
          "name": "Even more text",
          "value": "Yup",
          "inline": true
        },
        {
          "name": "Use `\"inline\": true` parameter, if you want to display fields in the same line.",
          "value": "okay..."
        },
        {
          "name": "Thanks!",
          "value": "You're welcome :wink:"
        }
      ],
      "thumbnail": {
        "url": "https://upload.wikimedia.org/wikipedia/commons/3/38/4-Nature-Wallpapers-2014-1_ukaavUI.jpg"
      },
      "image": {
        "url": "https://upload.wikimedia.org/wikipedia/commons/5/5a/A_picture_from_China_every_day_108.jpg"
      },
      "footer": {
        "text": "Woah! So cool! :smirk:",
        "icon_url": "https://i.imgur.com/fKL31aD.jpg"
      }
    }
  ]
} 
  
 * 
 
 {
  "app_name":"{app-name}",
  "app_display_name":"{app-display-name}",
  "release_id":"123",
  "platform":"Android",
  "uploaded_at":"2018-07-17T20:46:14Z",
  "fingerprint":"0abed1269e4ae3bf524e4cc7165f4f34",
  "release_notes":"",
  "version":"74",
  "short_version":"1.7.0",
  "min_os":"4.0.3",
  "mandatory_update":true,
  "size":2634279,
  "provisioning_profile_name":null,
  "provisioning_profile_type":null,
  "bundle_identifier":"com.microsoft.appcenter.test",
  "install_link":"https://install.appcenter.ms/orgs/{org-name}/apps/{app-name}/releases/123?source=email",
  "icon_link":"https://appcenter-filemanagement-distrib4ede6f06e.azureedge.net/f7794e4c-42f1-4e7c-8013-07ed2e1b733d/ic_launcher.png?sv=2020-02-18&sr=c&sig=gs4JfcWjpKeYH%2F%2Fg0jEtSKKbeRkug9q%2FldslmzzeOg0%3D&se=2020-02-26T08%3A57%3A58Z&sp=r",
  "distribution_group_id":"1a5a0605-4b9c-4de2-9a35-t569456df0cc",
  "installable":true,
  "sent_at":"2019-05-16T23:20:08.7799314Z",
  "app_id":"f37c6194-9ac9-4504-be61-55re334r5649"
}
 */
