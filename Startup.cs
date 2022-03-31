using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Interfaces;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.Serializers;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Common.Web.Routing;
using TowerPortal.Utilities;

namespace TowerPortal
{
    public class Startup : PlatformStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services, Owner.Nathan, warnMS: 30_000, errorMS: 60_000, criticalMS: 90_000, webServerEnabled: true);
            
            services.ConfigureApplicationCookie(options => options.LoginPath = "/portal/account/google-login");
            
            services.AddAuthentication(configureOptions: options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/portal/account/google-login";
                    options.LogoutPath = "/portal/account/google-logout";
                })
                .AddGoogle(options =>
                {
                    options.ClientId = PlatformEnvironment.Require("GOOGLE_CLIENT_ID");
                    options.ClientSecret = PlatformEnvironment.Require("GOOGLE_CLIENT_SECRET");
                    options.SaveTokens = true;
                    
                    // options.CallbackPath = "/account/signin-google";
                });
    
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    name: "Admin",
                    configurePolicy: policy => policy
                        .RequireClaim(ClaimTypes.Name)
                        .AddRequirements(new NameAuthorizationRequirement("Nathan Mac"))
                ); // TODO flush out later
                options.AddPolicy(
                    name: "CompanyStaffOnly",
                    configurePolicy: policy => policy
                        .RequireClaim(ClaimTypes.Email)
                        .AddRequirements(new DomainRequirement("rumbleentertainment.com"))
                );
            });
    
            services.AddControllersWithViews();
        }
    
        protected override void ConfigureRoutes(IEndpointRouteBuilder builder)
        {
            // builder.MapControllerRoute(
            //     name: "default",
            //     pattern: "{controller=Home}/{action=Index}/{id?}"
            // );
        }
    }
}