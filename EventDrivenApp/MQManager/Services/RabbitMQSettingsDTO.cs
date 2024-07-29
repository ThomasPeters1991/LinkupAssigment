namespace MQManager.Services
{
    public class RabbitMQSettingsDTO
    {
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OrdersQueueName { get; set; }
        public string UserInfoQueueName { get; set; }
        public string ReplyQueueName { get; set; }
    }
}
