using System.Collections.Generic;
using UnityEngine;

namespace ScenarioKit
{
    public interface IWorldState
    {
        bool Get(StateKey key);
    }

    /// <summary>Boolean flags published by scene objects (see <see cref="StateFlag"/>). Unknown keys read as false.</summary>
    public sealed class WorldState : IWorldState
    {
        public static WorldState Shared { get; private set; } = new WorldState();

        readonly Dictionary<StateKey, bool> values = new Dictionary<StateKey, bool>();

        public bool Get(StateKey key) => key != null && values.TryGetValue(key, out var value) && value;

        public void Set(StateKey key, bool value)
        {
            if (key != null)
                values[key] = value;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Shared = new WorldState();
    }
}
