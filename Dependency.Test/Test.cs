using System;
using Xunit;

namespace Dependency.Test
{
    public class Test
    {
        [Fact]
        public void StepDepTest()
        {
            IScheduler sched;
            IDependency dep;
            IDependContext ctx;

            sched = new Scheduler();
            ctx = new TestDependContext();
            sched.AddStep(new Step("INIT"));

            dep = new StepDependency("INIT");
            Assert.Equal(false, dep.Refresh(sched, ctx));
            Assert.Equal(DependencyState.Blocked, dep.State);
        }

        [Fact]
        public void StepDepSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;

            // Add Step, Get it Back, Has default state
            sched = new Scheduler();
            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);

            sched = new Scheduler();
            ctx = new TestDependContext();
            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            stepA2 = new Step("A2");
            sched.AddStep(stepA2);
            stepA2.AddDependency(new StepDependency("A1"));
            Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A2").State);

            // Nothing to hold A1, it gets Queued
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);

            // Nothing new happend asatus as before
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);

            // A1 in Success, then A2 Released
            stepA1.State = StepState.Success;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Success, sched.GetStep("A1").State);
            Assert.Equal(StepState.Queued, sched.GetStep("A2").State);
        }

        [Fact]
        public void ReverseStepDepSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;
            Step stepA3;

            // Step Dependency in reversed order

            sched = new Scheduler();
            ctx = new TestDependContext();
            stepA1 = new Step("A1"); sched.AddStep(stepA1);
            stepA2 = new Step("A2"); sched.AddStep(stepA2);
            stepA3 = new Step("A3"); sched.AddStep(stepA3);

            stepA1.AddDependency(new StepDependency("A2"));
            stepA2.AddDependency(new StepDependency("A3"));

            Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A2").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A3").State);

            // Nothing to hold A3, it gets Queued
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.Equal(StepState.Queued, sched.GetStep("A3").State);

            // Nothing new happened status as before
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.Equal(StepState.Queued, sched.GetStep("A3").State);

            // A3 in Success, then A2 Released
            stepA3.State = StepState.Success;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.Equal(StepState.Queued, sched.GetStep("A2").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);

            // Nothing new happened status as before
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.Equal(StepState.Queued, sched.GetStep("A2").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);

            // A2 in Success, then A1 Released
            stepA2.State = StepState.Success;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.Success, sched.GetStep("A2").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);

            // Nothing new happened status as before
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.Success, sched.GetStep("A2").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);

            // A1 in Success, Nothing more to do
            stepA1.State = StepState.Success;
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Success, sched.GetStep("A1").State);
            Assert.Equal(StepState.Success, sched.GetStep("A2").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);

            // Nothing new happened status as before
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Success, sched.GetStep("A1").State);
            Assert.Equal(StepState.Success, sched.GetStep("A2").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);
        }

        public void TimeThenStepDepSchedulerTest()
        {
        
            var sched = new Scheduler();
            var ctx = new TestDependContext();
            var trx = TestTimeInspectorContext.Create();

            var stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            var stepA2 = new Step("A2");
            sched.AddStep(stepA2);
        
            stepA1.AddDependency(new InspectorDependency(new TimeInspector(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), trx)));
            stepA2.AddDependency(new StepDependency("A1"));
        
            Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A2").State);
        
            // A1 waiting for time
            trx.Set("00:00");
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        
            // Time is Good, but A1 still only queued
            trx.Set("09:00");
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        
            // Time Passes, still on time
            trx.Set("09:01");
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        }
        
        [Fact]
        public void TimeAndStepDepSchedulerTest()
        {
        
            var sched = new Scheduler();
            var ctx = new TestDependContext();
            var trx = TestTimeInspectorContext.Create();
        
            var stepA1 = new Step("A1"); sched.AddStep(stepA1);
            var stepA2 = new Step("A2"); sched.AddStep(stepA2);
            var stepA3 = new Step("A3"); sched.AddStep(stepA3);
        
            stepA2.AddDependency(new StepDependency("A1"));
            stepA3.AddDependency(new StepDependency("A2"));
            stepA3.AddDependency(new InspectorDependency( new TimeInspector(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), trx)));
        
            Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A2").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A3").State);
        
            // A1 can queue no restrictions
            // NOT IMPLEMENTED NewRefresh = DateTime.MaxValue};
            trx.Set("00:00");
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            // NOT IMPLEMENTED Assert.Equal(new DateTime(1, 1, 1, 8, 0, 0), ctx.NewRefresh);
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A3").State);
        
            // Move Time In window, only one dependeny release son no changes
            // NOT IMPLEMENTED NewRefresh = DateTime.MaxValue};
            trx.Set("09:00");
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            // NOT IMPLEMENTED Assert.Equal(new DateTime(1, 1, 1, 10, 0, 0), ctx.NewRefresh);
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A3").State);
        }

        [Fact]
        public void ReleaseAllDepSchedulerTest()
        {
            var sched = new Scheduler();
            var ctx = new TestDependContext();
            var trx = TestTimeInspectorContext.Create();
        
            var stepA1 = new Step("A1"); sched.AddStep(stepA1);
            var stepA2 = new Step("A2"); sched.AddStep(stepA2);
            var stepA3 = new Step("A3"); sched.AddStep(stepA3);
        
            stepA2.AddDependency(new StepDependency("A1")); 
            stepA3.AddDependency(new StepDependency("A2"));
            stepA3.AddDependency(new InspectorDependency( new TimeInspector(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), trx), DependencyAction.ReleaseAll)); 
        
            Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A2").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A3").State);
        
            // A1 can queue no restrictions
            // NOT IMPLEMENTED NewRefresh = DateTime.MaxValue
            trx.Set("00:00");
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            // NOT IMPLEMENTED Assert.Equal(new DateTime(1, 1, 1, 8, 0, 0), ctx.NewRefresh);
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A3").State);
        
            // Move Time In window, force releases A3
            // NOT IMPLEMENTED ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0), NewRefresh = DateTime.MaxValue};
            trx.Set("09:00");
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            // NOT IMPLEMENTED Assert.Equal(new DateTime(1, 1, 1, 10, 0, 0), ctx.NewRefresh);
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.Equal(StepState.Queued, sched.GetStep("A3").State);
        
            // Move Time outside window, Take back Releasing, refresh on next day
            // NOT IMPLEMENTED ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 0, 0), NewRefresh = DateTime.MaxValue};
            trx.Set("11:00");
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            // NOT IMPLEMENTED Assert.Equal(new DateTime(1, 1, 2, 8, 0, 0), ctx.NewRefresh);
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A3").State);
        
            // Step A1 Success, release A2, still next day
            // NOT IMPLEMENTED ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 2, 0), NewRefresh = DateTime.MaxValue};
            trx.Set("11:02");
            stepA1.State = StepState.Success;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            // NOT IMPLEMENTED Assert.Equal(new DateTime(1, 1, 2, 8, 0, 0), ctx.NewRefresh);
            Assert.Equal(StepState.Success, sched.GetStep("A1").State);
            Assert.Equal(StepState.Queued, sched.GetStep("A2").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A3").State);
        
            // Step A2 Success, But release A3, still next day
            // NOT IMPLEMENTED ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 4, 0), NewRefresh = DateTime.MaxValue};
            trx.Set("11:04");
            stepA2.State = StepState.Success;
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            // NOT IMPLEMENTED Assert.Equal(new DateTime(1, 1, 2, 8, 0, 0), ctx.NewRefresh);
            Assert.Equal(StepState.Success, sched.GetStep("A1").State);
            Assert.Equal(StepState.Success, sched.GetStep("A2").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A3").State);
        }
        
        [Fact]
        public void SkipDepSchedulerTest()
        {
            var sched = new Scheduler();
            var ctx = new TestDependContext();
            var trx = TestTimeInspectorContext.Create();
            
            var stepA1 = new Step("A1"); sched.AddStep(stepA1);
            var stepA2 = new Step("A2"); sched.AddStep(stepA2);
        
            stepA2.AddDependency(new StepDependency("A1", DependencyAction.StepSkip));
            stepA2.AddDependency(new InspectorDependency(new TimeInspector(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), trx), DependencyAction.ReleaseAll));
        
            Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A2").State);
        
            // A1 can queue no restrictions
            trx.Set("00:00");
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        
            // Time Passes, no changes
            trx.Set("01:00");
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        
            // A1 Now Running, no changes
            trx.Set("01:01");
            stepA1.State = StepState.Running;
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(StepState.Running, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
        
            // A1 Now Success, Skip A2
            trx.Set("01:01");
            stepA1.State = StepState.Success;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Success, sched.GetStep("A1").State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A2").State);
        
            // Complete
            trx.Set("01:01");
            Assert.Equal(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Success, sched.GetStep("A1").State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A2").State);
        }

        [Fact]
        public void AllCompleteStateschedulerTest()
        {
            // Error -> ALl to Skipped by Default -------------

            var sched = new Scheduler();
            var ctx = new TestDependContext();
            
            var stepA1 = new Step("A1"); sched.AddStep(stepA1);
            var stepA2 = new Step("A2"); sched.AddStep(stepA2);
            var stepA3 = new Step("A3"); sched.AddStep(stepA3);

            stepA2.AddDependency(new StepDependency("A1"));
            stepA3.AddDependency(new StepDependency("A2"));

            Assert.Equal(SchedulerState.NotSub, sched.State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A1").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A2").State);
            Assert.Equal(StepState.NotSub, sched.GetStep("A3").State);

            // A1 can queue no restrictions
            
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Active, sched.State);
            Assert.Equal(StepState.Queued, sched.GetStep("A1").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.Equal(StepState.WaitDep, sched.GetStep("A3").State);

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Error;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Error, sched.GetStep("A1").State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A2").State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A3").State);

            // Error -> All forced to Error -------------

            sched = new Scheduler();

            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            stepA2 = new Step("A2");
            sched.AddStep(stepA2);
            stepA3 = new Step("A3");
            sched.AddStep(stepA3);

            stepA2.AddDependency(new StepDependency("A1", DependencyAction.ReleaseDep, DependencyAction.StepError));
            stepA3.AddDependency(new StepDependency("A2", DependencyAction.ReleaseDep, DependencyAction.StepError));

            // A1 can queue no restrictions
            ctx = new TestDependContext();
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Error;
            ctx = new TestDependContext();
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Error, sched.GetStep("A1").State);
            Assert.Equal(StepState.Error, sched.GetStep("A2").State);
            Assert.Equal(StepState.Error, sched.GetStep("A3").State);

            // Skip -> All to Skipped -------------

            sched = new Scheduler();

            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            stepA2 = new Step("A2");
            sched.AddStep(stepA2);
            stepA3 = new Step("A3");
            sched.AddStep(stepA3);

            stepA2.AddDependency(new StepDependency("A1"));
            stepA3.AddDependency(new StepDependency("A2"));

            // A1 can queue no restrictions
            ctx = new TestDependContext();
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Skipped;
            ctx = new TestDependContext();
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A1").State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A2").State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A3").State);

            // Timeout -> All to Skipped -------------

            sched = new Scheduler();

            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            stepA2 = new Step("A2");
            sched.AddStep(stepA2);
            stepA3 = new Step("A3");
            sched.AddStep(stepA3);

            stepA2.AddDependency(new StepDependency("A1"));
            stepA3.AddDependency(new StepDependency("A2"));

            // A1 can queue no restrictions
            ctx = new TestDependContext();
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Timeout;
            ctx = new TestDependContext();
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Timeout, sched.GetStep("A1").State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A2").State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A3").State);

            // Success -> Force all to Skipped -------------

            sched = new Scheduler();

            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            stepA2 = new Step("A2");
            sched.AddStep(stepA2);
            stepA3 = new Step("A3");
            sched.AddStep(stepA3);

            stepA2.AddDependency(new StepDependency("A1", DependencyAction.StepSuccess));
            stepA3.AddDependency(new StepDependency("A2", DependencyAction.StepSuccess));

            // A1 can queue no restrictions
            ctx = new TestDependContext();
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Success;
            ctx = new TestDependContext();
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Success, sched.GetStep("A1").State);
            Assert.Equal(StepState.Success, sched.GetStep("A2").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);
        }

        [Fact]
        public void CompletePriorityStateschedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;
            Step stepA3;

            // Setup ---------

            sched = new Scheduler();

            stepA1 = new Step("A1"); sched.AddStep(stepA1);
            stepA2 = new Step("A2"); sched.AddStep(stepA2);
            stepA3 = new Step("A3"); sched.AddStep(stepA3);

            stepA3.AddDependency(
                new StepDependency("A1",
                    DependencyAction.StepSuccess,
                    DependencyAction.StepError,
                    DependencyAction.StepSkip,
                    DependencyAction.StepSkip
                )
            );
            stepA3.AddDependency(
                new StepDependency("A2",
                    DependencyAction.StepSuccess,
                    DependencyAction.StepError,
                    DependencyAction.StepSkip,
                    DependencyAction.StepSkip
                )
            );

            ctx = new TestDependContext();

            // Error wins over success ------

            stepA1.State = StepState.Success;
            stepA2.State = StepState.Error;
            stepA3.State = StepState.NotSub;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Error, sched.GetStep("A3").State);

            // Error wins over success ------ Reverse Order

            stepA1.State = StepState.Error;
            stepA2.State = StepState.Success;
            stepA3.State = StepState.NotSub;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Error, sched.GetStep("A3").State);

            // Skip wins over success ------

            stepA1.State = StepState.Success;
            stepA2.State = StepState.Skipped;
            stepA3.State = StepState.NotSub;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A3").State);

            // Skip wins over success ------ Reverse Order

            stepA1.State = StepState.Skipped;
            stepA2.State = StepState.Success;
            stepA3.State = StepState.NotSub;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A3").State);

            // Skip wins over Error ------

            stepA1.State = StepState.Error;
            stepA2.State = StepState.Skipped;
            stepA3.State = StepState.NotSub;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A3").State);

            // Skip wins over Error ------ Reverse Order

            stepA1.State = StepState.Skipped;
            stepA2.State = StepState.Error;
            stepA3.State = StepState.NotSub;
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Skipped, sched.GetStep("A3").State);
        }

        [Fact]
        public void OrderSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;


            // Setup ---------

            sched = new Scheduler()
                .AddStep(
                    new Step("A1")
                )
                .AddStep(
                    new Step("A2")
                    .AddDependency(new StepDependency("A3", DependencyAction.StepSuccess))
                )
                .AddStep(
                    new Step("A3")
                    .AddDependency(new StepDependency("A1", DependencyAction.StepSuccess))
                );

            ctx = new TestDependContext();

            // Error wins over success ------

            sched.GetStep("A1").State = StepState.Success;
            sched.GetStep("A2").State = StepState.NotSub;
            sched.GetStep("A3").State = StepState.NotSub;
            
            Assert.Equal(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.Equal(SchedulerState.Complete, sched.State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);
            Assert.Equal(StepState.Success, sched.GetStep("A3").State);
        }

        [Fact]
        public void CycleCheckSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;
            Step stepA3;

            // Setup ---------

            sched = new Scheduler();

            stepA1 = new Step("A1"); sched.AddStep(stepA1);
            stepA2 = new Step("A2"); sched.AddStep(stepA2);
            stepA3 = new Step("A3"); sched.AddStep(stepA3);

            stepA1.AddDependency(new StepDependency("A3", DependencyAction.StepSuccess));
            stepA2.AddDependency(new StepDependency("A1", DependencyAction.StepSuccess));
            stepA3.AddDependency(new StepDependency("A2", DependencyAction.StepSuccess));

            ctx = new TestDependContext();
            Assert.Throws<Exception>(() => sched.RefreshDependency(ctx));
        }

        [Fact]
        public void MissingDepCheckSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;

            // Setup ---------

            sched = new Scheduler();

            stepA1 = new Step("A1"); sched.AddStep(stepA1);
            stepA1.AddDependency(new StepDependency("MISSING", DependencyAction.StepSuccess));

            ctx = new TestDependContext();
            Assert.Throws<Exception>(() => sched.RefreshDependency(ctx));
        }
    }
}
