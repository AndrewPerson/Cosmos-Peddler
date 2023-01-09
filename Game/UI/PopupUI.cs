namespace CosmosPeddler.Game;

public partial class PopupUI<DataT> : ReactiveUI<DataT>
{
    private static PopupUI<DataT> instance = null!;

    public static void Show(DataT data)
    {
        instance.Data = data;
        UINode.Instance.Show(instance);
    }

    public override void _Ready()
    {
        instance = this;
    }
}