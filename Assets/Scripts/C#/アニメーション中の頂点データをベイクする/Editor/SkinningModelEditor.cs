using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;


[CustomEditor(typeof(SkinningModel))]
public class SkinningModelEditor : Editor
{

    #region Create menu item functions

    static Mesh[] SelectedMeshAssets
    {
        get
        {
            Object[] assets = Selection.GetFiltered(typeof(Mesh), SelectionMode.Deep);
            return assets.Select(x => (Mesh)x).ToArray();
        }
    }

    static bool CheckSkinned(Mesh mesh)
    {
        if (mesh.boneWeights.Length > 0) return true;
        
        return false;
    }

    [MenuItem("Assets/Skinning/Convert Mesh", true)]
    static bool ValidateAssets()
    {
        return SelectedMeshAssets.Length > 0;
    }

    [MenuItem("Assets/Skinning/Convert Mesh")]
    static void ConvertAssets()
    {
        var converted = new List<Object>();

        foreach (var source in SelectedMeshAssets)
        {
            if (!CheckSkinned(source)) continue;

            var dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(source));
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(dirPath + "/New Skinning Model.asset");

            SkinningModel asset = ScriptableObject.CreateInstance<SkinningModel>();
            asset.Initialize(source);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.AddObjectToAsset(asset.mesh, asset);

            converted.Add(asset);
        }

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.objects = converted.ToArray();
    }

    #endregion
}

