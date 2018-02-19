# Single Sign-On Web Chat Bot protected by the Azure AD 

Once your user is authenticated by the hosting web app, it will also be authenticated for your bot implementation. 

The solution consists of two parts:
The hosting web app fetching the Azure AD id token, and the bot implementation receiving this token and validating it.
The id token is passed with each message to the bot implementation through the channel data (aka back-channel) with the Direct Line API. The bot implementation validates the token and gives the authenticated user access to the bot implementation logic. 
The solution is a non-optimized and non-productive prototype. 

The hosting web app is based on ASP.NET Core 2.0. Use the [Secret Manager](https://blogs.msdn.microsoft.com/mihansen/2017/09/10/managing-secrets-in-net-core-2-0-apps/) for your DirectLineSecret setting.

The config file for the bot implementation needs the following secrets:

 key="MicrosoftAppId" value="YOURAPPID"

 key="MicrosoftAppPassword" value="YOURAPPPASSWORD"
 
 key="AzureWebJobsStorage" value="YOURSTORAGECONNSTRING"
 
 key="AzureWebJobsDashboard" value="YOURSTORAGECONNSTRING"
 
 key="HostingWebAppId" value="YOURHOSTINGWEBAPPID"    

In order to be able to run this solution you need: 

1. An Azure AD tenant with at least one user being able to authenticate in Azure AD

2. You need to register your hosting web app with your Azure AD tenant

3. You need to enable your bot for the Direct Line channel and use the secret to access the channel. You can optimize it and use the Direct Line token instead.

You can debug the solution with ngrok as described [here](debugdirectline.md)
