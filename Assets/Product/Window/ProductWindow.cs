using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class ProductWindow : EditorWindow
{
    const string ASSETS_FOLDER = "Assets";
    const string TEXTURES_FOLDER = "MerchantImages";
    const string DEFAULT_TEXTURES_PATH = ASSETS_FOLDER + "/" + TEXTURES_FOLDER;
    Product _product;
    Vector2 scrollPos;

    public void LoadAssetFile(Product product)
    {
        _product = product;
    }

    private void OnGUI()
    {
        if (_product == null) return;
        EditorStyles.label.wordWrap = true;
        EditorStyles.textArea.wordWrap = true;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Name: ", _product.name);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Description: ");
        _product.description = EditorGUILayout.TextArea(_product.description, EditorStyles.textArea, GUILayout.Height(80));

        
        if (_product.image != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selected texture: ");
            _product.image = (Texture)EditorGUILayout.ObjectField(_product.image, typeof(Texture), true);
            if (GUILayout.Button("Select other", GUILayout.Width(100)))
            {
                _product.image = null;
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            DrawTextureSelection();
            EditorGUILayout.HelpBox("Select an image to continue", MessageType.Info);
        }

        _product.price = EditorGUILayout.FloatField("Price: ", _product.price);
        _product.rarity = EditorGUILayout.IntField("Rarity: ", _product.rarity);

        EditorUtility.SetDirty(_product);
    }

    void DrawTextureSelection()
    {
        if (!AssetDatabase.IsValidFolder(DEFAULT_TEXTURES_PATH))
        {
            AssetDatabase.CreateFolder(ASSETS_FOLDER, TEXTURES_FOLDER);
        }

        string[] paths = AssetDatabase.FindAssets("", new string[1] { DEFAULT_TEXTURES_PATH });
        if (paths.Length == 0)
        {
            EditorGUILayout.HelpBox("Put your images inside MerchantImages", MessageType.Info);
            return;
        }

        var textureList = new List<Texture>();
        var pathsLoaded = new string[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
            if (!pathsLoaded.Contains(paths[i]))
            {
                pathsLoaded[i] = paths[i];
                Texture loaded = (Texture)AssetDatabase.LoadAssetAtPath(paths[i], typeof(Texture));
                textureList.Add(loaded);
            }
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < textureList.Count; i++)
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button(textureList[i], GUILayout.Height(100), GUILayout.Width(100))) _product.image = textureList[i];
            if (GUILayout.Button("Trash", GUILayout.Width(100)))
            {
                AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(textureList[i]));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }
}

