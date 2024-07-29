using Newtonsoft.Json;
using NotificationService.DTO;
using NotificationService.Services.Configurations;
using NotificationService.Services.Publishers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Services.Listeners
{
    public class OrdersListener
    {
        private readonly RabbitMQService _rabbitMqService;
        private readonly ILogger<OrdersListener> _logger;
        private readonly Publisher _publisher;
        private const string ORDER_QUEUE_NAME = "orders_queue";
        private const string USER_QUEUE_NAME = "user_info_queue";
        public OrdersListener(RabbitMQService rabbitMqService, ILogger<OrdersListener> logger, Publisher publisher)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
            _publisher = publisher;
        }

        public void Listen()
        {
            _logger.LogInformation("Starting OrdersListener...");

            var channel = _rabbitMqService.Channel;

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                _logger.LogInformation("Message received in OrdersListener.");

                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var orderRequest = JsonConvert.DeserializeObject<Order>(message);

                    if (orderRequest == null)
                    {
                        _logger.LogWarning("Received null orderRequest.");
                        return;
                    }

                    _logger.LogInformation("OrderRequest deserialized successfully.");

                    var userDetail = await _publisher.RequestUserDetailsAsync(orderRequest.UserId, USER_QUEUE_NAME);
                    _logger.LogInformation("User details received successfully.");

                    LogOrderAndUserDetails(orderRequest, userDetail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message in OrdersListener.");
                }
            };

            channel.BasicConsume(queue: ORDER_QUEUE_NAME, autoAck: true, consumer: consumer);

            _logger.LogInformation("OrdersListener started.");
        }

        private void LogOrderAndUserDetails(Order order, User user)
        {
            _logger.LogInformation($"Order details: {JsonConvert.SerializeObject(order)}");
            _logger.LogInformation($"User details: {JsonConvert.SerializeObject(user)}");
        }
    }
}
