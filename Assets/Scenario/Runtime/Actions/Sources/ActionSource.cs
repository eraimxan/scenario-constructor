using UnityEngine;

namespace ScenarioKit
{
    /// <summary>
    /// Base for components that turn Unity callbacks into scenario actions.
    /// A custom source is any MonoBehaviour that calls <see cref="Emit"/> (or <see cref="ActionBus.Raise"/>).
    /// </summary>
    public abstract class ActionSource : MonoBehaviour
    {
        protected void Emit(ActionKey key, GameObject actor = null) => ActionBus.Raise(key, gameObject, actor);

        /// <summary>Empty tag means "anyone".</summary>
        protected static bool MatchesTag(GameObject candidate, string requiredTag) =>
            string.IsNullOrEmpty(requiredTag) || candidate.CompareTag(requiredTag);
    }
}
