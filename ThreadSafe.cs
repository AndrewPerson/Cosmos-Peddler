using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosPeddler;

public partial class ThreadSafe : Node
{
    private static TaskCompletionSource<SynchronizationContext> context = new TaskCompletionSource<SynchronizationContext>();

    public static void Run(Action action)
    {
        context.Task.Result.Post(_ => action(), null);
    }

    public static void Run<T>(Action<T> action, T arg)
    {
        context.Task.Result.Post(arg => action((T)arg!), arg);
    }

    public static void RunSync(Action action)
    {
        context.Task.Result.Send(_ => action(), null);
    }

    public static void RunSync<T>(Action<T> action, T arg)
    {
        context.Task.Result.Send(arg => action((T)arg!), arg);
    }

    public override void _Ready()
    {
        context.TrySetResult(SynchronizationContext.Current!);
    }
}