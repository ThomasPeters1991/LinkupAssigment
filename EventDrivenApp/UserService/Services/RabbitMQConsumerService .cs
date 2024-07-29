using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using UserService.Services.Configurations;
using System.Text;
using System.Text.Json;
using UserService.DTO;

namespace UserService.Services
{
    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;
        private readonly int _retryDelayInSeconds = 5;

        public RabbitMQConsumerService(IOptions<RabbitMQSettings> rabbitMQSettings, ILogger<RabbitMQConsumerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMQSettings = rabbitMQSettings.Value;
            _serviceProvider = serviceProvider;
        }

        private void CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.HostName,
                UserName = _rabbitMQSettings.UserName,
                Password = _rabbitMQSettings.Password
            };

            while (_connection == null || !_connection.IsOpen)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclarePassive(_rabbitMQSettings.QueueName);
                    _logger.LogInformation("RabbitMQ connection created and queue is available.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not create RabbitMQ connection or queue is not available: {ex.Message}");
                    _logger.LogInformation($"Retrying in {_retryDelayInSeconds} seconds...");
                    Thread.Sleep(_retryDelayInSeconds * 1000);
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CreateConnection();

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received message: {message}");

                var request = JsonSerializer.Deserialize<UserRequest>(message);
                if (request != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var user = await userRepository.GetUserByIdAsync(request.UserId);
                    var responseMessage = user != null ? JsonSerializer.Serialize(user) : "User not found";

                    // Send response back to RabbitMQ
                    var responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                    _channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo, basicProperties: replyProps, body: responseBytes);
                }
            };

            _channel.BasicConsume(queue: _rabbitMQSettings.QueueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
