﻿namespace OrderService.Services
{
    public interface IMessageProducer
    {
        void SendMessage<T>(T message);
    }
}