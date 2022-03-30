using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using Rumble.Platform.Common.Web;
using tower_admin_portal.Utilities;

namespace tower_admin_portal
{
    public class Startup : PlatformStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services, Owner.Nathan, warnMS: 30_000, errorMS: 60_000, criticalMS: 90_000, webServerEnabled: true);
            
            services.ConfigureApplicationCookie(options => options.LoginPath = "/account/google-login");
            
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/account/google-login";
                })
                .AddGoogle(options =>
                {
                    options.ClientId = PlatformEnvironment.Require("GOOGLE_CLIENT_ID");
                    options.ClientSecret = PlatformEnvironment.Require("GOOGLE_CLIENT_SECRET");
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
        }

        protected override void ConfigureRoutes(IEndpointRouteBuilder builder)
        {
            builder.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
        }
    }
}