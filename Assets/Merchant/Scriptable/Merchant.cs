using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

[Serializable]
[CreateAssetMenu(fileName = "New Merchant", menuName = "Merchant")]
public class Merchant : ScriptableObject
{
    public string id;
    public string description;
    public Texture image;
    public List<Category> categories = new List<Category>();

    public List<string> dialogueKeys = new List<string> { "Open", "Purchase", "Not enough funds", "Exit with purchase", "Exit without purchase", "Exit beacuse of insuficient funds" };
    public List<string> dialogueValues = new List<string> { "", "", "", "", "", "" };

    [OnOpenAsset(1)]
    public static bool Open(int instanceID, int line)
    {
        var instance = EditorUtility.InstanceIDToObject(instanceID) as Merchant;
        if (instance == null) return false;
        EditorWindow.GetWindow<MerchantWindow>().LoadAssetFile(instance);
        return true;
    }
}