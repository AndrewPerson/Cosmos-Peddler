namespace CosmosPeddler.Game.UI;

public partial class PopupUI<DataT, DerivedT> : ReactiveUI<DataT>
{
    private static PopupUI<DataT, DerivedT> instance = null!;

    public static void Show(DataT data)
    {
        instance.Data = data;
        UINode.Instance.Show(instance);
        instance.Shown();
    }

    public virtual void Shown() { }

    public override void _Ready()
    {
        instance = this;
    }
}
