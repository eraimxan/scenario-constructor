using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScenarioKit.Demo
{
    /// <summary>
    /// Example of a custom component with its own UnityEvents. The scenario reacts to it
    /// only through inspector wiring (UnityEventActionSource, StateFlag), no code references.
    /// </summary>
    public sealed class PowerSwitch : MonoBehaviour
    {
        [SerializeField] bool isOn;
        [SerializeField] Renderer indicator;
        [SerializeField] Text label;
        [SerializeField] Color onColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] Color offColor = new Color(0.6f, 0.2f, 0.2f);

        public UnityEvent onTurnedOn = new UnityEvent();
        public UnityEvent onTurnedOff = new UnityEvent();
        public UnityEvent<bool> onChanged = new UnityEvent<bool>();

        public bool IsOn => isOn;

        void Start() => UpdateVisuals();

        public void Toggle()
        {
            isOn = !isOn;
            UpdateVisuals();
            onChanged.Invoke(isOn);
            (isOn ? onTurnedOn : onTurnedOff).Invoke();
        }

        void UpdateVisuals()
        {
            if (indicator != null)
                indicator.material.color = isOn ? onColor : offColor;
            if (label != null)
                label.text = isOn ? "Питание: ВКЛ" : "Питание: ВЫКЛ";
        }
    }
}
