using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace Site.Api.Utils
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            // Thêm Header X-Device-ID
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-ID",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Device identifier",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("DEV-DEVICE-2604")
                }
            });

            // Thêm Header X-Device-Name
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-Name",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Device name",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString(Environment.MachineName)
                }
            });
        }
    }
}
