using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ScenarioKit
{
    /// <summary>
    /// One pass of a scenario in one mode. Plain C#: receives actions, activates steps by the plan,
    /// applies the mode rules and writes everything to <see cref="Journal"/>.
    /// Knows nothing about scenes, UI, log outputs or scoring.
    /// </summary>
    public sealed class ScenarioSession
    {
        readonly ScenarioDefinition scenario;
        readonly ExecutionRules rules;
        readonly IWorldState world;
        readonly Func<float> clock;
        readonly List<GroupRun> groups = new List<GroupRun>();
        readonly List<StepRun> steps = new List<StepRun>();
        readonly List<string> planWarnings = new List<string>();

        public ModeProfile Mode { get; }
        public ExecutionRules Rules => rules;
        public SessionJournal Journal { get; } = new SessionJournal();
        public IReadOnlyList<GroupRun> Groups => groups;
        public IReadOnlyList<StepRun> Steps => steps;
        public bool IsRunning { get; private set; }
        public bool IsFinished { get; private set; }

        public event Action<SessionMessage> Message;
        public event Action Changed;
        public event Action Finished;

        public ScenarioSession(ScenarioDefinition scenario, ModeProfile mode, IWorldState world, Func<float> clock)
        {
            if (mode == null)
                throw new ArgumentNullException(nameof(mode));
            if (mode.Rules == null)
                throw new ArgumentException($"У режима «{mode.DisplayName}» не заданы правила исполнения.", nameof(mode));

            this.scenario = scenario;
            Mode = mode;
            rules = mode.Rules;
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.clock = clock ?? (() => 0f);
            BuildPlan();
        }

        void BuildPlan()
        {
            var seen = new HashSet<StepDefinition>();
            for (int g = 0; g < Mode.Groups.Count; g++)
            {
                var group = new GroupRun(Mode.Groups[g], g);
                groups.Add(group);

                foreach (var stage in group.Definition.stages)
                {
                    var together = new List<StepRun>();
                    foreach (var definition in stage.steps)
                    {
                        if (definition == null)
                            continue;
                        if (!seen.Add(definition))
                        {
                            planWarnings.Add($"Шаг «{definition.Title}» повторяется в плане — повтор пропущен.");
                            continue;
                        }

                        var step = new StepRun(definition, group);
                        group.steps.Add(step);
                        steps.Add(step);
                        if (stage.activation == StageActivation.Sequential)
                            group.waves.Add(new List<StepRun> { step });
                        else
                            together.Add(step);
                    }
                    if (together.Count > 0)
                        group.waves.Add(together);
                }
            }
        }

        public void Start()
        {
            if (IsRunning || IsFinished)
                throw new InvalidOperationException("Сессию можно запустить только один раз.");
            IsRunning = true;

            Write(RecordTypes.SessionStart, null, null, null,
                ("scenario", scenario != null ? scenario.Title : ""),
                ("mode", Mode.DisplayName),
                ("rules", rules.name),
                ("outOfTurn", Mode.OutOfTurn),
                ("startedAt", DateTime.Now.ToString("s", CultureInfo.InvariantCulture)));
            foreach (var warning in planWarnings)
            {
                Write(RecordTypes.SessionWarning, null, null, warning);
                Notify(MessageKind.Warning, warning);
            }
            foreach (var step in steps)
                Write(RecordTypes.StepRegistered, step, null, null,
                    ("title", step.Definition.Title), ("group", step.Group.Definition.name), ("groupIndex", step.Group.Index));

            StartReadyGroups();
            Changed?.Invoke();
        }

        /// <summary>Stops an unfinished session (e.g. the trainee restarts). Unresolved steps keep no result.</summary>
        public void Abort()
        {
            if (!IsRunning)
                return;
            IsRunning = false;
            IsFinished = true;
            Write(RecordTypes.SessionEnd, null, "Aborted", null);
            Changed?.Invoke();
        }

        public void HandleAction(in ActionEvent action)
        {
            var key = action.Key;
            if (!IsRunning || key == null)
                return;

            Write(RecordTypes.Action, null, null, null,
                ("action", key.DisplayName), ("source", action.Source != null ? action.Source.name : ""));

            bool handled = false;
            foreach (var step in steps.Where(s => s.Status == StepStatus.Active).ToList())
            {
                // An earlier reaction in this loop may have closed the group.
                if (step.Status != StepStatus.Active)
                    continue;

                var response = step.Logic.Handle(action, world);
                switch (response.Kind)
                {
                    case StepResponseKind.Progress:
                        handled = true;
                        Write(RecordTypes.StepProgress, step, null, response.Message, ("progress", step.ProgressHint ?? ""));
                        break;
                    case StepResponseKind.Completed:
                        handled = true;
                        Resolve(step, StepStatus.Completed, null);
                        Notify(MessageKind.Success, $"Выполнено: {step.Definition.Title}");
                        AdvanceWaves(step.Group);
                        break;
                    case StepResponseKind.Violation:
                        handled = true;
                        RegisterViolation(step, response.Message, false);
                        break;
                }
            }

            if (!handled && Mode.OutOfTurn == OutOfTurnPolicy.Violation)
                foreach (var step in steps.Where(s => s.Status == StepStatus.Pending && s.Logic.IsRelevant(key)).ToList())
                    RegisterViolation(step, "действие выполнено вне очереди, шаг ещё не активен", true);

            StartReadyGroups();
            Changed?.Invoke();
        }

        void RegisterViolation(StepRun step, string reason, bool outOfTurn)
        {
            var title = step.Definition.Title;
            step.Violations++;
            step.Group.Violations++;
            Write(RecordTypes.StepViolation, step, null, reason, ("violation", step.Violations), ("outOfTurn", outOfTurn));

            if (rules.OnViolation(step) == ViolationOutcome.FailStep)
            {
                Resolve(step, StepStatus.Failed, reason);
                Notify(MessageKind.Violation, $"«{title}»: {reason}. Шаг не засчитан.");
            }
            else
            {
                step.Logic.Reset();
                Write(RecordTypes.StepRetry, step, null, null, ("attempt", step.Attempt));
                Notify(MessageKind.Violation, $"«{title}»: {reason}. Повторите шаг (попытка {step.Attempt}).");
            }

            var group = step.Group;
            if (group.State == GroupState.Active && rules.ShouldCloseGroup(group))
                CloseGroup(group, $"достигнут порог нарушений ({group.Violations})");
            else if (step.IsResolved)
                AdvanceWaves(group);
        }

        void StartReadyGroups()
        {
            bool started;
            do
            {
                started = false;
                foreach (var group in groups)
                    if (group.State == GroupState.Pending && CanStart(group))
                    {
                        StartGroup(group);
                        started = true;
                    }
            } while (started);

            if (IsRunning && groups.All(g => g.State == GroupState.Done))
                Finish();
        }

        bool CanStart(GroupRun group)
        {
            if (group.Index == 0)
                return true;
            var previous = groups[group.Index - 1];
            return group.Definition.start == GroupStart.WithPreviousGroup
                ? previous.State != GroupState.Pending
                : previous.State == GroupState.Done;
        }

        void StartGroup(GroupRun group)
        {
            group.State = GroupState.Active;
            Write(RecordTypes.GroupStart, null, null, null, ("name", group.Definition.name), ("index", group.Index));
            if (groups.Count > 1)
                Notify(MessageKind.Info, $"Начата группа «{group.Definition.name}»");

            // Out-of-turn violations may have reached the threshold before the group started.
            if (rules.ShouldCloseGroup(group))
                CloseGroup(group, $"достигнут порог нарушений ({group.Violations})");
            else
                AdvanceWaves(group);
        }

        /// <summary>Activates the current wave; moves on while a wave has nothing left to do.</summary>
        void AdvanceWaves(GroupRun group)
        {
            if (group.State != GroupState.Active)
                return;

            while (group.waveIndex < group.waves.Count)
            {
                var wave = group.waves[group.waveIndex];
                foreach (var step in wave)
                    if (step.Status == StepStatus.Pending)
                        Activate(step);
                if (wave.Exists(s => s.Status == StepStatus.Active))
                    return;
                group.waveIndex++;
            }
            CloseGroup(group, null);
        }

        void Activate(StepRun step)
        {
            step.Status = StepStatus.Active;
            step.Logic.Reset();
            Write(RecordTypes.StepActivated, step, null, null);
        }

        void CloseGroup(GroupRun group, string earlyReason)
        {
            int skipped = 0;
            foreach (var step in group.steps)
                if (!step.IsResolved)
                {
                    Resolve(step, StepStatus.Skipped, earlyReason);
                    skipped++;
                }

            group.State = GroupState.Done;
            Write(RecordTypes.GroupEnd, null, null, earlyReason, ("name", group.Definition.name), ("violations", group.Violations));
            if (earlyReason != null)
                Notify(MessageKind.Warning, $"Группа «{group.Definition.name}» завершена досрочно: {earlyReason}. Пропущено шагов: {skipped}.");
        }

        void Resolve(StepRun step, StepStatus status, string message)
        {
            step.Status = status;
            int attempts = status == StepStatus.Completed ? step.Violations + 1 : step.Violations;
            Write(RecordTypes.StepResult, step, status.ToString(), message, ("attempts", attempts), ("violations", step.Violations));
        }

        void Finish()
        {
            IsRunning = false;
            IsFinished = true;
            Write(RecordTypes.SessionEnd, null, "Finished", null,
                ("completed", steps.Count(s => s.Status == StepStatus.Completed)),
                ("failed", steps.Count(s => s.Status == StepStatus.Failed)),
                ("skipped", steps.Count(s => s.Status == StepStatus.Skipped)));
            Finished?.Invoke();
        }

        void Write(string type, StepRun step, string result, string message, params (string key, object value)[] data)
        {
            var record = new LogRecord(clock(), type, step?.Definition.Id, result, message);
            foreach (var (key, value) in data)
                record.With(key, value);
            Journal.Add(record);
        }

        void Notify(MessageKind kind, string text) => Message?.Invoke(new SessionMessage(kind, text));
    }
}
