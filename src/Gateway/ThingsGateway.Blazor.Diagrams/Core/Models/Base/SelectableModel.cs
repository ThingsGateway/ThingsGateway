namespace ThingsGateway.Blazor.Diagrams.Core.Models.Base;

public abstract class SelectableModel : Model
{
    private int _order;

    public event Action<SelectableModel>? OrderChanged;

    protected SelectableModel() { }

    protected SelectableModel(string id) : base(id) { }

    public bool Selected { get; internal set; }
    public int Order
    {
        get => _order;
        set
        {
            if (value == Order)
                return;

            _order = value;
            OrderChanged?.Invoke(this);
        }
    }
}
