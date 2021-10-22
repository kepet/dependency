namespace Dependency
{
	public interface IScheduler
	{
	    SchedulerState State { get; }
	    Scheduler AddStep(Step step); // TODO Noget rod at retunere nedarvet klasse
		Step GetStep(string name);
		RefreshState RefreshDependency(IDependContext context);
	}
}