using System.Collections.Generic;

namespace Dependency
{
    public interface IStep
    {
        string Name { get; }
        StepState State { get; }
        bool AllowQueueRevoke { get; set; }

//        IList<string> StepDependencyNameList();
        void AddDependency(Dependency dependency);
        RefreshState Refresh(Scheduler sched, IDependContext context);
    }
}