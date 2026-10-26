using UnityEngine;

namespace ScenarioKit
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Scenario/Sources/Trigger Action Source")]
    public sealed class TriggerActionSource : ActionSource
    {
        [Tooltip("Действие при входе в триггер. Можно оставить пустым.")]
        [SerializeField] ActionKey onEnter;
        [Tooltip("Действие при выходе из триггера. Можно оставить пустым.")]
        [SerializeField] ActionKey onExit;
        [Tooltip("Реагировать только на объекты с этим тегом. Пусто — на любые.")]
        [SerializeField] string actorTag = "Player";

        void OnTriggerEnter(Collider other)
        {
            if (onEnter != null && MatchesTag(other.gameObject, actorTag))
                Emit(onEnter, other.gameObject);
        }

        void OnTriggerExit(Collider other)
        {
            if (onExit != null && MatchesTag(other.gameObject, actorTag))
                Emit(onExit, other.gameObject);
        }
    }
}
