using RabbitMQ.Client;

namespace OrderService.Services
{
    public class RabbitConnection : IRabbitConnection, IDisposable
    {
        private IConnection? _connection { get; set; }
        public IConnection Connection => _connection;

        public RabbitConnection()
        {
            InitiConnection();
        }

        private void InitiConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                UserName = "dev",
                Password = "password"
            };
            _connection = factory.CreateConnection();
        }

        public void Dispose()
        {
           _connection?.Dispose();
        }
    }
}
