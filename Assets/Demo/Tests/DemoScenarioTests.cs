using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ScenarioKit.Demo.Tests
{
    /// <summary>End-to-end checks of the control cases on the real demo scene and its wiring.</summary>
    public class DemoScenarioTests
    {
        DemoSceneDriver demo;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            demo = new DemoSceneDriver();
            yield return demo.Load();
        }

        ModeProfile Mode(string assetName) => demo.Runner.Scenario.Modes.Single(m => m.name == assetName);

        [UnityTest]
        public IEnumerator Exam_TwoViolationsInFirstGroup_Gives70()
        {
            var exam = Mode("Mode_Exam");
            demo.Click(exam.DisplayName);
            Assert.That(demo.Session.Mode, Is.EqualTo(exam));

            // Group 1: two violations -> two Failed, the other two Skipped.
            demo.Click("Пуск");               // forbidden for S02 before power is on
            yield return demo.EnterDanger();  // forbidden for S03
            yield return demo.GoToNeutral();
            Assert.That(demo.Step("S02").Status, Is.EqualTo(StepStatus.Failed));
            Assert.That(demo.Step("S03").Status, Is.EqualTo(StepStatus.Failed));
            Assert.That(demo.Step("S01").Status, Is.EqualTo(StepStatus.Skipped));
            Assert.That(demo.Step("S04").Status, Is.EqualTo(StepStatus.Skipped));

            // Group 2 one by one, group 3 in two pairs, all clean.
            yield return demo.EnterZoneB();
            yield return demo.EnterZoneC();
            demo.Click("Рубильник");
            demo.Click("Пуск");
            yield return demo.PushCrateToDock();
            demo.Click("Отчёт");
            demo.Click("Рубильник");
            yield return demo.EnterBase();
            demo.Click("Подтвердить");
            yield return demo.TouchTerminal();

            Assert.That(demo.Session.IsFinished, Is.True, string.Join("\n", demo.Session.Journal.Records));
            var report = demo.Analyze(exam.DefaultAnalyzer);
            Assert.That(report.Score, Is.EqualTo(70), report.ToText());
            Assert.That(report.Warnings, Is.Empty);
        }

        [UnityTest]
        public IEnumerator Training_FirstSecondThirdAttempts_Give10_5_0()
        {
            var training = Mode("Mode_Training");
            demo.Click(training.DisplayName);

            demo.Click("Проверка связи");      // S01: first attempt
            yield return demo.EnterDanger();   // S03: violation, retry
            Assert.That(demo.Step("S03").Status, Is.EqualTo(StepStatus.Active), "training keeps the step active");
            yield return demo.EnterZoneA();    // S03: second attempt
            demo.Click("Пуск");                // S02: violation
            demo.Click("Пуск");                // S02: violation
            demo.Click("Рубильник");           // S02: third attempt
            yield return demo.TouchPillar();   // S04

            Assert.That(demo.Session.Steps.Take(4).Select(s => s.Status), Is.All.EqualTo(StepStatus.Completed));
            var analyzer = training.DefaultAnalyzer;
            var lines = demo.Analyze(analyzer).Lines;
            Assert.That(lines.Single(l => l.StartsWith("Проверка связи")), Does.EndWith("— 10"));
            Assert.That(lines.Single(l => l.StartsWith("Осмотр зоны A")), Does.EndWith("— 5"));
            Assert.That(lines.Single(l => l.StartsWith("Включить питание")), Does.EndWith("— 0"));

            yield return demo.EnterZoneB();
            yield return demo.EnterZoneC();
            demo.Click("Пуск");
            yield return demo.PushCrateToDock();
            demo.Click("Отчёт");
            demo.Click("Рубильник");
            yield return demo.EnterBase();
            demo.Click("Подтвердить");
            yield return demo.TouchTerminal();

            Assert.That(demo.Session.IsFinished, Is.True, string.Join("\n", demo.Session.Journal.Records));
            var report = demo.Analyze(analyzer);
            Assert.That(report.Score, Is.EqualTo(10 + 5 + 0 + 9 * 10));
            Assert.That(report.MaxScore, Is.EqualTo(120));

            // The same journal scored by another analyzer without replaying.
            Assert.That(demo.Analyze(Mode("Mode_Exam").DefaultAnalyzer).Score, Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator StepByStep_StepsIgnoreActionsUntilTheirTurn()
        {
            demo.Click(Mode("Mode_TrainingStepByStep").DisplayName);

            demo.Click("Пуск");
            yield return demo.EnterZoneB();

            Assert.That(demo.Session.Journal.Records.Any(r => r.type == RecordTypes.StepViolation), Is.False);
            Assert.That(demo.Session.Steps.Count(s => s.Status == StepStatus.Active), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ContextualStep_StartWithoutPower_IsViolation()
        {
            demo.Click(Mode("Mode_Training").DisplayName);
            demo.Click("Проверка связи");
            demo.Click("Рубильник");
            yield return demo.EnterZoneA();
            yield return demo.TouchPillar();
            yield return demo.EnterZoneB();
            yield return demo.EnterZoneC();
            demo.Click("Рубильник");           // power off again
            demo.Click("Пуск");

            Assert.That(demo.Step("S06").Status, Is.EqualTo(StepStatus.Active));
            Assert.That(demo.Step("S06").Violations, Is.EqualTo(1));
            Assert.That(demo.Session.Journal.Records.Last(r => r.type == RecordTypes.StepViolation).message,
                Does.Contain("без питания"));
        }

        [UnityTest]
        public IEnumerator Replay_ReloadsSceneAndStartsSameModeAgain()
        {
            var exam = Mode("Mode_Exam");
            demo.Click(exam.DisplayName);
            demo.Click("Пуск");

            demo.Runner.Play(exam);
            yield return null;
            yield return null;
            demo.Bind();

            Assert.That(demo.Session, Is.Not.Null);
            Assert.That(demo.Session.Mode, Is.EqualTo(exam));
            Assert.That(demo.Session.Journal.Records.Any(r => r.type == RecordTypes.StepViolation), Is.False);
        }
    }
}
