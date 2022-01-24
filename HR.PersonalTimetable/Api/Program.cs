using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System;
using System.IO;

namespace HR.PersonalTimetable.Api
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
            .UseDefaultServiceProvider(options => options.ValidateScopes = false)
            .UseContentRoot(Path.Combine(AppContext.BaseDirectory, "Api"));
    }
}
