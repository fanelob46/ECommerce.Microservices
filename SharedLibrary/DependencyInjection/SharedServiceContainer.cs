using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SharedLibrary.Middleware;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddShared<TContext>
            (this IServiceCollection services, IConfiguration config, string fileName) where TContext:DbContext
        {
            //add generic database context
            services.AddDbContext<TContext>(option => option.UseSqlServer(
                config.GetConnectionString("eCommerceConnection"), sqlServerOption =>
                sqlServerOption.EnableRetryOnFailure()));

            //configure serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"{fileName}-.text",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {message:lj{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //Add JWT authentication Scheme
            JWTAuthScheme.AddJWTAuthenticationScheme(services,config);

            return services;
        }

        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            //use global Exception
            app.UseMiddleware<GlobalExeption>();

            //register middleware
            app.UseMiddleware<ListenToOnlyApiGateway>();

            return app;
        }
    }
}
