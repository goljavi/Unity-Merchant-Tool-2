using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/* Este es el custom editor de los archivos "ProductEventMap"
 * Sirve para poder abrir la ventana de nodos haciendo click en el archivo */
[CustomEditor(typeof(ProductEventMap))]
public class ProductEventMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("To open the product event editor windows double click on a product event map");
    }
}
