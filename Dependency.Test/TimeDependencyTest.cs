using System;
using NUnit.Framework;

namespace Dependency.Test
{
    [TestFixture]
    public class TimeDependencyTest
    {
        [Test]
        public void TimeDepTest()
        {
            IScheduler sched;
            IDependency dep;
            IDependContext ctx;

            // Dependency Test ---------------------

            sched = new Scheduler();
            dep = new Dependency();
            ctx = new TestDependContext();

            Assert.AreEqual(false, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.Blocked, dep.State);

            // Closed Time Range -------------------------

            // To Early
            dep = new TimeDependency(new TimeSpan(10, 0, 0), new TimeSpan(10, 0, 0));
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
            Assert.AreEqual(false, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.Blocked, dep.State);

            // To Late
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 0, 0)};
            Assert.AreEqual(false, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.Blocked, dep.State);

            // On Time
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 10, 0, 0)};
            Assert.AreEqual(true, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.ReleaseThis, dep.State);

            // Back to Blocked, so updated
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 0, 0)};
            Assert.AreEqual(true, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.Blocked, dep.State);

            // After Time ------------------------------

            // To Early
            dep = new TimeDependency(new TimeSpan(10, 0, 0));
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
            Assert.AreEqual(false, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.Blocked, dep.State);

            // On Time
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 10, 0, 0)};
            Assert.AreEqual(true, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.ReleaseThis, dep.State);

            // After Time
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 0, 0)};
            Assert.AreEqual(false, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.ReleaseThis, dep.State);

            // Back to Blocked, so updated
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
            Assert.AreEqual(true, dep.Refresh(sched, ctx));
            Assert.AreEqual(DependencyState.Blocked, dep.State);
        }

        [Test]
        public void TimeDepStepDepSchedulerTest()
        {
            IScheduler sched;
            IDependContext ctx;
            Step stepA1;
            Step stepA2;

            sched = new Scheduler();

            stepA1 = new Step("A1");
            sched.AddStep(stepA1);
            stepA1.AddDependency(new TimeDependency(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)));
            stepA2 = new Step("A2");
            sched.AddStep(stepA2);
            stepA2.AddDependency(new StepDependency("A1"));

            Assert.AreEqual(StepState.NotSub, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.NotSub, sched.GetStep("A2").State);

            // Too early nothing Happens
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // On Time, A1 Released
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Time Passes, still on time
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 1, 0)};
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Time Over, Step is Unqueued
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 0, 0)};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Switch off AllowQueueRevoke
            stepA1.AllowQueueRevoke = false;

            // On Time, A1 Released again
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Time Passes, still on time
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 1, 0)};
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Time Over, Step is keep Released
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 0, 0)};
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Queued, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.WaitDep, sched.GetStep("A2").State);

            // Step A1 Complete, make room for releasing A2
            stepA1.State = StepState.Success;

            // Time Over, Step is keep Released
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 1, 0)};
            Assert.AreEqual(RefreshState.Updated, sched.RefreshDependency(ctx));
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Queued, sched.GetStep("A2").State);

            // Complete A2
            stepA2.State = StepState.Error;
            ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 2, 0)};
            Assert.AreEqual(RefreshState.Untouched, sched.RefreshDependency(ctx));
            Assert.AreEqual(SchedulerState.Complete, sched.State);
            Assert.AreEqual(StepState.Success, sched.GetStep("A1").State);
            Assert.AreEqual(StepState.Error, sched.GetStep("A2").State);
        }

    }
}