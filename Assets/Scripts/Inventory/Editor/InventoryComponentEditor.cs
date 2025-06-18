using UnityEditor;
using UnityEngine;
using Player;
using System.Linq;

[CustomEditor(typeof(InventoryComponent))]
public class InventoryComponentEditor : UnityEditor.Editor
{
private ItemData[] allItems;
private void OnEnable()
{
    // Load all ItemData assets from anywhere in the project
    allItems = AssetDatabase.FindAssets("t:ItemData")
        .Select(guid => AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid)))
        .ToArray();
}

public override void OnInspectorGUI()
{
    DrawDefaultInspector();

    InventoryComponent inventory = (InventoryComponent)target;

    EditorGUILayout.Space(10);
    EditorGUILayout.LabelField("Item Editor", EditorStyles.boldLabel);

    if (allItems == null || allItems.Length == 0)
    {
        EditorGUILayout.HelpBox("No ItemData assets found.", MessageType.Warning);
        return;
    }

    foreach (ItemData item in allItems)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(item.itemName, GUILayout.Width(200));

        if (GUILayout.Button("+", GUILayout.Width(25)))
        {
            Undo.RecordObject(inventory, "Add Item");
            inventory.AddItemEditor(item);
            EditorUtility.SetDirty(inventory);
        }

        if (GUILayout.Button("-", GUILayout.Width(25)))
        {
            Undo.RecordObject(inventory, "Remove Item");
            inventory.RemoveItemEditor(item);
            EditorUtility.SetDirty(inventory);
        }

        EditorGUILayout.EndHorizontal();
    }
}
}