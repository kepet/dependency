using System;

namespace Dependency.Test
{
    class TestDependContext : IDependContext
    {
        public TestDependContext( /*Scheduler sched*/)
        {
            //Param = new Dictionary<string, string>();
        }

        public DateTime Now { get; set; }

        public DateTime NewRefresh { get; set; }
        //public Dictionary<string, string> Param { get; set; }
    }
}