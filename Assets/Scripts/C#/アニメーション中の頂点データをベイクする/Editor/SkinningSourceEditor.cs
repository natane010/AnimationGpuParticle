using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SkinningSystem))]
public class SkinningSourceEditor : Editor
{
    SerializedProperty _model;

    void OnEnable()
    {
        _model = serializedObject.FindProperty("_model");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_model);

        serializedObject.ApplyModifiedProperties();
    }
}
