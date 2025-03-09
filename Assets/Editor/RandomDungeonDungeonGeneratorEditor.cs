using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class RandomDungeonGeneratorDungeonEditor : UnityEditor.Editor
{
    private AbstractDungeonGenerator _generator;

    private void Awake()
    {
        _generator = (AbstractDungeonGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Create Dungeon"))
        {
            _generator.GenerateDungeon();
        }
    }
}
