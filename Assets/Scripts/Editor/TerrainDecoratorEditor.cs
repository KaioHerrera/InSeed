using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainDecorator))]
public class TerrainDecoratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TerrainDecorator decorator = (TerrainDecorator)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Decorate Terrain (Editor Only)"))
        {
            decorator.DecorateTerrainEditor();
        }
        
        if (GUILayout.Button("Clear All Decorations"))
        {
            decorator.ClearDecorations();
        }
        
        EditorGUILayout.HelpBox("Use 'Decorate Terrain' to populate the terrain with decorations. " +
            "This will create objects in the scene that will persist. " +
            "The decorations will NOT spawn at runtime.", MessageType.Info);
    }
}
