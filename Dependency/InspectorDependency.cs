using System;
using System.Runtime.InteropServices.ComTypes;

namespace Dependency
{
	public class InspectorDependency : Dependency
	{

		public IInspector Inspector { get; /*set;*/ }
		
		public DependencyAction Action { get; /*set;*/ }

		public InspectorDependency(IInspector inspector, DependencyAction action = DependencyAction.ReleaseDep)
		{
			Inspector = inspector;
			Action = action;
		}
		public override bool Refresh(IScheduler sched, IDependContext context)
		{
			switch (Inspector.Allocate(/*IScheduler sched, IDependContext context*/))
			{
				case InspectorState.Unknown:
					// TODO: Timeout of unreachable resources
					break;
				case InspectorState.Blocked:
					return SetState(DependencyState.Blocked);
					break;
				case InspectorState.Available:
					return SetState(ConvertState(Action));
					break;
				case InspectorState.Expired:
					// TODO: 
					break;
			}
			return false;
		}
	}
}




