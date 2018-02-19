using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using NoSecretsWebSecure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace NoSecretsWebSecure.Pages
{
    public class IndexModel : PageModel
    {
        public string IdToken { get; private set; } = string.Empty;
        public string UserName { get; private set; } = string.Empty;
        public string DLSecret { get; private set; } = string.Empty;

        private readonly string _dlSecret = string.Empty;


        public IndexModel(IOptions<Secrets> secrets)
        {
            _dlSecret = secrets.Value.DirectLineSecret;
        }

        public void OnGet()
        {
            string owner = (User.FindFirst(ClaimTypes.NameIdentifier))?.Value;
            if (owner != null)
            {
                // in case you need it 
                //string accessToken = HttpContext.GetTokenAsync("access_token").Result;               
                UserName = User.FindFirstValue("name");
                IdToken = HttpContext.GetTokenAsync("id_token").Result;
                DLSecret = _dlSecret;
            }
            else
            // just to make to the bot to propmt "We are sorry, but you are not authenticated. Please sign in" 
            {
                UserName = "anonymous user";
                IdToken = "fake token";
                DLSecret = _dlSecret;
            }
        }
    }
}
