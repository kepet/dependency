using System;

namespace Dependency
{
	public class TimeDependency : Dependency
	{
		public override string Type => "TIME";
		public TimeSpan From { get; set; }
		public TimeSpan To { get; set; }
		public DependencyAction Action { get; set; }

		public TimeDependency(TimeSpan from, DependencyAction action = DependencyAction.ReleaseDep)
		{
			From = from;
			To = new TimeSpan(23,59,59,999);
			Action = action;
		}
		public TimeDependency(TimeSpan from, TimeSpan to, DependencyAction action = DependencyAction.ReleaseDep)
		{
			From = from;
			To = to;
			Action = action;
		}
		public override bool Refresh(IScheduler sched, IDependContext context)
		{
			if (From <= context.Now.TimeOfDay && context.Now.TimeOfDay <= To)
			{
				return SetState(ConvertState(Action));
			}
			return SetState(DependencyState.Blocked);
		}
	}
}




