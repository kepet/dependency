using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependency;

namespace Dependency.Test
{
	class TestDependContext : IDependContext
	{
		public TestDependContext(/*Scheduler sched*/)
		{
			//Param = new Dictionary<string, string>();
		}

		public DateTime Now { get; set; }
		//public Dictionary<string, string> Param { get; set; }
	}

	[TestClass]
	public class UnitTestDependency
	{
		[TestMethod]
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
			Assert.AreEqual(DependencyState.CanRun, dep.State);

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
			Assert.AreEqual(DependencyState.CanRun, dep.State);

			// After Time
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 11, 0, 0)};
			Assert.AreEqual(false, dep.Refresh(sched, ctx));
			Assert.AreEqual(DependencyState.CanRun, dep.State);

			// Back to Blocked, so updated
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
			Assert.AreEqual(true, dep.Refresh(sched, ctx));
			Assert.AreEqual(DependencyState.Blocked, dep.State);
		}


		[TestMethod]
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

		[TestMethod]
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
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);

			sched = new Scheduler();
			ctx = new TestDependContext();
			stepA1 = new Step("A1");
			sched.AddStep(stepA1);
			stepA2 = new Step("A2");
			sched.AddStep(stepA2);
			stepA2.AddDependency(new StepDependency("A1"));
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Nothing to hold A1, it gets Queued
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Nothing new happend asatus as before 
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// A1 in Success, then A2 Released
			stepA1.State = StepState.StateSuccess;
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A2").State);
		}

		[TestMethod]
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
			stepA1 = new Step("A1");
			sched.AddStep(stepA1);
			stepA2 = new Step("A2");
			sched.AddStep(stepA2);
			stepA3 = new Step("A3");
			sched.AddStep(stepA3);

			stepA1.AddDependency(new StepDependency("A2"));
			stepA2.AddDependency(new StepDependency("A3"));

			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A3").State);

			// Nothing to hold A3, it gets Queued
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A3").State);

			// Nothing new happend status as before 
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A3").State);

			// A3 in Success, then A2 Released
			stepA3.State = StepState.StateSuccess;
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A3").State);

			// Nothing new happend status as before 
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A3").State);

			// A2 in Success, then A1 Released
			stepA2.State = StepState.StateSuccess;
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A3").State);

			// Nothing new happend status as before 
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A3").State);

			// A1 in Success, Nothing more to do
			stepA1.State = StepState.StateSuccess;
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A3").State);

			// Nothing new happend status as before 
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A2").State);
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A3").State);
		}


		[TestMethod]
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

			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Too early nothing Happens
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// On Time, A1 Released
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Time Passes, still on time
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 1, 0)};
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Time Over, Step is Unqueued
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 0, 0)};
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Switch off TimeWindow
			stepA1.TimeWindow = false;

			// On Time, A1 Released again
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 0, 0)};
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Time Passes, still on time
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 9, 1, 0)};
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Time Over, Step is keep Released
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 0, 0)};
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Step A1 Complete, make room for releasing A2
			stepA1.State = StepState.StateSuccess;

			// Time Over, Step is keep Released
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 12, 1, 0)};
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateSuccess, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A2").State);

		}

		[TestMethod]
		public void TimeAndStepDepSchedulerTest()
		{
			IScheduler sched;
			IDependContext ctx;
			Step stepA1;
			Step stepA2;

			sched = new Scheduler();

			stepA1 = new Step("A1"); sched.AddStep(stepA1);
			stepA2 = new Step("A2"); sched.AddStep(stepA2);

			stepA1.AddDependency(new TimeDependency(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)));
			stepA2.AddDependency(new StepDependency("A1"));

			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// A1 waiting for time
			ctx = new TestDependContext() { Now = new DateTime(1, 1, 1, 0, 0, 0) };
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Time is Good, but A1 still only queued
			ctx = new TestDependContext() { Now = new DateTime(1, 1, 1, 9, 0, 0) };
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// Time Passes, still on time
			ctx = new TestDependContext() { Now = new DateTime(1, 1, 1, 9, 1, 0) };
			Assert.AreEqual(false, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

		}

		[TestMethod]
		public void ReleaseAllDepSchedulerTest()
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

			stepA2.AddDependency(new StepDependency("A1"));
			stepA2.AddDependency(new TimeDependency(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), DependencyAction.ReleaseAll));

			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

			// A1 can queue no restrictions
			ctx = new TestDependContext() {Now = new DateTime(1, 1, 1, 0, 0, 0)};
			Assert.AreEqual(true, sched.RefreshDependency(ctx));
			Assert.AreEqual(StepState.StateQueued, sched.GetStep("A1").State);
			Assert.AreEqual(StepState.StateNotSub, sched.GetStep("A2").State);

		}
	}
	}
