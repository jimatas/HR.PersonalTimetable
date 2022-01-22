using Developist.Core.Cqrs.DependencyInjection;
using Developist.Core.Persistence.EntityFrameworkCore.DependencyInjection;

using HR.CwipsClient;
using HR.PersonalTimetable.Api.Extensions;
using HR.PersonalTimetable.Api.Filters;
using HR.PersonalTimetable.Api.Infrastructure;
using HR.PersonalTimetable.Api.Models;
using HR.PersonalTimetable.Api.Persistence;
using HR.WebUntisConnector.Configuration;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

namespace HR.PersonalTimetable.Api
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
            services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));
            services.AddSingleton<IClock, SystemClock>();

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString(nameof(ApplicationDbContext))));
            services.AddUnitOfWork<ApplicationDbContext>();

            var configuration = WebUntisConfigurationSection.FromXmlFile("webuntis.config");
            services.AddSingleton(_ => configuration);

            services.AddCachedApiClientFactory(configuration);
            services.AddDispatcher();
            services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());
            
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("C:\\datamap\\Key_Ring"))
                .SetApplicationName("hrweb");

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CwipsAuthenticationDefaults.AuthenticationScheme;
            }).AddCwips(options =>
            {
                options.AllowAuthenticationMethod = AuthenticationMethods.MobileChallengeResponse | AuthenticationMethods.NetworkCredentialsForTokenless | AuthenticationMethods.UsernameAndPassword;
                options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
            }).AddCookie(options =>
            {
                options.Cookie.Name = ".AspNet.SharedCookie";
                options.Cookie.Path = "/";
            });

            services.AddHttpContextAccessor();

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers(options => options.Filters.Add(new ApiExceptionFilterAttribute())).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HR.PersonalTimetable.Api", Version = "v1" });
                c.CustomSchemaIds(type => type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? type.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? type.Name);
                c.OperationFilter<AddRequiredHeaderParameter>();

                foreach (var xmlFilePath in new[] {
                    Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"),
                    Path.Combine(AppContext.BaseDirectory, "HR.WebUntisConnector.xml")
                })
                {
                    if (File.Exists(xmlFilePath))
                    {
                        c.IncludeXmlComments(xmlFilePath);
                    }
                }
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowCredentials();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.SetIsOriginAllowed(_ => true);
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile(Configuration.GetSection("Serilog:FileLogging"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Allow use of Swagger outside of development.
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("../swagger/v1/swagger.json", "HR.PersonalTimetable.Api v1"));

            app.UseCors();

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
