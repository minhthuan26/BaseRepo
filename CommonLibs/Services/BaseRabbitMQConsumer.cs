using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibs.Services
{
    public abstract class BaseRabbitMQConsumer<TMessage> : BackgroundService
    {
        private readonly IConnection _connection;
        protected readonly IModel _channel;
        private readonly string _queueName;

        protected BaseRabbitMQConsumer(IConfiguration configuration, string queueName)
        {
            _queueName = queueName;

            // Đọc cấu hình từ appsettings của service đang chạy
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                DispatchConsumersAsync = true // Kích hoạt xử lý bất đồng bộ (Async) cho Consumer
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Đảm bảo Queue luôn tồn tại
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            // Cấu hình PrefetchCount = 1 để chia đều tải nếu chạy nhiều cụm IIS (Fair Dispatch)
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonConvert.DeserializeObject<TMessage>(json);

                    if (message != null)
                    {
                        // Gọi hàm xử lý logic - Hàm này sẽ do Service con tự viết
                        bool isSuccess = await ProcessMessageAsync(message);

                        if (isSuccess)
                        {
                            _channel.BasicAck(ea.DeliveryTag, false); // Xác nhận thành công, xóa tin khỏi hàng đợi
                            return;
                        }
                    }

                    // Nếu xử lý lỗi, đẩy lại vào hàng đợi để xử lý lại (hoặc cho vào Dead Letter Queue tùy bạn)
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }
                catch
                {
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Các service con bắt buộc phải override hàm này để viết logic nghiệp vụ cụ thể
        /// </summary>
        /// <returns>Trả về true nếu xử lý tin nhắn thành công, false nếu thất bại</returns>
        protected abstract Task<bool> ProcessMessageAsync(TMessage message);

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
