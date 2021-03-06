using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Dependency
{
    public enum RefreshState
    {
        Untouched,
        Updated,
    }

    public class Step : IStep, IGraphItem
    {
		public string Name { get; }
		public StepState State { get; internal set; }

		public bool AllowQueueRevoke { get; set; } = true;
		private List<Dependency> DependencyList { get; }

		public Step(string name)
		{
			Name = name;
			State = StepState.NotSub;
			DependencyList = new List<Dependency>();
		}

//        public IList<string> StepDependencyNameList()

        public List<string> Dependencies
        {
            get
            {
                var result = new List<string>();
                foreach (var dependency in DependencyList)
                {
                    if (dependency is StepDependency)
                    {
                        result.Add((dependency as StepDependency).StepName);
                    }
                }
                return result;
            }
        }

        public Step AddDependency(Dependency dependency)
		{
			DependencyList.Add(dependency);
			return this;
		}

		public RefreshState Refresh(Scheduler sched, IDependContext context)
		{
		    bool wasUpdated = false;
		    switch (State)
		    {
		        case StepState.NotSub:
		            // Hold
		            State = StepState.WaitDep;
		            wasUpdated = true;
		            break;

		        ////StateHold,
		        case StepState.WaitDep:
		            // Analysing dependencies happens below
		            break;
		        ////StateWaitRerun,
		        case StepState.Queued:
		            if (!AllowQueueRevoke) return RefreshState.Untouched;
		            // Maybe a dependency set this back to WaitDep
		            break;
		        case StepState.Running:
		            return RefreshState.Untouched;
		        ////StatePause,
		        ////StateDebug,
		        ////StateReview,
		        case StepState.Success:
		        case StepState.Skipped:
//		        case StepState.Terminated:
//		        case StepState.Timeout:
		        case StepState.Error:
		            // Complete
		            return RefreshState.Untouched;
		    }

			if (DependencyList.Count == 0)
			{
				if (State == StepState.WaitDep)
				{
					State = StepState.Queued;
					return RefreshState.Updated;
				}
				return RefreshState.Untouched;
			}

			int waitCount = 0;
			bool setState = false;
			StepState newState = StepState.NotSub;
			//Dictionary<string, int> dependencyTypeWaitCount = new Dictionary<string, int>();

			foreach (var dependency in DependencyList)
			{
				dependency.Refresh(sched, context);
			    switch (dependency.State)
			    {
			        case DependencyState.Blocked:
			            switch (State)
			            {
			                case StepState.Queued:
			                    setState = true;
			                    newState = StepState.WaitDep;
			                    break;
			                default:
			                    waitCount++;
			                    break;
			            }
			            break;

			        case DependencyState.ReleaseThis:
			            switch (State)
			            {
			                case StepState.WaitDep:
			                    break;
			                default:
			                    break;
			            }
			            break;

			        case DependencyState.ReleaseAll:
			            switch (State)
			            {
			                case StepState.WaitDep:
			                    waitCount = int.MinValue;
			                    break;
			                default:
			                    break;
			            }
			            break;

			        case DependencyState.StepSuccess:
			            if (newState != StepState.Error && newState != StepState.Skipped)
			            {
			                setState = true;
			                newState = StepState.Success;
			            }
			            break;

			        case DependencyState.StepError:
			            if (newState != StepState.Skipped)
			            {
			                setState = true;
			                newState = StepState.Error;
			            }
			            break;

			        case DependencyState.StepSkip:
			            setState = true;
			            newState = StepState.Skipped;
			            break;

			        default:
//			            switch ()
//			            Success,
//			            Skipped,
//			            ////Fail,
//			            ////Abend,
//			            //Terminated,
//			            //Timeout,
//			            Error,
//
//			            //bool setState = false;
//			            evalState =
			            break;
			    }
			}

			if (setState)
			{
				State = newState;
				return RefreshState.Updated;
			}
			if (State == StepState.WaitDep)
			{
				if (waitCount <= 0)
				{
					State = StepState.Queued;
					return RefreshState.Updated;
				}
			}
			return wasUpdated ? RefreshState.Updated : RefreshState.Untouched;
		}
	}
}