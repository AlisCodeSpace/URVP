using Microsoft.OpenApi;

namespace RICHConnect.Backend.Api.Configuration.Swagger
{
    /// <summary>
    /// Configuration for Swagger/OpenAPI documentation
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Add Swagger services to the service collection
        /// </summary>
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RICHConnect API", Version = "v1" });
                
                // Configure API groups
                c.TagActionsBy(api =>
                {
                    if (api.GroupName != null)
                    {
                        return new[] { api.GroupName };
                    }

                    var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                    if (controllerActionDescriptor != null)
                    {
                        return new[] { controllerActionDescriptor.ControllerName };
                    }

                    return new[] { api.ActionDescriptor.RouteValues["controller"] };
                });
                
                c.DocInclusionPredicate((name, api) => true);
                
                // Note: Authentication is cookie-based, so Swagger UI requires browser cookies
            });

            return services;
        }

        /// <summary>
        /// Configure Swagger middleware in the application pipeline
        /// </summary>
        public static IApplicationBuilder UseSwaggerServices(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            // Enable Swagger only in Development and Staging
            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RICHConnect Auth API v1");
                    // Do NOT serve Swagger UI at site root, otherwise it overrides the SPA.
                    c.RoutePrefix = "swagger";
                });
            }

            return app;
        }
    }
}
