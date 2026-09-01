using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;
namespace Site.Api.Utils
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<IOpenApiParameter>();

            // Thêm Header X-Device-ID
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Device-ID",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Device identifier",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Default = JsonValue.Create("DEV-DEVICE-2604")
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
                    Type = JsonSchemaType.String,
                    Default = JsonValue.Create(Environment.MachineName)
                }
            });
        }
    }
}
