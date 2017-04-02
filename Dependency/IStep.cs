using System.Collections.Generic;

namespace Dependency
{
    public interface IStep
    {
        string Name { get; }
        StepState State { get; }
        bool AllowQueueRevoke { get; set; }

        void AddDependency(Dependency dependency);
        RefreshState Refresh(Scheduler sched, IDependContext context);
    }
}