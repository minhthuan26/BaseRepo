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
    public interface IRabbitMQService
    {
        void Publish<T>(string queueName, T message);
        void Subscribe<T>(string queueName, Func<T, Task> onMessage);
        bool Ping();
    }

    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;

        // Constructor nhận ConnectionString
        public RabbitMQService(string connectionString)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString),
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
        }

        public RabbitMQService(string hostName, string userName, string password, int port)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
        }

        public bool Ping()
        {
            try
            {
                return _connection?.IsOpen == true;
            }
            catch
            {
                return false;
            }
        }

        public void Publish<T>(string queueName, T message)
        {
            using var channel = _connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);
        }

        public void Subscribe<T>(string queueName, Func<T, Task> onMessage)
        {
            var channel = _connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (sender, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonConvert.DeserializeObject<T>(json);

                    if (message != null)
                    {
                        await onMessage(message);
                    }

                    channel.BasicAck(args.DeliveryTag, false);
                }
                catch
                {
                    // Trả lại tin nhắn về hàng đợi nếu quá trình xử lý (onMessage) bị lỗi
                    channel.BasicNack(args.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
