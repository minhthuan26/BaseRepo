using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CommonLibs.Utils
{
    public class RabbitMQHelper : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQHelper(
            string hostName,
            string userName,
            string password,
            int port = 5672)
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
            _channel = _connection.CreateModel();
        }

        /// <summary>
        /// Declare Queue
        /// </summary>
        public void DeclareQueue(string queueName)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        /// <summary>
        /// Publish Message
        /// </summary>
        public void Publish<T>(string queueName, T message)
        {
            DeclareQueue(queueName);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);
        }

        /// <summary>
        /// Send Message Alias
        /// </summary>
        public void SendMessage<T>(string queueName, T message)
        {
            Publish(queueName, message);
        }

        /// <summary>
        /// Subscribe / Consume
        /// </summary>
        public void Subscribe<T>(
            string queueName,
            Func<T, Task> onMessage)
        {
            DeclareQueue(queueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);

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

                    _channel.BasicAck(args.DeliveryTag, false);
                }
                catch
                {
                    _channel.BasicNack(args.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
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
