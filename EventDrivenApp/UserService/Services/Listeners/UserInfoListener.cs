using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using UserService.DTO;
using UserService.Services.Configurations;

namespace UserService.Services.Listeners
{
    public class UserInfoListener
    {

        private readonly RabbitMQService _rabbitMqService;
        private readonly ILogger<UserInfoListener> _logger;
        private readonly IUserRepository _userRepository;

        public UserInfoListener(RabbitMQService rabbitMqService, ILogger<UserInfoListener> logger, IUserRepository userRepository)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
            _userRepository = userRepository;
        }

        public void Start()
        {
            _logger.LogInformation("Starting NotificationServiceListener...");

            var channel = _rabbitMqService.Channel;

          
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                 var request = JsonSerializer.Deserialize<UserRequest>(message);


                 
                var userDetails = _userRepository.GetUserByIdAsync(request.UserId);

                var responseProps = channel.CreateBasicProperties();
                responseProps.CorrelationId = ea.BasicProperties.CorrelationId;

                var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userDetails));

                channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo, basicProperties: responseProps, body: responseBody);

                _logger.LogInformation($"Sent response for UserId: {request.UserId}");

                _logger.LogInformation(message);
            };

            channel.BasicConsume(queue: "user_info_queue", autoAck: true, consumer: consumer);

            _logger.LogInformation("NotificationServiceListener started.");
        }
    }
}
