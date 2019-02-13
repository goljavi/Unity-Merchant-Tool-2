using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Product))]
public class ProductEditor : Editor
{
    private Product _target;

    private void OnEnable()
    {
        _target = (Product)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (_target == null) return;
        EditorStyles.label.wordWrap = true;
        EditorGUILayout.LabelField("");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Add aditional data (key, value):");
        if (GUILayout.Button("(+)", GUILayout.Width(50)))
        {
            _target.optionalDataKeys.Add("");
            _target.optionalDataValues.Add("");
            Repaint();
            return;
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < _target.optionalDataKeys.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X"))
            {
                _target.optionalDataKeys.RemoveAt(i);
                _target.optionalDataValues.RemoveAt(i);
                Repaint();
                return;
            }
            _target.optionalDataKeys[i] = EditorGUILayout.TextField(_target.optionalDataKeys[i]);
            _target.optionalDataValues[i] = EditorGUILayout.TextField(_target.optionalDataValues[i]);
            EditorGUILayout.EndHorizontal();
        }

        EditorUtility.SetDirty(_target);
    }
}
