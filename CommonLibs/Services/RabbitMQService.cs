using Newtonsoft.Json;
using RabbitMQ.Client;
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
        bool Ping();
    }

    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService(string hostName, string userName, string password, int port)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public bool Ping()
        {
            try
            {
                return _connection?.IsOpen == true &&
                       _channel?.IsOpen == true;
            }
            catch
            {
                return false;
            }
        }

        public RabbitMQService(string connectionString)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        private void DeclareQueue(string queueName)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public void Publish<T>(string queueName, T message)
        {
            DeclareQueue(queueName);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Lưu trữ tin nhắn xuống ổ đĩa, tránh mất khi restart RabbitMQ

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
