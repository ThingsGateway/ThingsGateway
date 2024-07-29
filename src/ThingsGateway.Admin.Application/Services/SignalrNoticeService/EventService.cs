using BootstrapBlazor.Components;

namespace ThingsGateway.Admin.Application;
public interface IEventService<TEntry>
{
    Task Publish(string key, TEntry payload);
    void Subscribe(string key, Func<TEntry, Task> callback);
    void UnSubscribe(string key);
}
public class EventService<TEntry> : IEventService<TEntry>
{
    private Dictionary<string, Func<TEntry, Task>> Cache { get; } = new(50);

    public async Task Publish(string key, TEntry payload)
    {

        if (Cache.TryGetValue(key, out var func))
        {
            await func(payload);
        }
    }

    public void Subscribe(string key, Func<TEntry, Task> callback)
    {
        Cache.TryAdd(key, callback);
    }

    public void UnSubscribe(string key)
    {
        Cache.Remove(key);
    }


}
