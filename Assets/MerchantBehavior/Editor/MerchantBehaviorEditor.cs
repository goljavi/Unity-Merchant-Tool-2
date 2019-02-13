using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MerchantBehavior))]
public class MerchantBehaviorEditor : Editor
{
    MerchantBehavior _target;
    bool _hideNames;
    bool _hideNodes;
    bool _snap;
    float _snapValue;

    void OnEnable()
    {
        _target = (MerchantBehavior)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.LabelField("Nodes:");
        for (int i = 0; i < _target.nodes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _target.nodes[i] = EditorGUILayout.Vector3Field(i.ToString(), _target.nodes[i]);
            if (GUILayout.Button("X"))
            {
                _target.nodes.Remove(_target.nodes[i]);
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add Node"))
        {
            _target.nodes.Add(_target.transform.position);
        }  
    }

    void OnSceneGUI()
    {
        Handles.color = Color.white;
        if (!_hideNames) Handles.Label(_target.transform.position + new Vector3(0, 2, 0), _target.name + " - Speed: " + _target.speed + " - Vision: " + _target.radius);
        _target.radius = Handles.RadiusHandle(Quaternion.identity, _target.transform.position, _target.radius);
        _target.speed = Handles.ScaleValueHandle(_target.speed, _target.transform.position, Quaternion.Euler(new Vector3(0, 90, 0)), _target.speed * 1.5f, Handles.ArrowHandleCap, 0.5f);
        if (_target.speed < 1) _target.speed = 1;

        if (!_hideNodes && _target != null)
        {
            Handles.color = Color.green;
            for (int i = 0; i < _target.nodes.Count; i++)
            {
                _target.nodes[i] = Handles.PositionHandle(_target.nodes[i], Quaternion.identity);
                if (_snap) _target.nodes[i] = new Vector3(_snapValue * Mathf.Round(_target.nodes[i].x / _snapValue), _snapValue * Mathf.Round(_target.nodes[i].y / _snapValue), _snapValue * Mathf.Round(_target.nodes[i].z / _snapValue));
                Handles.SphereHandleCap(i, _target.nodes[i], Quaternion.identity, 0.3f, UnityEngine.EventType.Repaint);
                if (!_hideNames) Handles.Label(_target.nodes[i], i.ToString());
            }

            for (int i = 0; i < _target.nodes.Count; i++)
            {
                Handles.DrawLine(_target.nodes[i], _target.nodes[(int)Mathf.Repeat(i + 1, _target.nodes.Count)]);
            }
        }

        Handles.BeginGUI();
        GUILayout.BeginVertical();
        GUILayout.BeginArea(new Rect(20, 20, 150, 500));
        if (GUILayout.Button("Show/hide names"))
        {
            _hideNames = !_hideNames;
        }
        if (GUILayout.Button("Show/hide nodes"))
        {
            _hideNodes = !_hideNodes;
        }
        if (!_hideNodes)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Snap: " + _snap.ToString()))
            {
                _snap = !_snap;
            }
            if (_snap)
            {
                _snapValue = EditorGUILayout.FloatField(_snapValue);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
        GUILayout.EndVertical();
        Handles.EndGUI();

        if (_snapValue == 0) _snapValue = 1;
        if (_snap) _target.transform.position = new Vector3(_snapValue * Mathf.Round(_target.transform.position.x / _snapValue), _snapValue * Mathf.Round(_target.transform.position.y / _snapValue), _snapValue * Mathf.Round(_target.transform.position.z / _snapValue));
    }
}
