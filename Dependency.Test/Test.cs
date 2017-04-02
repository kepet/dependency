using NUnit.Framework;
using System;

namespace Dependency.Test
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void StepDepTest()
        {
            IScheduler sched;
            IDependency dep;
            IDependContext ctx;

            sched = new Scheduler();
            ctx = new TestDependContext();
            sched.AddStep(new Step("INIT"));

            dep = new StepDependency("INIT");
            Assert.AreEqual(false, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.Blocked, dep.State);
        }

        [Test]
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
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);

            sched = new Scheduler();
            ctx = new TestDependContext();
            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            stepA2 = new Step("A2");
            sched.AddStep(stepA2);
            stepA2.AddDependency(new StepDependency("A1"));
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A2").State);

            // Nothing to hold A1, it gets Queued
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Nothing new happend asatus as before
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // A1 in Success, then A2 Released
            stepA1.State = StepState.Success;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A2").State);
        }

        [Test]
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

            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A3").State);

            // Nothing to hold A3, it gets Queued
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A3").State);

            // Nothing new happend status as before
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A3").State);

            // A3 in Success, then A2 Released
            stepA3.State = StepState.Success;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);

            // Nothing new happend status as before
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);

            // A2 in Success, then A1 Released
            stepA2.State = StepState.Success;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);

            // Nothing new happend status as before
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);

            // A1 in Success, Nothing more to do
            stepA1.State = StepState.Success;
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);

            // Nothing new happend status as before
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);
        }

        [Test]
        public void TimeThenStepDepSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;

            sched = new Scheduler();

            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            stepA2 = new Step("A2");
            sched.AddStep(stepA2);

            stepA1.AddDependency(new TimeDependency(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)));
            stepA2.AddDependency(new StepDependency("A1"));

            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A2").State);

            // A1 waiting for time
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Time is Good, but A1 still only queued
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Time Passes, still on time
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 1, 0)};
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
        }

        [Test]
        public void TimeAndStepDepSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;
            Step stepA3;

            sched = new Scheduler();

            stepA1 = new Step("A1"); sched.AddStep(stepA1);
            stepA2 = new Step("A2"); sched.AddStep(stepA2);
            stepA3 = new Step("A3"); sched.AddStep(stepA3);

            stepA2.AddDependency(new StepDependency("A1"));
            stepA3.AddDependency(new StepDependency("A2"));
            stepA3.AddDependency(new TimeDependency(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)));

            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A3").State);

            // A1 can queue no restrictions
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0), NewRefresh = DateTime.MaxValue};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(new DateTime(1, 1, 1, 8, 0, 0), ctx.NewRefresh);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A3").State);

            // Move Time In window, only one dependeny release son no changes
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0), NewRefresh = DateTime.MaxValue};
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(new DateTime(1, 1, 1, 10, 0, 0), ctx.NewRefresh);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A3").State);
        }

        [Test]
        public void ReleaseAllDepSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;
            Step stepA3;

            sched = new Scheduler();

            stepA1 = new Step("A1"); sched.AddStep(stepA1);
            stepA2 = new Step("A2"); sched.AddStep(stepA2);
            stepA3 = new Step("A3"); sched.AddStep(stepA3);

            stepA2.AddDependency(new StepDependency("A1"));
            stepA3.AddDependency(new StepDependency("A2"));
            stepA3.AddDependency(new TimeDependency(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), DependencyAction.ReleaseAll));

            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A3").State);

            // A1 can queue no restrictions
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0), NewRefresh = DateTime.MaxValue};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(new DateTime(1, 1, 1, 8, 0, 0), ctx.NewRefresh);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A3").State);

            // Move Time In window, force releases A3
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0), NewRefresh = DateTime.MaxValue};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(new DateTime(1, 1, 1, 10, 0, 0), ctx.NewRefresh);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A3").State);

            // Move Time outside window, Take back Releasing, refresh on next day
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 0, 0), NewRefresh = DateTime.MaxValue};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(new DateTime(1, 1, 2, 8, 0, 0), ctx.NewRefresh);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A3").State);

            // Step A1 Success, release A2, still next day
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 2, 0), NewRefresh = DateTime.MaxValue};
            stepA1.State = StepState.Success;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(new DateTime(1, 1, 2, 8, 0, 0), ctx.NewRefresh);
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A3").State);

            // Step A2 Success, But release A3, still next day
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 4, 0), NewRefresh = DateTime.MaxValue};
            stepA2.State = StepState.Success;
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(new DateTime(1, 1, 2, 8, 0, 0), ctx.NewRefresh);
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A3").State);
        }

        [Test]
        public void SkipDepSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;

            sched = new Scheduler();

            stepA1 = new Step("A1"); sched.AddStep(stepA1);
            stepA2 = new Step("A2"); sched.AddStep(stepA2);

            stepA2.AddDependency(new StepDependency("A1", DependencyAction.StepSkip));
            stepA2.AddDependency(new TimeDependency(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0),
                DependencyAction.ReleaseAll));

            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A2").State);

            // A1 can queue no restrictions
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Time Passes, no changes
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 1, 0, 0)};
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // A1 Now Running, no changes
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 1, 1, 0)};
            stepA1.State = StepState.Running;
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Running, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // A1 Now Success, Skip A2
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 1, 1, 0)};
            stepA1.State = StepState.Success;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A2").State);

            // Complete
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 1, 1, 0)};
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A2").State);
        }

        [Test]
        public void AllCompleteStateschedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;
            Step stepA3;

            // Error -> ALl to Skipped by Default -------------

            sched = new Scheduler();

            stepA1 = new Step("A1"); sched.AddStep(stepA1);
            stepA2 = new Step("A2"); sched.AddStep(stepA2);
            stepA3 = new Step("A3"); sched.AddStep(stepA3);

            stepA2.AddDependency(new StepDependency("A1"));
            stepA3.AddDependency(new StepDependency("A2"));

            Assert.AreEqual(SchedulerState.NotSub, sched.State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A3").State);

            // A1 can queue no restrictions
            ctx = new TestDependContext();
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Active, sched.State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A3").State);

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Error;
            ctx = new TestDependContext();
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Error, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A3").State);

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
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Error;
            ctx = new TestDependContext();
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Error, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Error, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Error, sched.GetStep("A3").State);

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
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Skipped;
            ctx = new TestDependContext();
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A3").State);

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
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Timeout;
            ctx = new TestDependContext();
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Timeout, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A3").State);

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
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));

            // A1 in Error, by Default triggers all to Error
            stepA1.State = StepState.Success;
            ctx = new TestDependContext();
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A2").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);
        }

        [Test]
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
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Error, sched.GetStep("A3").State);

            // Error wins over success ------ Reverse Order

            stepA1.State = StepState.Error;
            stepA2.State = StepState.Success;
            stepA3.State = StepState.NotSub;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Error, sched.GetStep("A3").State);

            // Skip wins over success ------

            stepA1.State = StepState.Success;
            stepA2.State = StepState.Skipped;
            stepA3.State = StepState.NotSub;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A3").State);

            // Skip wins over success ------ Reverse Order

            stepA1.State = StepState.Skipped;
            stepA2.State = StepState.Success;
            stepA3.State = StepState.NotSub;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A3").State);

            // Skip wins over Error ------

            stepA1.State = StepState.Error;
            stepA2.State = StepState.Skipped;
            stepA3.State = StepState.NotSub;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A3").State);

            // Skip wins over Error ------ Reverse Order

            stepA1.State = StepState.Skipped;
            stepA2.State = StepState.Error;
            stepA3.State = StepState.NotSub;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Skipped, sched.GetStep("A3").State);
        }

        [Test]
        public void OrderSchedulerTest()
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

            stepA3.AddDependency(new StepDependency("A1", DependencyAction.StepSuccess));
            stepA2.AddDependency(new StepDependency("A3", DependencyAction.StepSuccess));

            ctx = new TestDependContext();

            // Error wins over success ------

            stepA1.State = StepState.Success;
            stepA2.State = StepState.NotSub;
            stepA3.State = StepState.NotSub;
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A3").State);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
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
//            Assert.Throws<Exception>(() => sched.RefreshDependency(ctx));
            sched.RefreshDependency(ctx);

            Assert.Fail("Expected Exception before reaching here.");

        }

        [Test]
        [ExpectedException(typeof(Exception))]
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
//            Assert.Throws<Exception>(() => sched.RefreshDependency(ctx));
            sched.RefreshDependency(ctx);

            Assert.Fail("Expected Exception before reaching here.");
        }


    }

}
