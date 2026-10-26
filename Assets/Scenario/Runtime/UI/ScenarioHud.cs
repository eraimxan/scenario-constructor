using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ScenarioKit
{
    /// <summary>
    /// Minimal uGUI front end: mode menu, active tasks, violation messages, results with
    /// re-analysis and the journal. Only reads the session; all logic stays in the core.
    /// </summary>
    [AddComponentMenu("Scenario/Scenario HUD")]
    public sealed class ScenarioHud : MonoBehaviour
    {
        const int MessageLimit = 5;
        const int JournalLimit = 150;

        [SerializeField] ScenarioRunner runner;
        [Tooltip("Анализаторы, доступные для повторной оценки (анализатор режима добавляется автоматически).")]
        [SerializeField] List<ResultAnalyzer> analyzers = new List<ResultAnalyzer>();

        [Header("Меню")]
        [SerializeField] GameObject menuPanel;
        [SerializeField] Text menuTitle;
        [SerializeField] Button modeButtonTemplate;
        [SerializeField] Button closeMenuButton;

        [Header("Прохождение")]
        [SerializeField] Text headerText;
        [SerializeField] Text tasksText;
        [SerializeField] Text messagesText;
        [SerializeField] Button openMenuButton;

        [Header("Результат")]
        [SerializeField] GameObject resultPanel;
        [SerializeField] Dropdown analyzerDropdown;
        [SerializeField] Text reportText;
        [SerializeField] Button replayButton;

        [Header("Журнал")]
        [SerializeField] GameObject journalPanel;
        [SerializeField] Text journalText;
        [SerializeField] Button journalButton;

        readonly List<string> messages = new List<string>();
        readonly List<ResultAnalyzer> offeredAnalyzers = new List<ResultAnalyzer>();
        ScenarioSession session;

        void Awake()
        {
            runner.SessionCreated += Attach;
            BuildMenu();
            closeMenuButton.onClick.AddListener(() => menuPanel.SetActive(false));
            openMenuButton.onClick.AddListener(() => menuPanel.SetActive(true));
            journalButton.onClick.AddListener(() => SetJournalVisible(!journalPanel.activeSelf));
            replayButton.onClick.AddListener(() => runner.Play(session.Mode));
            analyzerDropdown.onValueChanged.AddListener(_ => ShowReport());

            resultPanel.SetActive(false);
            journalPanel.SetActive(false);
            menuPanel.SetActive(true);
            closeMenuButton.gameObject.SetActive(false);
            headerText.text = runner.Scenario != null ? runner.Scenario.Title : "Сценарий не задан";
            tasksText.text = "Выберите режим прохождения.";
            messagesText.text = "";
        }

        void OnDestroy() => runner.SessionCreated -= Attach;

        void BuildMenu()
        {
            menuTitle.text = runner.Scenario != null ? runner.Scenario.Title : "Сценарий не задан";
            modeButtonTemplate.gameObject.SetActive(false);
            if (runner.Scenario == null)
                return;

            foreach (var mode in runner.Scenario.Modes.Where(m => m != null))
            {
                var button = Instantiate(modeButtonTemplate, modeButtonTemplate.transform.parent);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<Text>().text = mode.DisplayName;
                button.onClick.AddListener(() => runner.Play(mode));
            }
        }

        void Attach(ScenarioSession newSession)
        {
            session = newSession;
            session.Changed += Refresh;
            session.Message += AddMessage;
            session.Finished += ShowResults;
            menuPanel.SetActive(false);
            closeMenuButton.gameObject.SetActive(true);
            messages.Clear();
            messagesText.text = "";
        }

        void Refresh()
        {
            headerText.text = $"{session.Mode.DisplayName}" + (session.IsFinished ? " — завершено" : "");

            var text = new StringBuilder();
            foreach (var group in session.Groups.Where(g => g.State == GroupState.Active))
            {
                var hint = session.Rules.DescribeGroup(group);
                text.AppendLine($"<b>{group.Definition.name}</b>{(hint != null ? $"  <color=#aaaaaa>({hint})</color>" : "")}");
                foreach (var step in group.Steps)
                    text.AppendLine(DescribeStep(step));
                int pending = group.Steps.Count(s => s.Status == StepStatus.Pending);
                if (pending > 0)
                    text.AppendLine($"<color=#888888>  ещё впереди: {pending}</color>");
                text.AppendLine();
            }
            tasksText.text = text.ToString();

            if (journalPanel.activeSelf)
                RefreshJournal();
        }

        static string DescribeStep(StepRun step)
        {
            var title = step.Definition.Title;
            switch (step.Status)
            {
                case StepStatus.Active:
                    var progress = step.ProgressHint != null ? $" [{step.ProgressHint}]" : "";
                    var attempt = step.Attempt > 1 ? $" <color=#ffcc66>попытка {step.Attempt}</color>" : "";
                    return $"<color=#ffffff>> <b>{title}</b>{progress}{attempt}</color>\n    <color=#cccccc>{step.Definition.Instruction}</color>";
                case StepStatus.Completed:
                    return $"<color=#66dd77>  + {title}</color>";
                case StepStatus.Failed:
                    return $"<color=#ff6666>  x {title} (Failed)</color>";
                case StepStatus.Skipped:
                    return $"<color=#999999>  - {title} (Skipped)</color>";
                default:
                    return null;
            }
        }

        void AddMessage(SessionMessage message)
        {
            var color = message.Kind switch
            {
                MessageKind.Violation => "#ff6666",
                MessageKind.Warning => "#ffaa44",
                MessageKind.Success => "#66dd77",
                _ => "#dddddd"
            };
            messages.Add($"<color={color}>{message.Text}</color>");
            if (messages.Count > MessageLimit)
                messages.RemoveAt(0);
            messagesText.text = string.Join("\n", messages);
        }

        void ShowResults()
        {
            offeredAnalyzers.Clear();
            if (session.Mode.DefaultAnalyzer != null)
                offeredAnalyzers.Add(session.Mode.DefaultAnalyzer);
            offeredAnalyzers.AddRange(analyzers.Where(a => a != null && !offeredAnalyzers.Contains(a)));

            analyzerDropdown.ClearOptions();
            analyzerDropdown.AddOptions(offeredAnalyzers.Select(a => a.DisplayName).ToList());
            analyzerDropdown.SetValueWithoutNotify(0);
            resultPanel.SetActive(true);
            ShowReport();
        }

        void ShowReport()
        {
            if (offeredAnalyzers.Count == 0)
            {
                reportText.text = "Не настроено ни одного анализатора.";
                return;
            }
            var analyzer = offeredAnalyzers[Mathf.Clamp(analyzerDropdown.value, 0, offeredAnalyzers.Count - 1)];
            var report = analyzer.Analyze(session.Journal.Records);
            var text = new StringBuilder();
            text.AppendLine($"<size=26><b>{report.Summary}</b></size>");
            foreach (var line in report.Lines)
                text.AppendLine(line);
            foreach (var warning in report.Warnings)
                text.AppendLine($"<color=#ffaa44>! {warning}</color>");
            reportText.text = text.ToString();
        }

        void SetJournalVisible(bool visible)
        {
            journalPanel.SetActive(visible);
            if (visible)
                RefreshJournal();
        }

        void RefreshJournal()
        {
            if (session == null)
            {
                journalText.text = "Журнал появится после запуска режима.";
                return;
            }
            var records = session.Journal.Records;
            var shown = records.Skip(Mathf.Max(0, records.Count - JournalLimit));
            journalText.text = (records.Count > JournalLimit ? $"… показаны последние {JournalLimit} из {records.Count}\n" : "")
                + string.Join("\n", shown.Select(r => r.ToString()));
        }
    }
}
