﻿using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Microsoft.AspNetCore.Authentication
{
    public class AzureAdOptions
    {
        public string ClientId { get; set; }
        
        public string ClientSecret { get; set; }
        
        public string Instance { get; set; }
        
        public string Domain { get; set; }
        
        public string TenantId { get; set; }
        
        public string CallbackPath { get; set; }

        //manually added
        public string TargetApiAppId { get; set; }
        public OpenIdConnectEvents Events { get; internal set; }
    }
}
