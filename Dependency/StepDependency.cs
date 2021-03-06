namespace Dependency
{
	public class StepDependency : Dependency
	{
		// public override string Type => "STEP";
		public string StepName { get; set; }
	    public DependencyAction OnSuccess { get; set; }
	    public DependencyAction OnError { get; set; }
	    public DependencyAction OnTimeout { get; set; }
	    public DependencyAction OnSkipped { get; set; }


	    public StepDependency (
		    string stepName,
		    DependencyAction onSuccess = DependencyAction.ReleaseDep,
		    DependencyAction onError = DependencyAction.StepSkip,
		    DependencyAction onTimeout = DependencyAction.StepSkip,
		    DependencyAction onSkipped = DependencyAction.StepSkip
		) {
			StepName = stepName;
			OnSuccess = onSuccess;
	        OnError = onError;
	        OnTimeout = onTimeout;
	        OnSkipped = onSkipped;
	    }

		public override bool Refresh(IScheduler sched, IDependContext context)
		{
			var dependTaskState = sched.GetStep(StepName).State;
			//bool changed = false;
			switch (dependTaskState)
			{
			    case StepState.Success:
			        return SetState(ConvertState(OnSuccess));
			    case StepState.Error:
			        return SetState(ConvertState(OnError));
			    case StepState.Timeout:
			        return SetState(ConvertState(OnTimeout));
			    case StepState.Skipped:
			        return SetState(ConvertState(OnTimeout));
			    case StepState.Queued:
					return false; // Already Scheduled,
			}
			return SetState(DependencyState.Blocked);
		}

	}
}