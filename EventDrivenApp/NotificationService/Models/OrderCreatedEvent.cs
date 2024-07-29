public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }

    public OrderCreatedEvent(int orderId, int userId)
    {
        OrderId = orderId;
        UserId = userId;
    }
}
