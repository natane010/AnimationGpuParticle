using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkinningTrailNormal))]
public class SkinningTrailModelEditor : Editor
{
    #region Editor functions

    SerializedProperty _historyLength;

    void OnEnable()
    {
        _historyLength = serializedObject.FindProperty("_historyLength");
    }

    public override void OnInspectorGUI()
    {
        var template = (SkinningTrailNormal)target;

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_historyLength);
        var rebuild = EditorGUI.EndChangeCheck();

        serializedObject.ApplyModifiedProperties();

        if (rebuild) 
        { 
            template.RebuildMesh(); 
        }
    }

    #endregion

    #region Create menu item functions

    [MenuItem("Assets/Create/Skinning/Trail Template")]
    public static void CreateTemplateAsset()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
            path = "Assets";
        else if (Path.GetExtension(path) != "")
            path = path.Replace(Path.GetFileName(path), "");
        var assetPathName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Skinning Trail Template.asset");

        var asset = ScriptableObject.CreateInstance<SkinningTrailNormal>();
        AssetDatabase.CreateAsset(asset, assetPathName);
        AssetDatabase.AddObjectToAsset(asset.mesh, asset);

        asset.RebuildMesh();

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    #endregion
}
