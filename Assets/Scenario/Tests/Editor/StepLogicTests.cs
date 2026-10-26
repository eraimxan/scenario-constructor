using NUnit.Framework;
using static ScenarioKit.Tests.ScenarioFixture;

namespace ScenarioKit.Tests
{
    public class StepLogicTests
    {
        ScenarioFixture f;

        [SetUp] public void SetUp() => f = new ScenarioFixture();
        [TearDown] public void TearDown() => f.Dispose();

        ScenarioSession StartRoute()
        {
            var route = f.Step("route", f.Key("zone.a"), f.Key("zone.b"));
            return f.Start(f.Mode(f.Training(), Group("g", Together(route))));
        }

        [Test]
        public void Sequence_CompletesOnlyInOrder()
        {
            var session = StartRoute();

            f.Do(session, "zone.a");
            Assert.That(session.Step("route").Status, Is.EqualTo(StepStatus.Active));
            f.Do(session, "zone.b");

            Assert.That(session.Step("route").Status, Is.EqualTo(StepStatus.Completed));
        }

        [Test]
        public void Sequence_WrongOrderIsViolationAndRestartsTheSequence()
        {
            var session = StartRoute();

            f.Do(session, "zone.b");
            Assert.That(session.Step("route").Violations, Is.EqualTo(1));

            f.Do(session, "zone.a");
            f.Do(session, "zone.a"); // repeating a done part is harmless
            f.Do(session, "zone.b");
            Assert.That(session.Step("route").Status, Is.EqualTo(StepStatus.Completed));
            Assert.That(session.Step("route").Violations, Is.EqualTo(1));
        }

        [Test]
        public void StateRequirement_ViolatedWhenOtherObjectIsInWrongState()
        {
            var power = f.Create<StateKey>("power");
            var start = f.Step("start");
            start.AddRequirement(power, true);
            var session = f.Start(f.Mode(f.Training(), Group("g", Together(start))));

            f.Do(session, "do.start");
            Assert.That(session.Step("start").Violations, Is.EqualTo(1));
            Assert.That(session.Step("start").Status, Is.EqualTo(StepStatus.Active));

            f.World.Set(power, true);
            f.Do(session, "do.start");
            Assert.That(session.Step("start").Status, Is.EqualTo(StepStatus.Completed));
        }

        [Test]
        public void UnrelatedAction_IsIgnored()
        {
            var session = StartRoute();

            f.Do(session, "something.else");

            Assert.That(session.Step("route").Violations, Is.Zero);
        }
    }
}
