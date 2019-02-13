using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

/*Este script se encarga de crear un nuevo item en el AssetMenu de Unity para crear un tipo de archivo "ProductEventMap"
 * Este archivo almacena la lista de nodos que se va a cargar en el editor de nodos de forma serializada */
[CreateAssetMenu(fileName = "New Product Event Map", menuName = "Product Event Map")]
public class ProductEventMap : ScriptableObject
{
    public List<ProductEventMapSerializedObject> nodes = new List<ProductEventMapSerializedObject>();
	public ParametersData parameters = new ParametersData();

    [OnOpenAsset(1)]
    public static bool Open(int instanceID, int line)
    {
        var instance = EditorUtility.InstanceIDToObject(instanceID) as ProductEventMap;
        if (instance == null) return false;
        EditorWindow.GetWindow<ProductEventEditor>().LoadAssetFile(instance);
        return true;
    }
}
