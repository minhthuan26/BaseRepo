using CommonLibs.Options;
using CommonLibs.Utils;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Services
{
    public class ServiceDiscoveryInfo
    {
        public string ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string Version { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime LastHeartbeat { get; set; }

        public string Url { get; set; } = string.Empty;

        public string MachineName { get; set; } = Environment.MachineName;

        public int ProcessId { get; set; } = Environment.ProcessId;

        public string Status { get; set; } = "Stopped";
    }

    public class ServiceDiscoveryHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IServer _server;
        private readonly IConfiguration _config;
        private readonly ILogger<ServiceDiscoveryHostedService> _logger;
        private readonly IRabbitMQService _rabbitMq;
        private PeriodicTimer? _heartbeatTimer;
        private CancellationTokenSource? _heartbeatTokenSource;
        private Task? _heartbeatTask;
        private readonly AppOptions _options;
        private ServiceDiscoveryInfo _serviceDiscoveryInfo;
        private readonly IRedisService _redisService;

        public ServiceDiscoveryHostedService(
            IHostApplicationLifetime lifetime,
            IServer server,
            IConfiguration config,
            ILogger<ServiceDiscoveryHostedService> logger,
            IRabbitMQService rabbitMq,
            IOptions<AppOptions> options,
            IRedisService redisService)
        {
            _lifetime = lifetime;
            _server = server;
            _config = config;
            _logger = logger;
            _rabbitMq = rabbitMq;
            _options = options.Value;
            _serviceDiscoveryInfo = new()
            {
                ApplicationId = _options.ApplicationId,
                ApplicationName = _options.ApplicationName,
                Version = _options.Version,
            };
            _redisService = redisService;
        }

        private async Task WaitRedisReady(CancellationToken token)
        {
            _logger.LogInformation("Waiting Redis...");

            for (int i = 0; i < 30; i++)
            {
                try
                {
                    if (RedisHelper.IsConnected)
                    {
                        _logger.LogInformation("Redis Ready.");
                        return;
                    }
                }
                catch
                {
                }

                await Task.Delay(1000, token);
            }

            throw new TimeoutException("Redis not available.");
        }

        private async Task WaitRabbitReady(CancellationToken token)
        {
            _logger.LogInformation("Waiting RabbitMQ...");

            for (int i = 0; i < 30; i++)
            {
                if (_rabbitMq.Ping())
                {
                    _logger.LogInformation("RabbitMQ Ready.");
                    return;
                }

                await Task.Delay(1000, token);
            }

            throw new TimeoutException("RabbitMQ not available.");
        }

        private async Task RegisterService()
        {
            _serviceDiscoveryInfo.Url = GetServerUrl();
            _serviceDiscoveryInfo.StartedAt = DateTime.UtcNow;
            _serviceDiscoveryInfo.LastHeartbeat = _serviceDiscoveryInfo.StartedAt;
            _serviceDiscoveryInfo.Status = "Running";

            await _redisService.SetAsync(
                $"Local_Url:{_serviceDiscoveryInfo.ApplicationId}",
                GetServerUrl(),
                TimeSpan.FromMinutes(2));

            _rabbitMq.Publish(
                "system.service.started",
                _serviceDiscoveryInfo);

            _logger.LogInformation(
                "Service [{Application}] registered.",
                _serviceDiscoveryInfo.ApplicationName);
        }

        private async Task UnRegisterService()
        {
            _serviceDiscoveryInfo.Status = "Stopped";
            _serviceDiscoveryInfo.LastHeartbeat = DateTime.UtcNow;

            await _redisService.RemoveAsync($"Local_Url:{_serviceDiscoveryInfo.ApplicationId}");

            _rabbitMq.Publish(
                "system.service.stopped",
                _serviceDiscoveryInfo);

            _logger.LogInformation(
                "Service [{Application}] unregistered.",
                _serviceDiscoveryInfo.ApplicationName);
        }

        private void StartHeartbeat()
        {
            _heartbeatTokenSource = new CancellationTokenSource();
            _heartbeatTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));

            _heartbeatTask = Task.Run(async () =>
            {
                try
                {
                    while (await _heartbeatTimer.WaitForNextTickAsync(_heartbeatTokenSource.Token))
                    {
                        await SendHeartbeatAsync(_heartbeatTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Heartbeat error.");
                }
            });
        }

        private void StopHeartbeat()
        {
            try
            {
                _heartbeatTokenSource?.Cancel();
                _heartbeatTimer?.Dispose();
            }
            catch
            {
            }
        }

        private async Task<Task> SendHeartbeatAsync(CancellationToken token)
        {
            _serviceDiscoveryInfo.LastHeartbeat = DateTime.UtcNow;

            await _redisService.SetAsync(
                $"Local_Url:{_serviceDiscoveryInfo.ApplicationId}",
                GetServerUrl(),
                TimeSpan.FromMinutes(2));

            _rabbitMq.Publish(
                "system.service.heartbeat",
                _serviceDiscoveryInfo);

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("===== Infrastructure Bootstrap Started =====");

            await WaitRedisReady(cancellationToken);

            await WaitRabbitReady(cancellationToken);

            _lifetime.ApplicationStarted.Register(async () =>
            {
                await RegisterService();

                StartHeartbeat();
            });

            _lifetime.ApplicationStopping.Register(async () =>
            {
                StopHeartbeat();

                await UnRegisterService();
            });

            _logger.LogInformation("===== Infrastructure Bootstrap Finished =====");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            StopHeartbeat();

            if (_heartbeatTask != null)
                await _heartbeatTask;
        }

        // Hàm hỗ trợ lấy IP/Port ứng dụng đang chạy
        private string GetServerUrl()
        {
            var feature = _server.Features.Get<IServerAddressesFeature>();

            return feature?.Addresses.FirstOrDefault()?.Replace("[::]", "localhost")
                   ?? "http://localhost";
        }
    }
}
