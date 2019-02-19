using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class MerchantWindow : EditorWindow
{
    const string ASSETS_FOLDER = "Assets";
    const string TEXTURES_FOLDER = "MerchantImages";
    const string DEFAULT_TEXTURES_PATH = ASSETS_FOLDER + "/" + TEXTURES_FOLDER;
    Merchant _merchant;
    Vector2 scrollPos;

    public void LoadAssetFile(Merchant merchant)
    {
        _merchant = merchant;
        if (_merchant.id == "") _merchant.id = Guid.NewGuid().ToString();

        foreach (var category in _merchant.categories)
        {
            foreach (var product in category.products)
            {
                var found = false;
                foreach (var key in product.merchantStockKeys)
                {
                    if (key == _merchant.id) found = true;
                }

                if (!found)
                {
                    product.merchantStockKeys.Add(_merchant.id);
                    product.merchantStockValues.Add(0);
                }
            }
        }


    }

    private void OnGUI()
    {
        if (_merchant == null) return;
        EditorStyles.label.wordWrap = true;
        EditorStyles.textArea.wordWrap = true;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Name: ", _merchant.name);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Description: ");
        _merchant.description = EditorGUILayout.TextArea(_merchant.description, EditorStyles.textArea, GUILayout.Height(80));

        

        if (_merchant.image != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selected texture: ");
            _merchant.image = (Texture)EditorGUILayout.ObjectField(_merchant.image, typeof(Texture), true);
            if (GUILayout.Button("Select other", GUILayout.Width(100)))
            {
                _merchant.image = null;
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            DrawTextureSelection();
            EditorGUILayout.HelpBox("Select an image to continue", MessageType.Info);
        }

        foreach (var item in _merchant.categories)
        {
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField(item.name);
            for (int i = 0; i < item.products.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(item.products[i].name + " (edit)", GUILayout.Width(150)))
                {
                    GetWindow<ProductWindow>().LoadAssetFile(item.products[i]);
                }
                for (int j = 0; j < item.products[i].merchantStockKeys.Count; j++)
                {
                    if (item.products[i].merchantStockKeys[j] == _merchant.id)
                    {
                        item.products[i].merchantStockValues[j] = EditorGUILayout.IntField("Stock:", item.products[i].merchantStockValues[j]);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorUtility.SetDirty(_merchant);
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
            if (GUILayout.Button(textureList[i], GUILayout.Height(100), GUILayout.Width(100))) _merchant.image = textureList[i];
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
