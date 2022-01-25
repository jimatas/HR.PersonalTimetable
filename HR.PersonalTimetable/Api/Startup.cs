using Developist.Core.Cqrs.DependencyInjection;
using Developist.Core.Persistence.EntityFrameworkCore.DependencyInjection;

using HR.PersonalTimetable.Api.Filters;
using HR.PersonalTimetable.Application;
using HR.PersonalTimetable.Application.Services;
using HR.PersonalTimetable.Infrastructure.Extensions;
using HR.PersonalTimetable.Infrastructure.Persistence;
using HR.PersonalTimetable.Infrastructure.Services;
using HR.WebUntisConnector.Configuration;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

            var configuration = WebUntisConfigurationSection.FromXmlFile(Path.Combine(AppContext.BaseDirectory, "Api", "webuntis.config"));
            services.AddSingleton(_ => configuration);

            services.AddCachedApiClientFactory(configuration);
            services.AddDispatcher();
            services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());

            services.AddHttpContextAccessor();

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers(options => options.Filters.Add(new ApiExceptionFilterAttribute())).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "HR.PersonalTimetable.Api", Version = "v2" });
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
            app.UseSwaggerUI(c => c.SwaggerEndpoint("v2/swagger.json", "HR.PersonalTimetable.Api v2"));

            app.UseCors();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}