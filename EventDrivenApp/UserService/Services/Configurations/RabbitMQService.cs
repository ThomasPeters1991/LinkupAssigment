using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace UserService.Services.Configurations
{
    public class RabbitMQService : IDisposable
    {
        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;

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

            var retryCount = 0;
            var maxRetries = 5;
            var retryDelay = TimeSpan.FromSeconds(10);

            while (retryCount < maxRetries)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _logger.LogInformation("Connected to RabbitMQ");


                    break;
                }
                catch (BrokerUnreachableException ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "Could not connect to RabbitMQ. Retrying in {RetryDelay} seconds... ({RetryCount}/{MaxRetries})", retryDelay.TotalSeconds, retryCount, maxRetries);
                    Thread.Sleep(retryDelay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while initializing RabbitMQ");
                    throw;
                }
            }

            if (_connection == null || _channel == null)
            {
                _logger.LogError("Failed to connect to RabbitMQ after {MaxRetries} retries", maxRetries);
                throw new Exception("Failed to connect to RabbitMQ");
            }
        }

        public IModel Channel => _channel;

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
