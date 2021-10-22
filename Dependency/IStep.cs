namespace Dependency
{
    public interface IStep
    {
        string Name { get; }
        StepState State { get; }
        bool AllowQueueRevoke { get; set; }

        Step AddDependency(Dependency dependency); // TODO: Noget Rod at retunrer nedarvet klasse
        RefreshState Refresh(Scheduler sched, IDependContext context);
    }
}