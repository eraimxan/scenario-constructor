using UnityEngine;

namespace ScenarioKit
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Scenario/Sources/Collision Action Source")]
    public sealed class CollisionActionSource : ActionSource
    {
        [Tooltip("Действие при столкновении.")]
        [SerializeField] ActionKey onCollision;
        [Tooltip("Реагировать только на объекты с этим тегом. Пусто — на любые.")]
        [SerializeField] string actorTag = "Player";
        [Tooltip("Минимальная относительная скорость удара (м/с). 0 — любое касание.")]
        [SerializeField, Min(0f)] float minImpactSpeed;

        void OnCollisionEnter(Collision collision)
        {
            if (MatchesTag(collision.gameObject, actorTag) && collision.relativeVelocity.magnitude >= minImpactSpeed)
                Emit(onCollision, collision.gameObject);
        }
    }
}
