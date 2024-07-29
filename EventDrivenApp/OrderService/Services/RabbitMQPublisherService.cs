using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
namespace OrderService.Services
{
    public class RabbitMQPublisherService : IMessageProducer
    {
        private readonly IRabbitConnection _connection;

        public RabbitMQPublisherService(IRabbitConnection connection)
        {
            _connection = connection;   
        }

        public void SendMessage<T>(T message)
        {
            using var channel = _connection.Connection.CreateModel();
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange:"", routingKey:"orders_queue", body:body);
        }
    }
}
