using NotificationService.DTO;
using NotificationService.Services.Configurations;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace NotificationService.Services.Publishers
{
    public class publisher
    {

        public  async Task<User> requestUserDetailsAsync(IServiceProvider serviceProvider, int userId)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var rabbitMqService = scope.ServiceProvider.GetRequiredService<RabbitMQService>();
                using (var channel = rabbitMqService.Channel)
                {
                    var correlationId = Guid.NewGuid().ToString();
                    var replyQueueName = $"reply_{correlationId}";

                    // Declare the reply queue
                    channel.QueueDeclare(queue: replyQueueName, durable: false, exclusive: true, autoDelete: true, arguments: null);

                    var props = channel.CreateBasicProperties();
                    props.CorrelationId = correlationId;
                    props.ReplyTo = replyQueueName;

                    var message = new { UserId = userId };
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    // Send the request to the UserService request queue
                    channel.BasicPublish(exchange: "", routingKey: "user_info_queue", basicProperties: props, body: body);

                    var tcs = new TaskCompletionSource<User>();

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        if (ea.BasicProperties.CorrelationId == correlationId)
                        {
                            var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                            var userDetails = JsonConvert.DeserializeObject<User>(response);
                            tcs.SetResult(userDetails);
                        }
                    };

                    channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

                    return await tcs.Task;
                }
            }
        }
    }
}
