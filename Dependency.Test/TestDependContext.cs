using System;
using System.Dynamic;
using System.Globalization;

namespace Dependency.Test
{
    internal class TestDependContext : IDependContext
    {
        public TestDependContext( /*Scheduler sched*/)
        {
            //Param = new Dictionary<string, string>();
        }

        // public DateTime Now { get; set; }
        //
        // public DateTime NewRefresh { get; set; }
        // //public Dictionary<string, string> Param { get; set; }
    }
    
    internal class TestTimeInspectorContext : ITimeInspectorContext
    {
        internal static TestTimeInspectorContext Create(string time = null)
        {
            var r = new TestTimeInspectorContext();
            r.Set(time);
            return r;
        }
        
        public DateTime Now { get; private set; }
        
        internal void Set(string time)
        {
            if (string.IsNullOrEmpty(time)) { Now = DateTime.MinValue; return; }
            Now = DateTime.ParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
    }
}