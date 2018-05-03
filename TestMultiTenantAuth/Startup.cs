using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict;
using OpenIddict.Mvc;
using TestMultiTenantAuth.Entities;
using TestMultiTenantAuth.OpenIddict;

namespace TestMultiTenantAuth
{
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
            services.AddEntityFrameworkSqlServer()
                    .AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(Configuration.GetConnectionString("SecurityConnection"));
                        options.UseOpenIddict();
                    });

            services.AddMvc();

            services.AddOpenIddict(options =>
            {
                // Register the Entity Framework stores.
                options.AddEntityFrameworkCoreStores<ApplicationDbContext>();

                options.AddMvcBinders();

                options.AllowPasswordFlow();

                options.EnableTokenEndpoint("/connect/token");

                options.DisableHttpsRequirement();

                options.UseJsonWebTokens()
                        .Configure(opt =>
                        {
                            //opt.IdentityTokenLifetime = TimeSpan.FromDays(7);
                            opt.AccessTokenLifetime = TimeSpan.FromMinutes(30);
                        });

                options.AddEphemeralSigningKey();
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();


            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration.GetValue<string>("Authorization:Authority");
                    options.Audience = Configuration.GetValue<string>("Authorization:Audience");
                    //options.MetadataAddress = Configuration.GetValue<string>("Authorization:WellKnown");
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = OpenIdConnectConstants.Claims.Subject,
                        RoleClaimType = OpenIdConnectConstants.Claims.Role
                    };
                });

            services.AddSingleton<TenantProvider>();
            services.AddSingleton<IOptionsMonitor<OpenIddictOptions>, OpenIddictOptionsProvider>();
            services.AddSingleton<IConfigureOptions<OpenIddictOptions>, OpenIddictOptionsInitializer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(next => context =>
            {
                // This snippet uses a hardcoded resolution logic.
                // In a real world app, you'd want to customize that.
                if (context.Request.Path.StartsWithSegments("/fabrikam", out PathString path))
                {
                    context.Request.PathBase = "/fabrikam";
                    context.Request.Path = path;
                }

                return next(context);
            });

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
