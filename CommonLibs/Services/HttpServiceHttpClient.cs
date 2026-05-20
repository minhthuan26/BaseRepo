using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace CommonLibs.Services;

public class HttpServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpServiceHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AttachCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            // Tự động đính kèm mã vết log vào request tiếp theo
            if (!_httpClient.DefaultRequestHeaders.Contains("X-Correlation-ID"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId.ToString());
            }
        }
    }

    public async Task<TResponse?> GetAsync<TResponse>(string baseUrl, string url)
    {
        AttachCorrelationId();
        var requestUrl = $"{baseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
        return await _httpClient.GetFromJsonAsync<TResponse>(requestUrl);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string baseUrl, string url, TRequest data)
    {
        AttachCorrelationId();
        var requestUrl = $"{baseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
        var response = await _httpClient.PostAsJsonAsync(requestUrl, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }
}