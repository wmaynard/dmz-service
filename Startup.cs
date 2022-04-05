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
using Microsoft.Extensions.Options;
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
        
        
        string baseRoute = this.HasAttribute(out BaseRoute att)
            ? $"/{att.Route}"
            : "";
        
        services.ConfigureApplicationCookie(options => options.LoginPath = $"{baseRoute}/account/google-login");

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            options.Secure = CookieSecurePolicy.Always;
            options.OnAppendCookie = (cookieContext) =>
            {
                CookieOptions cookieOptions = cookieContext.CookieOptions;
                HttpContext context = cookieContext.Context;
                
                if (cookieOptions.SameSite == SameSiteMode.None) 
                { 
                    var userAgent = context.Request.Headers["User-Agent"].ToString(); 
                    if ( DisallowsSameSiteNone(userAgent)) 
                    { 
                        cookieOptions.SameSite = SameSiteMode.Unspecified; 
                    } 
                } 
            };
        });
        
        services.AddAuthentication(configureOptions: options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = $"{baseRoute}/account/google-login";
                options.LogoutPath = $"{baseRoute}/account/google-logout";
                options.Cookie.SameSite = SameSiteMode.Lax; // Suggestion from SO to resolve Correlation failed Exception
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
        
        base.ConfigureServices(services, Owner.Nathan, warnMS: 30_000, errorMS: 60_000, criticalMS: 90_000, webServerEnabled: true);

    }
    
    // More debugging on cookie conflicts
    // https://community.auth0.com/t/correlation-failed-unknown-location-error-on-chrome-but-not-in-safari/40013/7
    public bool DisallowsSameSiteNone(string userAgent)
    {
        // Check if a null or empty string has been passed in, since this
        // will cause further interrogation of the useragent to fail.
        if (String.IsNullOrWhiteSpace(userAgent))
            return false;

        // Cover all iOS based browsers here. This includes:
        // - Safari on iOS 12 for iPhone, iPod Touch, iPad
        // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
        // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
        // All of which are broken by SameSite=None, because they use the iOS networking
        // stack.
        if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
            return true;

        // Cover Mac OS X based browsers that use the Mac OS networking stack. 
        // This includes:
        // - Safari on Mac OS X.
        // This does not include:
        // - Chrome on Mac OS X
        // Because they do not use the Mac OS networking stack.
        if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") && userAgent.Contains("Version/") && userAgent.Contains("Safari"))
                return true;

        // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
        // and none in this range require it.
        // Note: this covers some pre-Chromium Edge versions, 
        // but pre-Chromium Edge does not require SameSite=None.
        if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            return true;

        return false;
    }
}