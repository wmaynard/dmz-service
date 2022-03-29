using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rumble.Platform.Common.Services;
using Rumble.Platform.Common.Utilities;
using tower_admin_portal.Utilities;

namespace tower_admin_portal
{
    // This should use PlatformStartup, and if changes are necessary to this to get the site up and running, common should be updated.
    // Without inheriting from PlatformStartup, many features of platform-common won't work out of the box; for example, the dependency injection
    // of common services.
    // TODO: Will to convert this
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
            // base.ConfigureServices(services, Owner.Nathan, warnMS: 30_000, errorMS: 60_000, criticalMS: 90_000, webServerEnabled: true);
            // services.ConfigureApplicationCookie(options => options.LoginPath = "/account/google-login");
            
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
                options.AddPolicy("Admin",
                    policy => policy.RequireClaim(ClaimTypes.Name)
                        .AddRequirements(new NameAuthorizationRequirement("Nathan Mac"))); // TODO flush out later
                options.AddPolicy("CompanyStaffOnly",
                    policy => policy.RequireClaim(ClaimTypes.Email)
                        .AddRequirements(new DomainRequirement("rumbleentertainment.com")));
            });

            services.AddSingleton<ApiService>();
            services.AddSingleton<DynamicConfigService>();
            
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}