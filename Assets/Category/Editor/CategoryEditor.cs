using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Category))]
public class CategoryEditor : Editor
{
    private Category _target;

    private void OnEnable()
    {
        _target = (Category)target;
    }

    public override void OnInspectorGUI()
    {
        if (_target == null) return;
        EditorStyles.label.wordWrap = true;
        EditorStyles.textArea.wordWrap = true;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Name: ", _target.name);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Description:");
        _target.description = EditorGUILayout.TextArea(_target.description, EditorStyles.textArea, GUILayout.Height(80));

        EditorGUILayout.LabelField("");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Products:");
        if (GUILayout.Button("(+)", GUILayout.Width(50)))
        {
            _target.products.Add(null);
            Repaint();
            return;
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < _target.products.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X"))
            {
                _target.products.RemoveAt(i);
                Repaint();
                return;
            }
            _target.products[i] = (Product)EditorGUILayout.ObjectField(_target.products[i], typeof(Product), true);
            EditorGUILayout.EndHorizontal();
        }

        EditorUtility.SetDirty(_target);
    }
}

