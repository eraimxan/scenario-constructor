using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScenarioKit.Demo.Tests
{
    /// <summary>Plays the demo scene like a trainee: clicks real buttons and moves real bodies through real colliders.</summary>
    sealed class DemoSceneDriver
    {
        public ScenarioRunner Runner { get; private set; }
        Rigidbody player;

        public IEnumerator Load()
        {
            SceneManager.LoadScene("Demo");
            yield return null;
            yield return null;
            Bind();
        }

        /// <summary>Re-binds after the scene was reloaded by a replay.</summary>
        public void Bind()
        {
            Runner = Object.FindAnyObjectByType<ScenarioRunner>();
            var mover = Object.FindAnyObjectByType<PlayerMover>();
            mover.enabled = false; // the test moves the player itself
            player = mover.GetComponent<Rigidbody>();
        }

        public ScenarioSession Session => Runner.Session;

        public void Click(string caption)
        {
            var button = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude)
                .FirstOrDefault(b => b.GetComponentInChildren<Text>()?.text == caption);
            Assert.That(button, Is.Not.Null, $"button «{caption}» not found");
            button.onClick.Invoke();
        }

        public IEnumerator MovePlayer(float x, float z, Vector3 push = default)
        {
            yield return Teleport(player, new Vector3(x, 0.85f, z), push);
        }

        public IEnumerator MoveCrate(float x, float z)
        {
            var crate = GameObject.FindWithTag("Box").GetComponent<Rigidbody>();
            yield return Teleport(crate, new Vector3(x, 0.4f, z), default);
        }

        static IEnumerator Teleport(Rigidbody body, Vector3 position, Vector3 push)
        {
            body.position = position;
            body.transform.position = position;
            body.linearVelocity = push;
            for (int i = 0; i < 4; i++)
                yield return new WaitForFixedUpdate();
            body.linearVelocity = Vector3.zero;
        }

        // Zone centres and touching positions of the demo layout (see DemoWorld).
        public IEnumerator EnterZoneA() => MovePlayer(-5, 3.5f);
        public IEnumerator EnterZoneB() => MovePlayer(0, 4.6f);
        public IEnumerator EnterZoneC() => MovePlayer(5, 3.5f);
        public IEnumerator EnterDanger() => MovePlayer(0, 0.6f);
        public IEnumerator EnterBase() => MovePlayer(-5.5f, -4.8f);
        public IEnumerator GoToNeutral() => MovePlayer(0, -3);
        public IEnumerator TouchPillar() => MovePlayer(-5, -1.3f, new Vector3(0, 0, 2));
        public IEnumerator TouchTerminal() => MovePlayer(5.67f, -5.6f, new Vector3(2, 0, 0));
        public IEnumerator PushCrateToDock() => MoveCrate(5.6f, -1);

        public AnalysisReport Analyze(ResultAnalyzer analyzer) => analyzer.Analyze(Session.Journal.Records);

        public StepRun Step(string id) => Session.Steps.Single(s => s.Definition.Id == id);
    }
}
