namespace Dependency
{
	public interface IDependency
	{
		string Type { get; }
		DependencyState State { get; }
		bool Refresh(IScheduler sched, IDependContext context);
	}
}