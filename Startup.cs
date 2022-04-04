using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;
using Rumble.Platform.Common.Attributes;
using Rumble.Platform.Common.Extensions;
using Rumble.Platform.Common.Filters;
using Rumble.Platform.Common.Interfaces;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Utilities.Serializers;
using Rumble.Platform.Common.Web;
using Rumble.Platform.Common.Web.Routing;
using TowerPortal.Utilities;

namespace TowerPortal;

// [BaseRoute("portal")]
public class Startup : PlatformStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        BypassFilter<PlatformPerformanceFilter>();
        
        base.ConfigureServices(services, Owner.Nathan, warnMS: 30_000, errorMS: 60_000, criticalMS: 90_000, webServerEnabled: true);

        string baseRoute = this.HasAttribute(out BaseRoute att)
            ? $"/{att.Route}"
            : "";
        
        services.ConfigureApplicationCookie(options => options.LoginPath = $"{baseRoute}/account/google-login");
        
        services.AddAuthentication(configureOptions: options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = $"{baseRoute}/account/google-login";
                options.LogoutPath = $"{baseRoute}/account/google-logout";
                options.Cookie.SameSite = SameSiteMode.Strict; // Suggestion from SO to resolve Correlation failed Exception
                options.Events.OnSignedIn = (context) =>
                {
                    Log.Dev(Owner.Will, $"{context.Principal.Identity.Name} signed in.");
                    
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogin = (context) =>
                {
                    Log.Dev(Owner.Will, $"Redirect login to '{context.RedirectUri}'.");
                    
                    return Task.CompletedTask;
                };
                options.Events.OnSigningIn = (context) =>
                {
                    Log.Dev(Owner.Will, $"{context?.Principal?.Identity?.Name ?? "(unknown)"} is signing in.");
                    
                    return Task.CompletedTask;
                };
                options.Events.OnSigningOut = (context) =>
                {
                    Log.Dev(Owner.Will, $"User is signing out.");

                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToReturnUrl = (context) =>
                {
                    Log.Dev(Owner.Will, $"Redirecting to {context.RedirectUri}");

                    return Task.CompletedTask;
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = PlatformEnvironment.Require("GOOGLE_CLIENT_ID");
                options.ClientSecret = PlatformEnvironment.Require("GOOGLE_CLIENT_SECRET");
                options.SaveTokens = true;
                
                // options.CallbackPath = $"{baseRoute}/account/google-response";
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
}