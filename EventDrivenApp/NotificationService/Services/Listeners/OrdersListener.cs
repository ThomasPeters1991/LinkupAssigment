﻿using NotificationService.DTO;
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
        private readonly publisher _publisher;
        private readonly IServiceProvider _serviceProvider;


        public OrdersListener(RabbitMQService rabbitMqService, ILogger<OrdersListener> logger, publisher publisher, IServiceProvider serviceProvider)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
            _publisher = publisher;
            _serviceProvider = serviceProvider;
        }

        public void Listen()
        {
            _logger.LogInformation("Starting NotificationServiceListener...");

            var channel = _rabbitMqService.Channel;

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                 var orderRequest = JsonSerializer.Deserialize<Order>(message);
                var userdetail = await _publisher.requestUserDetailsAsync(_serviceProvider, orderRequest.UserId);
              

                _logger.LogInformation(orderRequest.ToString());
                _logger.LogInformation(userdetail.ToString());
                //take message, get Id, push onto UserInfoQueue, get reponse
            };

            channel.BasicConsume(queue: "orders_queue", autoAck: true, consumer: consumer);

            _logger.LogInformation("NotificationServiceListener started.");
        }


    }
}
