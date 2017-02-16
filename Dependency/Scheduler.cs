using System.Collections.Generic;
using System.Linq;

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
			StepDictionary = new Dictionary<string, Step>();
		}

	    public SchedulerState State
	    {
	        get
	        {
	            bool active = false;
	            bool notsub = true;
	            foreach (var step in StepDictionary)
	            {
	                switch (step.Value.State)
	                {
	                    case StepState.NotSub:
	                        break;
	                    case StepState.Success:
	                    case StepState.Skipped:
	                    case StepState.Timeout:
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

	    protected Dictionary<string, Step> StepDictionary;

	    public IList<string> DepFunction(string name)
	    {
	        return StepDictionary[name].StepDependencyNameList();
	    }

	    protected IList<Step> OrderedStepList
	    {
	        get
	        {
	            if (_orderedStepList == null)
	            {
	                _orderedStepList = new List<Step>();
	                var nameList = StepDictionary.Select(step => step.Key).ToList();
	                var sorter = new TopologicalSorter();
	                var sortedNameList = sorter.Do(nameList, DepFunction);
	                foreach (var name in sortedNameList)
	                {
	                    _orderedStepList.Add(StepDictionary[name]);
	                }
	            }
	            return _orderedStepList;
	        }
	    }

		public void AddStep(Step step)
		{
			StepDictionary.Add(step.Name, step);
		    _orderedStepList = null;
		}

		public Step GetStep(string name)
		{
			return StepDictionary.ContainsKey(name) ? StepDictionary[name] : null;
		}

	    public RefreshState RefreshDependency(IDependContext context)
		{
		    RefreshState result = RefreshState.Untouched;
		    foreach (var step in OrderedStepList)
		    {
		        switch (step.Refresh(this, context))
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

	    private List<Step> _orderedStepList;
	}
}