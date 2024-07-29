using NotificationService.DTO;
using NotificationService.Services.Configurations;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace NotificationService.Services.Publishers
{
    public class Publisher
    {
        private readonly ILogger<Publisher> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Publisher(ILogger<Publisher> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<User> RequestUserDetailsAsync(int userId, string queueName)
        {
            _logger.LogInformation("Requesting user details for UserId: {UserId}", userId);

            using (var scope = _serviceProvider.CreateScope())
            {
                var rabbitMqService = scope.ServiceProvider.GetRequiredService<RabbitMQService>();
                var correlationId = Guid.NewGuid().ToString();
                var replyQueueName = $"reply_{correlationId}";

                try
                {
                    var channel = rabbitMqService.Channel;
                    var replyQueue = channel.QueueDeclare(queue: replyQueueName, durable: false, exclusive: true, autoDelete: true, arguments: null);

                    var props = channel.CreateBasicProperties();
                    props.CorrelationId = correlationId;
                    props.ReplyTo = replyQueue.QueueName;

                    var message = new { UserId = userId };
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);

                    var tcs = new TaskCompletionSource<User>();
                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        if (ea.BasicProperties.CorrelationId == correlationId)
                        {
                            var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                            var userDetails = JsonConvert.DeserializeObject<User>(response);
                            tcs.SetResult(userDetails);

                            _logger.LogInformation("Received user details for UserId: {UserId}", userId);

                            // Ensure the reply queue is deleted after receiving the response
                            channel.QueueDelete(replyQueue.QueueName);
                        }
                    };

                    channel.BasicConsume(queue: replyQueue.QueueName, autoAck: true, consumer: consumer);

                    return await tcs.Task;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error requesting user details for UserId: {UserId}", userId);
                    throw;
                }
            }
        }
    }
}
