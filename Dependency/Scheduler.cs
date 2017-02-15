using System.Collections.Generic;

namespace Dependency
{
    public enum SchedulerState
    {
        NotSub,
        Active,
        Complete
    }

	public class Scheduler : IScheduler
	{
		public Scheduler()
		{
			StepList = new Dictionary<string, Step>();
		}

	    public SchedulerState State
	    {
	        get
	        {
	            bool active = false;
	            bool notsub = true;
	            foreach (var step in StepList)
	            {
	                switch (step.Value.State)
	                {
	                    case StepState.NotSub:
	                        break;
	                    case StepState.Success:
	                    case StepState.Skipped:
	                    //case StepState.Terminated,
	                    //case StepState.Timeout,
	                    case StepState.Error:
	                        notsub = false;
	                        break;
	                    default:
	                        notsub = false;
	                        active = true;
	                        break;
	                }
	            }

	            if (active) return SchedulerState.Active;
                if (notsub) return SchedulerState.NotSub;
	            return SchedulerState.Complete;
	        }
	    }

	    protected Dictionary<string, Step> StepList;

		public void AddStep(Step step)
		{
			StepList.Add(step.Name, step);
		}

		public Step GetStep(string name)
		{
			return StepList.ContainsKey(name) ? StepList[name] : null;
		}

		//public bool RefreshDependency(IDependContext context)
		//{
		//	bool wasUpdated = false;
		//	bool runUpdated = false;
		//	do
		//	{
		//		runUpdated = false;
		//		foreach (var step in StepList)
		//		{
		//			if (step.Value.Refresh(this, context)) runUpdated = true;
		//		}
		//		if (runUpdated) wasUpdated = true;
		//	} while (runUpdated);
		//	return wasUpdated;
		//}

	    public RefreshState RefreshDependency(IDependContext context)
		{
		    RefreshState result = RefreshState.Untouched;
		    foreach (var step in StepList)
		    {
		        switch (step.Value.Refresh(this, context))
		        {
		            case RefreshState.Untouched:
		                break;

		            case RefreshState.Updated:
		                result = RefreshState.Updated;
		                break;
			    }
			}
			return result;
		}

	}
}