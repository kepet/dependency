namespace Dependency
{
	public interface IScheduler
	{
	    SchedulerState State { get; }
	    void AddStep(Step step);
		Step GetStep(string name);
		RefreshState RefreshDependency(IDependContext context);
	}
}