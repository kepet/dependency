using System;
using System.Globalization;
using Xunit;

namespace Dependency.Test
{
    public class TimeDependencyTest
    {
        public TimeSpan ToTimeSpan(string time)
        {
            return TimeSpan.ParseExact(time, "c", CultureInfo.InvariantCulture);
        }
        
        [Fact]
        public void TimeInspectorTestFromEqualTo()
        {
            var trx = TestTimeInspectorContext.Create();
            var tr = new TimeInspector(ToTimeSpan("10:00"), ToTimeSpan("10:00"), trx);
            var rs = InspectorState.Unknown;
            
            // Midnight
            trx.Set("00:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);

            // To Early
            trx.Set("09:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);

            // On Time
            trx.Set("10:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);

            // To Late
            trx.Set("11:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);
        }

        [Fact]
        public void TimeInspectorTestFromBeforeTo()
        {
            var trx = TestTimeInspectorContext.Create();
            var tr = new TimeInspector(ToTimeSpan("08:00"), ToTimeSpan("20:00"), trx);
            var rs = InspectorState.Unknown;
            
            // To Early
            trx.Set("00:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Blocked, rs);

            // To Early
            trx.Set("07:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Blocked, rs);

            // On Time
            trx.Set("08:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);

            // In Window
            trx.Set("12:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);

            // To Late
            trx.Set("20:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Blocked, rs);

            // To Late
            trx.Set("23:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Blocked, rs);
        }

        [Fact]
        public void TimeInspectorTestFromAfterTo()
        {
            var trx = TestTimeInspectorContext.Create();
            var tr = new TimeInspector(ToTimeSpan("22:00"), ToTimeSpan("07:00"), trx);
            var rs = InspectorState.Unknown;
            
            // To Early
            trx.Set("21:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Blocked, rs);

            // On Time
            trx.Set("22:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);

            // In Window
            trx.Set("23:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);
            
            // In Window
            trx.Set("00:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);

            // In Window
            trx.Set("06:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Available, rs);
            
            // To Late
            trx.Set("07:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Blocked, rs);

            // To Late
            trx.Set("09:00");
            rs = tr.Allocate();
            Assert.Equal(InspectorState.Blocked, rs);
        }

        
        // [Fact]
        // public void TimeDepTest()
        // {
        //     IScheduler sched;
        //     IDependency dep;
        //     IDependContext ctx;
        //     TestTimeResourceContext trx;
        //     
        //     // Dependency Test ---------------------
        //
        //     sched = new Scheduler();
        //     dep = new Dependency();
        //     ctx = new TestDependContext();
        //     trx = TestTimeResourceContext.Create();
        //
        //     Assert.Equal(false, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.Blocked, dep.State);
        //
        //     // Closed Time Range -------------------------
        //
        //     dep = new TimeDependency(
        //         new TimeResource(
        //             new TimeSpan(10, 0, 0),
        //             new TimeSpan(10, 0, 0),
        //             trx
        //         )
        //     );
        //
        //     // To Early
        //     trx.Set("00:00");
        //     
        //     Assert.Equal(false, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.Blocked, dep.State);
        //
        //     // To Late
        //     trx.Set("11:00");
        //     Assert.Equal(false, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.Blocked, dep.State);
        //
        //     // On Time
        //     trx.Set("10:00");
        //     Assert.Equal(true, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.ReleaseThis, dep.State);
        //
        //     // Back to Blocked, so updated
        //     trx.Set("10:00");
        //     Assert.Equal(true, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.Blocked, dep.State);

        //     // After Time ------------------------------
        //
        //     // To Early
        //     dep = new TimeDependency(new TimeSpan(10, 0, 0));
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
        //     Assert.Equal(false, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.Blocked, dep.State);
        //
        //     // On Time
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 10, 0, 0)};
        //     Assert.Equal(true, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.ReleaseThis, dep.State);
        //
        //     // After Time
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 0, 0)};
        //     Assert.Equal(false, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.ReleaseThis, dep.State);
        //
        //     // Back to Blocked, so updated
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
        //     Assert.Equal(true, dep.Refresh(sched, ctx));
        //     Assert.Equal(DependencyState.Blocked, dep.State);
        // }
        //
        // [Fact]
        // public void TimeDepStepDepSchedulerTest()
        // {
        //     IScheduler sched;
        //     IDependContext ctx;
        //     Step stepA1;
        //     Step stepA2;
        //
        //     sched = new Scheduler();
        //
        //     stepA1 = new Step("A1");
        //     sched.AddStep(stepA1);
        //     stepA1.AddDependency(new TimeDependency(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)));
        //     stepA2 = new Step("A2");
        //     sched.AddStep(stepA2);
        //     stepA2.AddDependency(new StepDependency("A1"));
        //
        //     Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.NotSub, sched.GetStep("A2").State);
        //
        //     // Too early nothing Happens
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
        //     Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        //
        //     // On Time, A1 Released
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
        //     Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
        //     Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        //
        //     // Time Passes, still on time
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 1, 0)};
        //     Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
        //     Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        //
        //     // Time Over, Step is Unqueued
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 0, 0)};
        //     Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        //
        //     // Switch off AllowQueueRevoke
        //     stepA1.AllowQueueRevoke = false;
        //
        //     // On Time, A1 Released again
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
        //     Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
        //     Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        //
        //     // Time Passes, still on time
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 1, 0)};
        //     Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
        //     Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        //
        //     // Time Over, Step is keep Released
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 0, 0)};
        //     Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
        //     Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        //
        //     // Step A1 Complete, make room for releasing A2
        //     stepA1.State = StepState.Success;
        //
        //     // Time Over, Step is keep Released
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 1, 0)};
        //     Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
        //     Assert.Equal(StepState.Success, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.Queued, sched.GetStep("A2").State);
        //
        //     // Complete A2
        //     stepA2.State = StepState.Error;
        //     ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 2, 0)};
        //     Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
        //     Assert.Equal(SchedulerState.Complete, sched.State);
        //     Assert.Equal(StepState.Success, sched.GetStep("A1").State);
        //     Assert.Equal(StepState.Error, sched.GetStep("A2").State);

    }
}