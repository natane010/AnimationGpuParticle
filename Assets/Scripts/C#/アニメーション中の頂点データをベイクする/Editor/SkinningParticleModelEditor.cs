using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(SkinningParticleModel))]
public class SkinningParticleModelEditor : Editor
{
    #region Editor

    SerializedProperty _shapes;
    SerializedProperty _maxInstanceCount;


    void OnEnable()
    {
        _shapes = serializedObject.FindProperty("_shapes");
        _maxInstanceCount = serializedObject.FindProperty("_maxInstanceCount");
    }

    public override void OnInspectorGUI()
    {
        var template = (SkinningParticleModel)target;

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_shapes, true);
        EditorGUILayout.PropertyField(_maxInstanceCount);
        var rebuild = EditorGUI.EndChangeCheck();

        serializedObject.ApplyModifiedProperties();

        
        EditorGUILayout.LabelField("Instance Count", template.instanceCount.ToString());

        rebuild |= GUILayout.Button("Rebuild");

        if (rebuild) template.RebuildMesh();
    }

    #endregion

    #region Create menu item functions

    [MenuItem("Assets/Create/Skinning/Particle Template")]
    public static void CreateTemplateAsset()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
            path = "Assets";
        else if (Path.GetExtension(path) != "")
            path = path.Replace(Path.GetFileName(path), "");
        var assetPathName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Skinning Particle Template.asset");

        var asset = ScriptableObject.CreateInstance<SkinningParticleModel>();
        AssetDatabase.CreateAsset(asset, assetPathName);
        AssetDatabase.AddObjectToAsset(asset.mesh, asset);

        asset.RebuildMesh();

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    #endregion
}
