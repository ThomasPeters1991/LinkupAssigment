public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent @event);
}
