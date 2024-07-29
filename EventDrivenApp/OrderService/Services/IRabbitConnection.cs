using RabbitMQ.Client;

namespace OrderService.Services
{
  public interface IRabbitConnection
    {
       IConnection Connection { get; } 
    }
}