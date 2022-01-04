using Developist.Core.Cqrs.DependencyInjection;
using Developist.Core.Persistence.EntityFrameworkCore.DependencyInjection;

using HR.Cwips.Client;
using HR.PersonalCalendar.Api.Extensions;
using HR.PersonalCalendar.Api.Filters;
using HR.PersonalCalendar.Api.Infrastructure;
using HR.PersonalCalendar.Api.Persistence;
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

namespace HR.PersonalCalendar.Api
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

            services.AddDbContext<PersonalCalendarDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString(nameof(PersonalCalendarDbContext))));
            services.AddUnitOfWork<PersonalCalendarDbContext>();

            var configuration = WebUntisConfigurationSection.FromXmlFile("webuntis.config");
            services.AddSingleton(_ => configuration);

            services.AddCachedApiClientFactory(configuration);
            services.AddDispatcher();
            services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());

            services.AddAuthentication(CwipsAuthenticationDefaults.AuthenticationScheme)
                .AddCwips(options =>
                {
                    options.AllowAuthenticationMethod = AuthenticationMethods.MobileChallengeResponse | AuthenticationMethods.NetworkCredentialsForTokenless | AuthenticationMethods.UsernameAndPassword;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie();
            
            services.AddHttpContextAccessor();

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers(options => options.Filters.Add(new ApiExceptionFilterAttribute())).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HR.PersonalCalendar.Api", Version = "v1" });
                c.CustomSchemaIds(type => type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? type.GetCustomAttribute<DisplayAttribute>()?.Name ?? type.Name);

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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR.PersonalCalendar.Api v1"));
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
