using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MQManager.Services
{
    public class RabbitMQSetupService
    {
        private readonly ILogger<RabbitMQSetupService> _logger;
        private readonly RabbitMQSettingsDTO _rabbitMQSettings;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQSetupService(IOptions<RabbitMQSettingsDTO> rabbitMQSettings, ILogger<RabbitMQSetupService> logger)
        {
            _logger = logger;
            _rabbitMQSettings = rabbitMQSettings.Value;
            CreateQueues();
        }

        private void CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.Host,
                UserName = _rabbitMQSettings.UserName,
                Password = _rabbitMQSettings.Password
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _logger.LogInformation("RabbitMQ connection created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not create RabbitMQ connection: {ex.Message}");
            }
        }

        public void CreateQueues()
        {
            try
            {
                CreateConnection();

                _channel.QueueDeclare(queue: _rabbitMQSettings.OrdersQueueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);

                _channel.QueueDeclare(queue: _rabbitMQSettings.UserInfoQueueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);

                _channel.QueueDeclare(queue: _rabbitMQSettings.ReplyQueueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);

                _logger.LogInformation("Queues declared successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not declare RabbitMQ queues: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
