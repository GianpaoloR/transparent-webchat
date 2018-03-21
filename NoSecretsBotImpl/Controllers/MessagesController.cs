using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            // check if activity is of type message
            if (activity.Type == ActivityTypes.Message)
            {
                string channelDataJson = activity.ChannelData.ToString();
                dynamic channelData = JObject.Parse(channelDataJson);
                string token = channelData.idToken;

                //await context.PostAsync($"Hello {context.Activity.From.Name}");

                if (ValidateJwt(token))
                {
                    // await Conversation.SendAsync(activity, () => new EchoDialog());
                    await Conversation.SendAsync(activity, () => new QnADialog());
                }
                else
                {
                    await Conversation.SendAsync(activity, () => new NonAuthenticatedDialog());
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private static bool ValidateJwt(string token)
        {
            if(String.IsNullOrEmpty(token))
            {
                return false;
            }
            string hostingWebAppId =
                    ConfigurationManager.AppSettings["HostingWebAppId"];
            string azureADTenant =
                    ConfigurationManager.AppSettings["AzureADTenant"];
          
            string stsDiscoveryEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

            IdentityModel.Protocols.ConfigurationManager<IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration> configManager = new IdentityModel.Protocols.ConfigurationManager<IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfigurationRetriever());

            IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration config = configManager.GetConfigurationAsync().Result;


            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                // verify that this your application 
                ValidateAudience = true,
                // this is your hosting web app id = aud
                // The application that receives the token must verify that the audience value is correct and reject any tokens intended for a different audience.
                ValidAudience = hostingWebAppId,
                ValidateIssuer = true,
                // this is your tenant id which issued the token and where the application is registered = iss
                ValidateIssuerSigningKey = true,
                ValidIssuer = "https://sts.windows.net/" + azureADTenant + "/",
                IssuerSigningKeys = config.SigningKeys,
                //ValidateLifetime = false
                ValidateLifetime = true
            };

            JwtSecurityTokenHandler tokendHandler = new JwtSecurityTokenHandler();

            SecurityToken sToken;
            try
            {
                ClaimsPrincipal claimsPrincipal = tokendHandler.ValidateToken(token, validationParameters, out sToken);
                JwtSecurityToken jwtToken = sToken as JwtSecurityToken;
            } catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}