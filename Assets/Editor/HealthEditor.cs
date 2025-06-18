using Components.Health;
using UnityEditor;
using UnityEngine;

namespace HealthEditor
{
    [CustomEditor(typeof(HealthComponent))]
    public class HealthEditor : UnityEditor.Editor
    {
        private HealthComponent _healthComponent;
        private int _damageValue = 4;
        private int _healValue = 5;
        private void Awake()
        {
            _healthComponent = (HealthComponent)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _damageValue = EditorGUILayout.IntField("Damage Value", _damageValue);
            _healValue = EditorGUILayout.IntField("Heal Value", _healValue);
            if (GUILayout.Button("Take Damage"))
            {
                _healthComponent.TakeDamage(_damageValue);
            }
            else if (GUILayout.Button("Heal"))
            {
                _healthComponent.Heal(_healValue);
            }
        }
    }
}
