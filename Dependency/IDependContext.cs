using System;
using System.Collections.Generic;

namespace Dependency
{
	public interface IDependContext
	{
		DateTime Now { get; set; }	
		//Dictionary<string, string>	Param { get; set; }
	}
}