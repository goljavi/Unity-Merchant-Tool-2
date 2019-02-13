using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Merchant))]
public class MerchantEditor : Editor
{
    private Merchant _target;

    private void OnEnable()
    {
        _target = (Merchant)target;
    }

    public override void OnInspectorGUI()
    {
        if (_target == null) return;
        EditorStyles.textArea.wordWrap = true;
        EditorStyles.label.wordWrap = true;
        EditorGUILayout.LabelField("To open the merchant editor window double click on a merchant");
        EditorGUILayout.LabelField("");

        EditorGUILayout.LabelField("");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Categories:");
        if (GUILayout.Button("(+)", GUILayout.Width(50)))
        {
            _target.categories.Add(null);
            Repaint();
            return;
        }
        EditorGUILayout.EndHorizontal();
        for (int i = 0; i < _target.categories.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X"))
            {
                _target.categories.RemoveAt(i);
                Repaint();
                return;
            }
            _target.categories[i] = (Category)EditorGUILayout.ObjectField(_target.categories[i], typeof(Category), true);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField("");


        for (int i = 0; i < _target.dialogueKeys.Count; i++)
        {
            EditorGUILayout.LabelField(_target.dialogueKeys[i]);
            _target.dialogueValues[i] = EditorGUILayout.TextArea(_target.dialogueValues[i], EditorStyles.textArea, GUILayout.Height(80));
            EditorGUILayout.LabelField("");
        }

        EditorUtility.SetDirty(_target);
    }
}