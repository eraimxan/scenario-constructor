using System;
using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Single entry point for actions. Sources do not know who listens;
    /// the runner subscribes while a session is alive.
    /// </summary>
    public static class ActionBus
    {
        public static event Action<ActionEvent> Raised;

        public static void Raise(ActionKey key, GameObject source = null, GameObject actor = null)
        {
            if (key == null)
            {
                Debug.LogWarning($"[Scenario] Источник действия на «{(source != null ? source.name : "?")}» не настроен: не задан Action Key.", source);
                return;
            }
            Raised?.Invoke(new ActionEvent(key, source, actor));
        }

        // Keeps the bus clean when domain reload is disabled in Enter Play Mode options.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Raised = null;
    }
}
