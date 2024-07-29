using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MQManager.Services
{
    public class RabbitMQSetupService : IDisposable
    {
        private readonly ILogger<RabbitMQSetupService> _logger;
        private readonly RabbitMQSettingsDTO _rabbitMQSettings;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQSetupService(IOptions<RabbitMQSettingsDTO> rabbitMQSettings, ILogger<RabbitMQSetupService> logger)
        {
            _logger = logger;
            _rabbitMQSettings = rabbitMQSettings.Value;
        }

        public async Task InitializeAsync()
        {
            bool isConnected = await RetryPolicyAsync(CreateConnectionAsync, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));

            if (isConnected)
            {
                CreateQueues();
            }
            else
            {
                _logger.LogError("Failed to connect to RabbitMQ after multiple retries.");
            }
        }

        private async Task<bool> RetryPolicyAsync(Func<Task<bool>> action, TimeSpan retryInterval, TimeSpan timeout)
        {
            var stopTime = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < stopTime)
            {
                if (await action())
                {
                    return true;
                }

                _logger.LogInformation($"Retrying in {retryInterval.TotalSeconds} seconds...");
                await Task.Delay(retryInterval);
            }

            return false;
        }

        private async Task<bool> CreateConnectionAsync()
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
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not create RabbitMQ connection: {ex.Message}");
                return false;
            }
        }

        public void CreateQueues()
        {
            try
            {
                _logger.LogInformation("Creating queues...");

                _channel.QueueDeclare(queue: _rabbitMQSettings.OrdersQueueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);
                _logger.LogInformation($"Queue {_rabbitMQSettings.OrdersQueueName} declared.");

                _channel.QueueDeclare(queue: _rabbitMQSettings.UserInfoQueueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);
                _logger.LogInformation($"Queue {_rabbitMQSettings.UserInfoQueueName} declared.");

                _logger.LogInformation("Queues declared successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not declare RabbitMQ queues: {ex.Message}");
            }
        }

        public void ClearQueues()
        {
            try
            {
                _logger.LogInformation("Clearing queues...");

                _channel.QueueDelete(queue: _rabbitMQSettings.OrdersQueueName);
                _logger.LogInformation($"Queue {_rabbitMQSettings.OrdersQueueName} cleared.");

                _channel.QueueDelete(queue: _rabbitMQSettings.UserInfoQueueName);
                _logger.LogInformation($"Queue {_rabbitMQSettings.UserInfoQueueName} cleared.");

                _logger.LogInformation("Queues cleared successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not clear RabbitMQ queues: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
