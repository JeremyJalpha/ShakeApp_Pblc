public class DefaultBackgroundTaskRunner : IBackgroundTaskRunner
{
    public void Run(Func<Task> backgroundTask)
    {
        _ = Task.Run(backgroundTask);
    }
}