using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

[Serializable]
[CreateAssetMenu(fileName = "New Category", menuName = "Category")]
public class Category : ScriptableObject
{
    public string description;
    public List<Product> products = new List<Product>();
}
