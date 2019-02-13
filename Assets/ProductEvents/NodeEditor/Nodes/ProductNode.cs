using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ProductNode : BaseNode {
    const string DATA_SEPARATOR = "**$#";
    string name;
    string description;
    float price;
    int rarity;
    bool dontShow;
    bool enablePrice;
    bool enableRarity;

    public override string GetNodeType { get { return "Product"; } }

    public override void DrawNode()
    {
		EditorStyles.textArea.wordWrap = true;
 
        var dontShowValue = EditorGUILayout.Toggle("Disable Product", dontShow);

        EditorGUI.BeginDisabledGroup(dontShow);
            var nameValue = EditorGUILayout.TextField("Name", name);

            EditorGUILayout.LabelField("Description");
            var descriptionValue = EditorGUILayout.TextArea(description, EditorStyles.textArea, GUILayout.Height(80));

            EditorGUILayout.BeginHorizontal();

                var enablePriceValue = EditorGUILayout.Toggle(enablePrice);

                EditorGUI.BeginDisabledGroup(!enablePrice);
                    var priceValue = EditorGUILayout.FloatField("Price", price);
                EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        var enableRarityValue = EditorGUILayout.Toggle(enableRarity);

        EditorGUI.BeginDisabledGroup(!enableRarity);
        var rarityValue = EditorGUILayout.IntField("Rarity", rarity);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Empty fields fallback to default values");

        if (
            dontShowValue != dontShow ||
            descriptionValue != description ||
            nameValue != name ||
            priceValue != price ||
            rarityValue != rarity ||
            enablePriceValue != enablePrice ||
            enableRarityValue != enableRarity
        )
		{
            dontShow = dontShowValue;
            name = nameValue;
			description = descriptionValue;
            price = priceValue;
            rarity = rarityValue;
            enablePrice = enablePriceValue;
            enableRarity = enableRarityValue;
            reference.NotifyChangesWereMade();
		}
	}

	public override Color GetBackgroundColor() {

        defaultColor = Color.green;
        return color;
    }


    public override string GetNodeData() {
		return string.Join(DATA_SEPARATOR, new string[] { name, description, "" + price, "" + rarity, dontShow.ToString(), enablePrice.ToString(), enableRarity.ToString() });
	}

    public static Dictionary<string, object> GetNodeDataAsDictionary(string data)
    {
        var dictionary = new Dictionary<string, object>();
        var dataArr = data.Split(new string[] { DATA_SEPARATOR }, StringSplitOptions.None);
        dictionary["name"] = dataArr[0];
        dictionary["description"] = dataArr[1];
        dictionary["price"] = float.Parse(dataArr[2]);
        dictionary["rarity"] = int.Parse(dataArr[3]);
        dictionary["dontShow"] = bool.Parse(dataArr[4]);
        dictionary["enablePrice"] = bool.Parse(dataArr[5]);
        dictionary["enableRarity"] = bool.Parse(dataArr[6]);
        return dictionary;
    }

    public override BaseNode SetNodeData(string data) {
		var dataArr = data.Split(new string[] { DATA_SEPARATOR }, StringSplitOptions.None);
        name = dataArr[0];
        description = dataArr[1];
        price = float.Parse(dataArr[2]);
        rarity = int.Parse(dataArr[3]);
        dontShow = bool.Parse(dataArr[4]);
        enablePrice = bool.Parse(dataArr[5]);
        enableRarity = bool.Parse(dataArr[6]);
        return this;
	}

	public override void DrawConnection() {
		if (parents.Count > 0)
		{
			foreach (var parent in parents)
			{
                if (parent == null) continue;

                var finalcolor = Color.white;
                if (parent.GetNodeType == "Comparison")
                {
                    var compnode = (ComparativeNode)parent;

                    if (System.Array.IndexOf(compnode.children, (BaseNode)this) == 0) finalcolor = Color.green;
                    else finalcolor = Color.red;
                }

                ProductEventEditor.DrawNodeConnection(parent.windowRect, windowRect, true, finalcolor);
			}
		}
	}

	public override bool CanTransitionTo(BaseNode node) {
		List<string> types = new List<string> { };

		return types.Contains(node.GetNodeType);
	}
}
