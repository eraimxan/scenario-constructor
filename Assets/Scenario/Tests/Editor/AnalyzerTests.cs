using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static ScenarioKit.Tests.ScenarioFixture;

namespace ScenarioKit.Tests
{
    public class AnalyzerTests
    {
        ScenarioFixture f;

        [SetUp] public void SetUp() => f = new ScenarioFixture();
        [TearDown] public void TearDown() => f.Dispose();

        [Test]
        public void Training_FirstSecondThirdAttempt_Gives15()
        {
            var s = f.Steps(3);
            var session = f.Start(f.Mode(f.Training(), Group("g", Together(s))));
            f.Do(session, "do.S01");
            f.Do(session, "wrong.S02");
            f.Do(session, "do.S02");
            f.Do(session, "wrong.S03");
            f.Do(session, "wrong.S03");
            f.Do(session, "do.S03");

            var report = f.Create<TrainingScoreAnalyzer>("training").Analyze(session.Journal.Records);

            Assert.That(report.Score, Is.EqualTo(15));
            Assert.That(report.MaxScore, Is.EqualTo(30));
            Assert.That(report.Warnings, Is.Empty);
        }

        [Test]
        public void Exam_TwoFailedTwoSkippedInGroupOfFour_Gives70()
        {
            var s = f.Steps(10);
            var session = f.Start(f.Mode(f.Exam(2),
                Group("G1", Together(s[0], s[1], s[2], s[3])),
                Group("G2", InOrder(s[4], s[5], s[6])),
                Group("G3", Together(s[7], s[8], s[9]))));
            f.Do(session, "wrong.S02");
            f.Do(session, "wrong.S04");
            for (int i = 5; i <= 10; i++)
                f.Do(session, $"do.S{i:00}");

            Assert.That(session.IsFinished, Is.True);
            var report = f.Create<ExamPenaltyAnalyzer>("exam").Analyze(session.Journal.Records);

            Assert.That(report.Score, Is.EqualTo(70));
            Assert.That(report.Lines, Has.Some.Contains("штраф 30"));
            Assert.That(report.Warnings, Is.Empty);
        }

        [Test]
        public void AnyAnalyzer_ScoresJournalOfAnyMode()
        {
            var s = f.Steps(2);
            var session = f.Start(f.Mode(f.Training(), Group("g", Together(s))));
            f.Do(session, "wrong.S01");
            f.Do(session, "do.S01");
            f.Do(session, "do.S02");

            var report = f.Create<ExamPenaltyAnalyzer>("exam").Analyze(session.Journal.Records);

            Assert.That(report.Score, Is.EqualTo(100));
            Assert.That(report.Warnings, Is.Empty);
        }

        [Test]
        public void IncompleteJournal_GivesWarningsInsteadOfFailure()
        {
            var records = new List<LogRecord>
            {
                new LogRecord(0, RecordTypes.StepResult, "S01", "Completed"),
                new LogRecord(1, "custom.type", "S02", "whatever").With("extra", 1),
                new LogRecord(2, RecordTypes.StepResult, "S03", "NotAStatus"),
            };

            var report = f.Create<TrainingScoreAnalyzer>("training").Analyze(records);

            Assert.That(report.Score, Is.EqualTo(10));
            Assert.That(report.Warnings, Has.Some.Contains("session.start"));
            Assert.That(report.Warnings, Has.Some.Contains("session.end"));
            Assert.That(report.Warnings, Has.Some.Contains("NotAStatus"));
        }

        [Test]
        public void EmptyJournal_GivesWarning()
        {
            var report = f.Create<ExamPenaltyAnalyzer>("exam").Analyze(new List<LogRecord>());

            Assert.That(report.Warnings, Is.Not.Empty);
        }

        [Test]
        public void Record_SurvivesJsonAndIgnoresUnknownFields()
        {
            var json = JsonUtility.ToJson(new LogRecord(1.5f, RecordTypes.StepViolation, "S01", null, "причина").With("outOfTurn", true));
            var extended = json.Insert(1, "\"newField\":42,");

            var record = JsonUtility.FromJson<LogRecord>(extended);

            Assert.That(record.type, Is.EqualTo(RecordTypes.StepViolation));
            Assert.That(record.message, Is.EqualTo("причина"));
            Assert.That(record.Get("outOfTurn"), Is.EqualTo("True"));
        }
    }
}
