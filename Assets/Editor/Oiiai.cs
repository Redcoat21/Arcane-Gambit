using LevelGeneration;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [CustomEditor(typeof(DungeonGenerator))]
    public class Oiiai : UnityEditor.Editor
    {

        private DungeonGenerator _generator;

        private void Awake()
        {
            _generator = (DungeonGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Create Dungeon"))
            {
                _generator.CreateLevel();
            }
        }
    }
}
