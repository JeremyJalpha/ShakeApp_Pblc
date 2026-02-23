public interface IBackgroundTaskRunner
{
    void Run(Func<Task> backgroundTask);
}