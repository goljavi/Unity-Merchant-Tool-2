using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

[Serializable]
[CreateAssetMenu(fileName = "New Product", menuName = "Product")]
public class Product : ScriptableObject
{
    [HideInInspector] public string description;
    [HideInInspector] public Texture image;
    [HideInInspector] public float price;
    [HideInInspector] public int rarity;
    [HideInInspector] public string category;

    [HideInInspector] public List<string> optionalDataKeys = new List<string>();
    [HideInInspector] public List<string> optionalDataValues = new List<string>();

    [HideInInspector] public List<string> merchantStockKeys = new List<string>();
    [HideInInspector] public List<int> merchantStockValues = new List<int>();

    public ProductEventMap productEvents;

    

    [OnOpenAsset(1)]
    public static bool Open(int instanceID, int line)
    {
        var instance = EditorUtility.InstanceIDToObject(instanceID) as Product;
        if (instance == null) return false;
        EditorWindow.GetWindow<ProductWindow>().LoadAssetFile(instance);
        return true;
    }
}