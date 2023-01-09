using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosPeddler.Game;

public partial class ReactiveUI<DataT> : Control
{
    private DataT? _data;
    public DataT Data
    {
        get => _data!;
        set
        {
            _data = value;
            UpdateUI();
        }
    }

    public virtual void UpdateUI() { }

    public void RenderList<ListDataT, TemplateT>(Node listContainer, IList<ListDataT> items, Action<ListDataT, TemplateT> assignData, Func<TemplateT> create) where TemplateT : Control
    {
        for (int i = 0; i < Mathf.Min(listContainer.GetChildCount(), items.Count); i++)
        {
            var listItem = listContainer.GetChild<TemplateT>(i);
            assignData(items[i], listItem);
        }

        for (int i = Mathf.Min(listContainer.GetChildCount(), items.Count); i < items.Count; i++)
        {
            var listItem = create();
            listItem.Ready += () => assignData(items[i], listItem);

            listContainer.AddChild(listItem);
        }

        for (int i = items.Count; i < listContainer.GetChildCount(); i++)
        {
            listContainer.GetChild(i).QueueFree();
        }
    }

    public void RenderList<ListDataT>(Node listContainer, PackedScene template, IList<ListDataT> items)
    {
        RenderList(listContainer, items, (item, listItem) => listItem.Data = item, () => template.Instantiate<ReactiveUI<ListDataT>>());
    }
}

public partial class AsyncReactiveUI<DataT> : ReactiveUI<DataT>
{
    public override void UpdateUI()
    {
        UpdateUIAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                GD.PrintErr(t.Exception);
                return;
            }
        },
        TaskScheduler.FromCurrentSynchronizationContext());
    }

    public virtual async Task UpdateUIAsync() { }
}