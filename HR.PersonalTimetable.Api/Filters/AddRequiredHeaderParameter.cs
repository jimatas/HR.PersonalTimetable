using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;
using System.Collections.Generic;

namespace HR.PersonalTimetable.Api.Filters
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!context.ApiDescription.RelativePath.StartsWith("api/personalization", StringComparison.OrdinalIgnoreCase)
                || string.Equals("GET", context.ApiDescription.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-HR-Authorization",
                In = ParameterLocation.Header,
                Description = "Salted hash of the username. The salt is the signing-key that was assigned to the integration."
                    + "<pre>hex(sha256(lowercase(username) + signingkey))</pre>",
                Required = true,
                AllowEmptyValue = false,
                Schema = new OpenApiSchema { Type = "string", Pattern = "^[0-9A-Fa-f]{64}$" }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-HR-Integration",
                In = ParameterLocation.Header,
                Description = "Name of the integration.",
                Required = true,
                AllowEmptyValue = false,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
