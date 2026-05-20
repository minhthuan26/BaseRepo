using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Grpc.Core;

namespace CommonLibs.Services;

public class GrpcClientHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GrpcClientHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TClient CreateClient<TClient>(string serviceUrl) where TClient : ClientBase<TClient>
    {
        // Khởi tạo Channel kết nối
        var channel = GrpcChannel.ForAddress(serviceUrl, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            }
        });

        // Tạo Client tương ứng từ file .proto
        var client = (TClient)Activator.CreateInstance(typeof(TClient), channel)!;
        return client;
    }

    // Hàm hỗ trợ tự động găm mã Correlation ID vào gRPC Metadata (Header của gRPC)
    public Metadata GetMetadataWithTrace()
    {
        var metadata = new Metadata();
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            metadata.Add("X-Correlation-ID", correlationId.ToString());
        }
        return metadata;
    }
}