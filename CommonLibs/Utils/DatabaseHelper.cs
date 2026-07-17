using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Utils
{
    public static class DatabaseHelper
    {
        public static bool TestSqlConnection(string connectionString, out string message)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        message = "Connection successful.";
                        return true;
                    }
                    else
                    {
                        message = "Connection failed.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static void ValidateDatabaseConnection<TDbContext>(this WebApplication app) where TDbContext : DbContext
        {
            // Bắt buộc phải tạo Scope mới vì DbContext mặc định là Scoped Service
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Lấy ra Logger xịn từ hệ thống (lúc này Serilog hay gì cũng đã sẵn sàng)
            var logger = services.GetRequiredService<ILogger<TDbContext>>();

            try
            {
                logger.LogInformation("Đang kiểm tra kết nối Database cho [{DbContextName}]...", typeof(TDbContext).Name);

                // Lấy DbContext ra từ DI Container
                var dbContext = services.GetRequiredService<TDbContext>();

                // Sử dụng tính năng có sẵn của EF Core để test kết nối (Rất an toàn và chuẩn xác)
                bool isConnected = dbContext.Database.CanConnect();

                if (isConnected)
                {
                    logger.LogInformation("Kết nối Database thành công!");
                }
                else
                {
                    logger.LogError("Kết nối Database thất bại. Vui lòng kiểm tra lại Connection String.");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Xảy ra lỗi nghiêm trọng khi cố gắng kết nối Database: {Message}", ex.Message);
            }
        }
    }
}
