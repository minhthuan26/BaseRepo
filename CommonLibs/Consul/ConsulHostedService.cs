using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Consul
{
    public class ConsulHostedService : IHostedService
    {
        private readonly IConsulClient _consulClient;
        private readonly ConsulOption _consulOption;
        private readonly ILogger<ConsulHostedService> _logger;

        public ConsulHostedService(
            IConsulClient consulClient,
            IOptions<ConsulOption> consulOption,
            ILogger<ConsulHostedService> logger)
        {
            _consulClient = consulClient;
            _consulOption = consulOption.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Đang đăng ký service {ServiceName} lên Consul...", _consulOption.ServiceName);

            var registration = new AgentServiceRegistration()
            {
                ID = _consulOption.ServiceId,
                Name = _consulOption.ServiceName,
                Address = _consulOption.ServiceIP,
                Port = _consulOption.ServicePort
            };

            // Nếu có cấu hình Health Check thì thêm vào
            if (!string.IsNullOrWhiteSpace(_consulOption.HealthCheckUrl))
            {
                registration.Check = new AgentServiceCheck()
                {
                    HTTP = _consulOption.HealthCheckUrl,
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1) // Tự xoá nếu sập quá 1 phút
                };
            }

            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
            _logger.LogInformation("Đăng ký thành công Service ID: {ServiceId}", _consulOption.ServiceId);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Đang hủy đăng ký service {ServiceId} khỏi Consul...", _consulOption.ServiceId);
            await _consulClient.Agent.ServiceDeregister(_consulOption.ServiceId, cancellationToken);
            _logger.LogInformation("Hủy đăng ký thành công.");
        }
    }
}
