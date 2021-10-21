using System;

namespace Dependency
{
    public interface ITimeInspectorContext
    {
        DateTime Now { get; }
    }

    public class TimeInspectorContext : ITimeInspectorContext
    {
        public DateTime Now => DateTime.Now;
    }

    public class TimeInspector : IInspector
    {
        private readonly ITimeInspectorContext _context;

        public static TimeSpan StartOfDay { get; } = new TimeSpan(0, 0, 0);
        public static TimeSpan EndOfDay { get; } = new TimeSpan(24, 0, 0);
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }

        public TimeInspector(TimeSpan from, TimeSpan to, ITimeInspectorContext context = null)
        {
            From = from;
            To = to;
            _context = context ?? new TimeInspectorContext();
        }
        public TimeInspector(TimeSpan from, ITimeInspectorContext context = null)
        {
            From = from;
            To = EndOfDay;
            _context = context ?? new TimeInspectorContext();
        }
		
        public InspectorState Allocate()
        {
            // TODO: truncate to milliseconds
            var time = _context.Now.TimeOfDay;
            if (From == To)
            {
                return InspectorState.Available;
            }
            else if (From < To)
            {
                if (From <= time && time < To)
                {
                    return InspectorState.Available;
                }
            }
            else
            {
                if (From <= time || time < To)
                {
                    return InspectorState.Available;
                }
            }
            return InspectorState.Blocked;
        }

        public void Regret()
        {
            throw new NotImplementedException();
        }

        public bool Take()
        {
            throw new NotImplementedException();
        }

        public void Release()
        {
            throw new NotImplementedException();
        }
		
        // private void UpdateRefresh()
        // {
        //     var time = _context.Now.TimeOfDay;
        //     DateTime refreshDateTime;
        //     if (time < From)
        //     {
        //         refreshDateTime = _context.Now.Date + From;
        //     }
        //     else if (To != EndOfDay && time < To)
        //     {
        //         refreshDateTime = _context.Now.Date + To;
        //     }
        //     else
        //     {
        //         refreshDateTime = _context.Now.Date.AddDays(1) + From;
        //     }
        //     if (refreshDateTime < _context.NewRefresh) context.NewRefresh = refreshDateTime;
        // }
    }
}