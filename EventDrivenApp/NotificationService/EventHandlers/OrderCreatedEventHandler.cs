public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        // Mock notification logic
        Console.WriteLine($"Notification: Order {@event.OrderId} created for user {@event.UserId}.");
        await Task.CompletedTask;
    }
}
