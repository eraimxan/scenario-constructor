using UnityEngine;

namespace ScenarioKit
{
    /// <summary>One raised action: which key, which scene object raised it and who caused it (may be null).</summary>
    public readonly struct ActionEvent
    {
        public readonly ActionKey Key;
        public readonly GameObject Source;
        public readonly GameObject Actor;

        public ActionEvent(ActionKey key, GameObject source = null, GameObject actor = null)
        {
            Key = key;
            Source = source;
            Actor = actor;
        }
    }
}
