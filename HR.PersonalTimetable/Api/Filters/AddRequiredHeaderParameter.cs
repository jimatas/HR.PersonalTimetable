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

            operation.Parameters.Add(new()
            {
                Name = "X-HR-Authorization",
                In = ParameterLocation.Header,
                Description = "Hex encoded SHA-256 hash digest of the lowercased username, the integration's signing key and the Unix timestamp in seconds, concatenated together in that order."
                    + "<pre>hex(sha256(lowercase(username) + signingkey + timestamp))</pre>",
                Required = true,
                AllowEmptyValue = false,
                Schema = new OpenApiSchema { Type = "string", Pattern = "^[0-9A-Fa-f]{64}$" }
            });

            operation.Parameters.Add(new()
            {
                Name = "X-HR-Integration",
                In = ParameterLocation.Header,
                Description = "Name of the integration whose signing key was used to compute the hash.",
                Required = true,
                AllowEmptyValue = false,
                Schema = new OpenApiSchema { Type = "string" }
            });

            operation.Parameters.Add(new()
            {
                Name = "X-HR-Timestamp",
                In = ParameterLocation.Header,
                Description = "Unix timestamp that was used to compute the hash. This value represents the number of UTC seconds that have passed on the client since the Unix epoch.",
                Required = true,
                AllowEmptyValue = false,
                Schema = new OpenApiSchema { Type = "integer" }
            });
        }
    }
}
