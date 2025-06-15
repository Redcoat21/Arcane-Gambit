using Components.Health;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(HealthComponent))]
    public class HealthEditor : UnityEditor.Editor
    {
        private HealthComponent _healthComponent;
        private int _value = 5;
        private void Awake()
        {
            _healthComponent = (HealthComponent)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _value = EditorGUILayout.IntField("Damage/Heal Value", _value);
            if (GUILayout.Button("Take Damage"))
            {
                _healthComponent.TakeDamage(_value);
            }
            else if (GUILayout.Button("Heal"))
            {
                _healthComponent.Heal(_value);
            }
        }
    }
}
