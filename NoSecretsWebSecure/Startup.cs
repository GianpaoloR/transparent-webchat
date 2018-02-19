using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.Caching.Distributed;
using NoSecretsWebSecure.Extensions;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace NoSecretsWebSecure
{
    public class AdalDistributedTokenCache : TokenCache
    {
        private readonly IDistributedCache _cache;
        private readonly string _userId;

        public AdalDistributedTokenCache(IDistributedCache cache, string userId)
        {
            _cache = cache;
            _userId = userId;
            BeforeAccess = BeforeAccessNotification;
            AfterAccess = AfterAccessNotification;
        }

        private string GetCacheKey()
        {
            return $"{_userId}_TokenCache";
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Deserialize(_cache.Get(GetCacheKey()));
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                _cache.Set(GetCacheKey(), Serialize(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });
                HasStateChanged = false;
            }
        }
    }

    public class Startup
    {
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //var _testSecret = Configuration["MySecret"];
            services.AddOptions();
            services.Configure<Secrets>(Configuration);
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAd(options => Configuration.Bind("AzureAd", options))
            .AddCookie();
            services.AddMvc(); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.UseSecrets();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }        
    }
    
    public static class UseMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecrets(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecretsMiddleware>();
        }
    }

    public class SecretsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Secrets settings;

        public SecretsMiddleware(
            RequestDelegate next,
            IOptions<Secrets> options)
        {
            this.next = next;
            this.settings = options.Value;
        }

        
         public async Task Invoke(HttpContext context)
        {
            var jsonSettings = JsonConvert.SerializeObject(this.settings);
            await context.Response.WriteAsync(jsonSettings);
        }
        
    }
}
