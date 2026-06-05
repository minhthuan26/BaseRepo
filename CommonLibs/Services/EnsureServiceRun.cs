using CommonLibs.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Services
{
    public class EnsureServiceRun : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnsureServiceRun> _logger;
        private readonly string _sectionName;

        public EnsureServiceRun(IConfiguration configuration, ILogger<EnsureServiceRun> logger, string sectionName)
        {
            _configuration = configuration;
            _logger = logger;
            _sectionName = sectionName;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("--- [Common.Libs] Bắt đầu kiểm tra các Windows Service phụ thuộc ---");

            if (!WindowsServiceHelper.IsWindows)
            {
                _logger.LogWarning("Ứng dụng không chạy trên môi trường Windows. Bỏ qua kiểm tra Windows Service.");
                return;
            }

            if (string.IsNullOrEmpty(_sectionName.Trim()))
            {
                _logger.LogWarning("Không tìm thấy WindowsServiceDependencies section");
                return;
            }

            // Đọc danh sách cấu hình từ mục "WindowsServiceDependencies" trong appsettings.json
            var servicesToCheck = _configuration.GetSection(_sectionName).Get<List<string>>();

            if (servicesToCheck == null || servicesToCheck.Count == 0)
            {
                _logger.LogInformation("Không có Windows Service nào được cấu hình cần kiểm tra.");
                return;
            }

            foreach (var serviceName in servicesToCheck)
            {
                if (string.IsNullOrWhiteSpace(serviceName)) continue;

                await EnsureServiceRunningAsync(serviceName, cancellationToken);
            }

            _logger.LogInformation("--- [Common.Libs] Kiểm tra và khởi chạy các dịch vụ hoàn tất ---");
        }

        private async Task EnsureServiceRunningAsync(string serviceName, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Đang kiểm tra trạng thái dịch vụ: {serviceName}...");

            var status = WindowsServiceHelper.GetStatus(serviceName);

            switch (status)
            {
                case WindowsServiceStatus.NotInstalled:
                    _logger.LogError($"Dịch vụ [{serviceName}] CHƯA ĐƯỢC CÀI ĐẶT trên máy này! Vui lòng kiểm tra lại.");
                    break;

                case WindowsServiceStatus.Running:
                    _logger.LogInformation($"Dịch vụ [{serviceName}] đã đang hoạt động.");
                    break;

                case WindowsServiceStatus.Stopped:
                    _logger.LogWarning($"Dịch vụ [{serviceName}] đang dừng. Tiến hành kích hoạt...");

                    // Sử dụng luôn Helper đã viết trước đó, truyền timeout mặc định 30 giây
                    bool isSuccess = await WindowsServiceHelper.StartServiceAsync(serviceName, 30, cancellationToken);

                    if (isSuccess)
                        _logger.LogInformation($"Kích hoạt thành công! Dịch vụ [{serviceName}] hiện đã hoạt động.");
                    else
                        _logger.LogError($"KHÔNG THỂ KÍCH HOẠT dịch vụ [{serviceName}]. Vui lòng chạy ứng dụng với quyền Administrator.");
                    break;

                case WindowsServiceStatus.Pending:
                    _logger.LogWarning($"Dịch vụ [{serviceName}] đang trong trạng thái chờ xử lý (Pending).");
                    break;

                default:
                    _logger.LogError($"Không thể xác định trạng thái của dịch vụ [{serviceName}].");
                    break;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
