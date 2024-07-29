using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;

namespace NotificationService.Services.Configurations
{
    public class RabbitMQService : IDisposable
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly ILogger<RabbitMQService> _logger;
        private IConnection _connection;
        private IModel _channel;

        private const int MaxRetries = 5;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

        public RabbitMQService(string hostName, string userName, string password, ILogger<RabbitMQService> logger)
        {
            _hostName = hostName;
            _userName = userName;
            _password = password;
            _logger = logger;

            InitializeRabbitMq();
        }

        private void InitializeRabbitMq()
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };

            for (int retryCount = 0; retryCount < MaxRetries; retryCount++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _logger.LogInformation("Connected to RabbitMQ.");
                    return;
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.LogWarning(ex, "Could not connect to RabbitMQ. Retrying in {RetryDelay} seconds... ({RetryCount}/{MaxRetries})", RetryDelay.TotalSeconds, retryCount + 1, MaxRetries);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while initializing RabbitMQ.");
                    throw;
                }

                Thread.Sleep(RetryDelay);
            }

            _logger.LogError("Failed to connect to RabbitMQ after {MaxRetries} retries.", MaxRetries);
            throw new Exception("Failed to connect to RabbitMQ");
        }

        public IModel Channel
        {
            get
            {
                if (_channel == null)
                {
                    throw new InvalidOperationException("RabbitMQ channel is not initialized.");
                }
                return _channel;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while disposing RabbitMQ resources.");
            }
        }
    }
}
