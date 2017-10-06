using System;
using System.Runtime.InteropServices.ComTypes;

namespace Dependency
{
	public class TimeDependency : Dependency
	{
	    public static TimeSpan StartOfDay { get; } = new TimeSpan(0, 0, 0, 0);
	    public static TimeSpan EndOfDay { get; } = new TimeSpan(23, 59, 59, 999);

		public override string Type => "TIME";
		public TimeSpan From { get; set; }
		public TimeSpan To { get; set; }
		public DependencyAction Action { get; set; }

		public TimeDependency(TimeSpan from, DependencyAction action = DependencyAction.ReleaseDep)
		{
			From = from;
			To = EndOfDay;
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
		    // TODO: truncate to milliseconds
		    var time = context.Now.TimeOfDay;
		    UpdateRefresh(context);
			if (From <= time && time <= To)
			{
				return SetState(ConvertState(Action));
			}
		    return SetState(DependencyState.Blocked);
		}

	    private void UpdateRefresh(IDependContext context)
	    {
	        var time = context.Now.TimeOfDay;
	        DateTime refreshDateTime;
	        if (time < From)
	        {
	            refreshDateTime = context.Now.Date + From;
	        }
	        else if (To != EndOfDay && time < To)
	        {
	            refreshDateTime = context.Now.Date + To;
	        }
	        else
	        {
	            refreshDateTime = context.Now.Date.AddDays(1) + From;
	        }
	        if (refreshDateTime < context.NewRefresh) context.NewRefresh = refreshDateTime;
	    }
	}
}




