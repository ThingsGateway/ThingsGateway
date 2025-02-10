using System.Collections;

using ThingsGateway.Blazor.Diagrams.Core.Models.Base;

namespace ThingsGateway.Blazor.Diagrams.Core;

public abstract class BaseLayer<T> : IReadOnlyList<T> where T : Model
{
    private readonly List<T> _items = new List<T>();

    public event Action<T>? Added;
    public event Action<T>? Removed;

    public BaseLayer(Diagram diagram)
    {
        Diagram = diagram;
    }

    public virtual TSpecific Add<TSpecific>(TSpecific item) where TSpecific : T
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        Diagram.Batch(() =>
        {
            _items.Add(item);
            OnItemAdded(item);
            Added?.Invoke(item);
        });
        return item;
    }

    public virtual void Add(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        Diagram.Batch(() =>
        {
            foreach (var item in items)
            {
                _items.Add(item);
                OnItemAdded(item);
                Added?.Invoke(item);
            }
        });
    }

    public virtual void Remove(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (_items.Remove(item))
        {
            Diagram.Batch(() =>
            {
                OnItemRemoved(item);
                Removed?.Invoke(item);
            });
        }
    }

    public virtual void Remove(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        Diagram.Batch(() =>
        {
            foreach (var item in items)
            {
                if (_items.Remove(item))
                {
                    OnItemRemoved(item);
                    Removed?.Invoke(item);
                }
            }
        });
    }

    public bool Contains(T item) => _items.Contains(item);

    public void Clear()
    {
        if (Count == 0)
            return;

        Diagram.Batch(() =>
        {
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];
                _items.RemoveAt(i);
                OnItemRemoved(item);
                Removed?.Invoke(item);
            }
        });
    }

    protected virtual void OnItemAdded(T item) { }

    protected virtual void OnItemRemoved(T item) { }

    public Diagram Diagram { get; }

    public int Count => _items.Count;
    public T this[int index] => _items[index];
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
