using Developist.Core.Cqrs.DependencyInjection;
using Developist.Core.Persistence.EntityFrameworkCore.DependencyInjection;

using HR.Cwips.Client;
using HR.PersonalCalendar.Infrastructure;
using HR.PersonalCalendar.Persistence;
using HR.WebUntisConnector.Configuration;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using System;
using System.IO;
using System.Reflection;

namespace HR.PersonalCalendar.WebApi
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
            services.AddSingleton<IClock, SystemClock>();

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString(nameof(ApplicationDbContext))));
            services.AddUnitOfWork<ApplicationDbContext>();

            var configuration = WebUntisConfigurationSection.FromXmlFile("webuntis.config");
            services.AddSingleton(_ => configuration);

            services.AddCachedApiClientFactory(configuration);
            services.AddDispatcher();
            services.AddHandlersFromAssembly(Assembly.Load("HR.PersonalCalendar"));

            services.AddAuthentication(CwipsAuthenticationDefaults.AuthenticationScheme)
                .AddCwips(options =>
                {
                    options.AllowAuthenticationMethod = AuthenticationMethods.MobileChallengeResponse | AuthenticationMethods.NetworkCredentialsForTokenless | AuthenticationMethods.UsernameAndPasswordWithToken;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie();

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HR.PersonalCalendar.WebApi", Version = "v1" });

                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                if (File.Exists(xmlFilePath))
                {
                    c.IncludeXmlComments(xmlFilePath);
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR.PersonalCalendar.WebApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
