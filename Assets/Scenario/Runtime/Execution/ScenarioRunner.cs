using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScenarioKit
{
    /// <summary>
    /// Scene entry point: creates a session for the chosen mode and feeds it actions from <see cref="ActionBus"/>.
    /// A repeated run reloads the scene so every object starts from its initial state.
    /// </summary>
    [AddComponentMenu("Scenario/Scenario Runner")]
    public sealed class ScenarioRunner : MonoBehaviour
    {
        [SerializeField] ScenarioDefinition scenario;
        [Tooltip("Режим, который запускается сразу при старте сцены. Пусто — режим выбирается в меню.")]
        [SerializeField] ModeProfile autoStartMode;
        [Tooltip("Куда писать журнал. Можно выбрать несколько логгеров или ни одного: журнал в памяти ведётся всегда.")]
        [SerializeField] List<LoggerAsset> loggers = new List<LoggerAsset>();

        // Survives the scene reload used for a repeated run.
        static ModeProfile pendingMode;

        readonly List<ILogWriter> writers = new List<ILogWriter>();
        float startTime;

        public ScenarioDefinition Scenario => scenario;
        public ScenarioSession Session { get; private set; }

        /// <summary>Raised before the session starts, so listeners receive its very first messages.</summary>
        public event Action<ScenarioSession> SessionCreated;

        void OnEnable() => ActionBus.Raised += OnAction;
        void OnDisable() => ActionBus.Raised -= OnAction;

        void Start()
        {
            var mode = pendingMode != null ? pendingMode : autoStartMode;
            pendingMode = null;
            if (mode != null)
                Begin(mode);
        }

        void OnDestroy()
        {
            Session?.Abort();
            CloseWriters();
        }

        /// <summary>Starts the mode; if a session already ran in this scene, reloads the scene first.</summary>
        public void Play(ModeProfile mode)
        {
            if (mode == null)
                return;
            if (Session == null)
            {
                Begin(mode);
                return;
            }

            Session.Abort();
            pendingMode = mode;
            SceneManager.LoadScene(gameObject.scene.buildIndex);
        }

        void Begin(ModeProfile mode)
        {
            foreach (var problem in ScenarioValidator.Validate(mode, scenario))
                Debug.LogWarning($"[Scenario] Режим «{mode.DisplayName}»: {problem}", mode);

            startTime = Time.time;
            Session = new ScenarioSession(scenario, mode, WorldState.Shared, () => Time.time - startTime);
            OpenWriters($"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{mode.name}");
            Session.Finished += CloseWriters;
            SessionCreated?.Invoke(Session);
            Session.Start();
        }

        void OnAction(ActionEvent action) => Session?.HandleAction(action);

        void OpenWriters(string sessionName)
        {
            foreach (var logger in loggers)
            {
                if (logger == null)
                    continue;
                try
                {
                    writers.Add(logger.Open(sessionName));
                }
                catch (Exception e)
                {
                    // A broken output must not stop the training session.
                    Debug.LogError($"[Scenario] Логгер «{logger.name}» не открылся: {e.Message}", logger);
                }
            }
            Session.Journal.Added += WriteToLoggers;
        }

        void WriteToLoggers(LogRecord record)
        {
            foreach (var writer in writers)
                writer.Write(record);
        }

        void CloseWriters()
        {
            foreach (var writer in writers)
                writer.Dispose();
            writers.Clear();
        }
    }
}
