using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Dependency
{

	public enum DependencyState
	{
		Blocked,
		ReleaseThis,
		//ReleaseDepType,
		ReleaseAll,
   	    StepSuccess,
		StepError,
		StepSkip,
		//StepFail,
		//StepOk,
		//ParentOk,
		//ParentError,
		//JobOk,
		//JobError,
		//JobTerminate,

	}

	public enum StepState
	{
		NotSub,
		////StateHold,
		WaitDep,
		////StateWaitRerun,
		Queued,
		Running,
		////StatePause,
		////StateDebug,
		////StateReview,
	    Success,
	    Error,
	    Skipped,
		////Fail,
		////Abend,
		//Terminated,
		Timeout,
	}

	public enum DependencyAction
	{
		ReleaseDep,
		//ReleaseDepType,
		ReleaseAll,
	    StepSuccess,
	    StepError,
		StepSkip,
		//StepFail,
		//ParentOk,
		//ParentError,
		//JobOk,
		//JobError,
		//JobTerminate,
	}

	public class Dependency : IDependency
	{
		public Dependency()
		{
			State = DependencyState.Blocked;
		}

		public virtual string Type => "";
		public virtual DependencyState State { get; protected set; }  
		public virtual bool Refresh(IScheduler sched, IDependContext context)
		{
			return false;
		}

		protected bool SetState(DependencyState newState)
		{
			if (State == newState) return false;
			State = newState;
			return true;
		}

		protected static DependencyState ConvertState(DependencyAction action)
		{
			switch (action)
			{
				case DependencyAction.ReleaseDep:
					return DependencyState.ReleaseThis;
				case DependencyAction.ReleaseAll:
					return DependencyState.ReleaseAll;
			    case DependencyAction.StepSuccess:
			        return DependencyState.StepSuccess;
			    case DependencyAction.StepError:
			        return DependencyState.StepError;
			    case DependencyAction.StepSkip:
			        return DependencyState.StepSkip;
//			    case DependencyAction.StepSkip:
//			        return DependencyState.StepSkip;
			}
			return DependencyState.Blocked;
		}

	}
}
