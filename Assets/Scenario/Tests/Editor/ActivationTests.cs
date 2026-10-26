using NUnit.Framework;
using static ScenarioKit.Tests.ScenarioFixture;

namespace ScenarioKit.Tests
{
    public class ActivationTests
    {
        ScenarioFixture f;

        [SetUp] public void SetUp() => f = new ScenarioFixture();
        [TearDown] public void TearDown() => f.Dispose();

        [Test]
        public void MixedActivation_FollowsThePlanFromTheTask()
        {
            // 1-4 together, then 5, then 6, 7, 8 one by one, then 9+10, then 11+12.
            var s = f.Steps(12);
            var mode = f.Mode(f.Training(), Group("all",
                Together(s[0], s[1], s[2], s[3]),
                InOrder(s[4], s[5], s[6], s[7]),
                Together(s[8], s[9]),
                Together(s[10], s[11])));
            var session = f.Start(mode);

            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S01", "S02", "S03", "S04" }));
            foreach (var id in new[] { "S03", "S01", "S04" })
                f.Do(session, "do." + id);
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S02" }), "the batch waits for its last step");
            f.Do(session, "do.S02");

            foreach (var id in new[] { "S05", "S06", "S07", "S08" })
            {
                Assert.That(session.ActiveIds(), Is.EqualTo(new[] { id }));
                f.Do(session, "do." + id);
            }

            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S09", "S10" }));
            f.Do(session, "do.S10");
            f.Do(session, "do.S09");
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S11", "S12" }));
            f.Do(session, "do.S12");
            f.Do(session, "do.S11");

            Assert.That(session.IsFinished, Is.True);
            Assert.That(session.Steps, Has.All.Matches<StepRun>(r => r.Status == StepStatus.Completed));
        }

        [Test]
        public void OutOfTurnAction_IsViolationOfThePendingStep_ByDefault()
        {
            var s = f.Steps(2);
            var session = f.Start(f.Mode(f.Training(), Group("g", InOrder(s[0], s[1]))));

            f.Do(session, "do.S02");

            Assert.That(session.Step("S02").Status, Is.EqualTo(StepStatus.Pending));
            Assert.That(session.Step("S02").Violations, Is.EqualTo(1));
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S01" }));
        }

        [Test]
        public void OutOfTurnAction_IsIgnored_WhenStepsAreInactiveUntilTheirTurn()
        {
            var s = f.Steps(2);
            var session = f.Start(f.Mode(f.Training(), OutOfTurnPolicy.IgnoreUntilActive, Group("g", InOrder(s[0], s[1]))));

            f.Do(session, "do.S02");
            f.Do(session, "do.S01");

            Assert.That(session.Step("S02").Violations, Is.Zero);
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S02" }));
        }

        [Test]
        public void Training_ViolationKeepsStepActiveAndOtherStepsUntouched()
        {
            var s = f.Steps(2);
            var session = f.Start(f.Mode(f.Training(), Group("g", Together(s[0], s[1]))));

            f.Do(session, "wrong.S01");
            f.Do(session, "wrong.S01");

            Assert.That(session.Step("S01").Status, Is.EqualTo(StepStatus.Active));
            Assert.That(session.Step("S01").Attempt, Is.EqualTo(3));
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S01", "S02" }));
        }

        [Test]
        public void Exam_ThresholdSkipsRestOfGroupAndStartsNextGroup()
        {
            var s = f.Steps(6);
            var session = f.Start(f.Mode(f.Exam(2),
                Group("G1", Together(s[0], s[1], s[2], s[3])),
                Group("G2", Together(s[4], s[5]))));

            f.Do(session, "wrong.S01");
            Assert.That(session.Step("S01").Status, Is.EqualTo(StepStatus.Failed));
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S02", "S03", "S04" }), "one violation is below the threshold");

            f.Do(session, "wrong.S02");

            Assert.That(session.Step("S03").Status, Is.EqualTo(StepStatus.Skipped));
            Assert.That(session.Step("S04").Status, Is.EqualTo(StepStatus.Skipped));
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S05", "S06" }));
        }

        [Test]
        public void Exam_FailedStepDoesNotBlockTheNextStage()
        {
            var s = f.Steps(2);
            var session = f.Start(f.Mode(f.Exam(5), Group("g", InOrder(s[0], s[1]))));

            f.Do(session, "wrong.S01");

            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S02" }));
        }

        [Test]
        public void Groups_StartedWithPreviousGroup_RunInParallel()
        {
            var s = f.Steps(3);
            var second = Group("G2", Together(s[1]));
            second.start = GroupStart.WithPreviousGroup;
            var session = f.Start(f.Mode(f.Exam(), Group("G1", Together(s[0])), second, Group("G3", Together(s[2]))));

            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S01", "S02" }));
            f.Do(session, "do.S01");
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S02" }), "G3 waits for G2");
            f.Do(session, "do.S02");
            Assert.That(session.ActiveIds(), Is.EqualTo(new[] { "S03" }));
        }
    }
}
