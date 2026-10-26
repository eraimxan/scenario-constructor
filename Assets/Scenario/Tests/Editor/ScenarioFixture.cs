using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScenarioKit.Tests
{
    sealed class FakeWorld : IWorldState
    {
        readonly Dictionary<StateKey, bool> values = new Dictionary<StateKey, bool>();

        public bool Get(StateKey key) => values.TryGetValue(key, out var value) && value;

        public void Set(StateKey key, bool value) => values[key] = value;
    }

    /// <summary>Builds throwaway scenario assets in memory and destroys them after the test.</summary>
    sealed class ScenarioFixture : IDisposable
    {
        readonly List<Object> created = new List<Object>();
        readonly Dictionary<string, ActionKey> keys = new Dictionary<string, ActionKey>();

        public FakeWorld World { get; } = new FakeWorld();

        public ActionKey Key(string name)
        {
            if (!keys.TryGetValue(name, out var key))
            {
                key = Create<ActionKey>(name);
                keys.Add(name, key);
            }
            return key;
        }

        /// <summary>A step completed by action "do.{id}" and violated by action "wrong.{id}".</summary>
        public ActionSequenceStep Step(string id) => Step(id, Key("do." + id));

        public ActionSequenceStep Step(string id, params ActionKey[] sequence)
        {
            var step = Create<ActionSequenceStep>(id);
            step.Describe(id, id, "");
            step.Configure(sequence);
            step.AddForbidden(Key("wrong." + id));
            return step;
        }

        public ActionSequenceStep[] Steps(int count) =>
            Enumerable.Range(1, count).Select(i => Step($"S{i:00}")).ToArray();

        public TrainingRules Training() => Create<TrainingRules>("training");

        public ExamRules Exam(int threshold = 2)
        {
            var rules = Create<ExamRules>("exam");
            rules.Configure(threshold);
            return rules;
        }

        public ModeProfile Mode(ExecutionRules rules, params StepGroup[] groups) =>
            Mode(rules, OutOfTurnPolicy.Violation, groups);

        public ModeProfile Mode(ExecutionRules rules, OutOfTurnPolicy policy, params StepGroup[] groups)
        {
            var mode = Create<ModeProfile>("mode");
            mode.Configure("mode", rules, null, policy, groups);
            return mode;
        }

        public static StepGroup Group(string name, params Stage[] stages) =>
            new StepGroup { name = name, stages = stages.ToList() };

        public static Stage Together(params StepDefinition[] steps) =>
            new Stage { activation = StageActivation.Simultaneous, steps = steps.ToList() };

        public static Stage InOrder(params StepDefinition[] steps) =>
            new Stage { activation = StageActivation.Sequential, steps = steps.ToList() };

        public ScenarioSession Start(ModeProfile mode)
        {
            var session = new ScenarioSession(null, mode, World, () => 0f);
            session.Start();
            return session;
        }

        public void Do(ScenarioSession session, string key) => session.HandleAction(new ActionEvent(Key(key)));

        public T Create<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            created.Add(asset);
            return asset;
        }

        public void Dispose()
        {
            foreach (var asset in created)
                Object.DestroyImmediate(asset);
        }
    }

    static class SessionExtensions
    {
        public static string[] ActiveIds(this ScenarioSession session) =>
            session.Steps.Where(s => s.Status == StepStatus.Active).Select(s => s.Definition.Id).OrderBy(id => id).ToArray();

        public static StepRun Step(this ScenarioSession session, string id) =>
            session.Steps.Single(s => s.Definition.Id == id);
    }
}
