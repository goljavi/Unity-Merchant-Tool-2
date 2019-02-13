using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinyJson;

public class MerchantBehavior : MonoBehaviour
{
    public Merchant merchant;
    public MerchantBehaviorUI merchantUI;
    public GameObject player;
    [HideInInspector] public float speed = 5;
    [HideInInspector] public float radius = 5;
    [HideInInspector] public List<Vector3> nodes;
    float _threshold = 0.3f;
    int _actualNode;
    public Inventory inventory;
    private static Inventory _inventory;
    private bool entered = false;

    private static Dictionary<string, int> intParameters;
    private static Dictionary<string, float> floatParameters;
    private static Dictionary<string, bool> boolParameters;

    void Start()
    {
        intParameters = new Dictionary<string, int>();
        floatParameters = new Dictionary<string, float>();
        boolParameters = new Dictionary<string, bool>();

        _inventory = inventory;

        if (nodes.Count > 0) transform.position = nodes[0];
        _actualNode = 1;

        SetFloat("money", GetMoney());
    }

    void Update()
    {
        if (nodes.Count == 0 || player == null) return;

        var distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance < radius)
        {
            if(merchantUI.closed && !entered) merchantUI.Run(GetAllProducts, GetMerchant(), Buy, GetMoney, GetStock);
            if (distance > 1.2) transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * (speed/4));
            entered = true;
            return;
        }

        entered = false;
        if (Vector3.Distance(transform.position, nodes[_actualNode]) < _threshold) _actualNode = (int)Mathf.Repeat(_actualNode + 1, nodes.Count);
        transform.position = Vector3.Lerp(transform.position, nodes[_actualNode], Time.deltaTime * speed);
    }

    public Merchant GetMerchant()
    {
        return merchant;
    }

    public float GetMoney()
    {
        return inventory.money;
    }

    public static bool Buy(Product product, string merchantId)
    {
        if (product.price > _inventory.money) return false;

        var stock = GetProductStock(product, merchantId);
        if (stock < 1) return false;

        AssignProductStock(product, merchantId, stock-1);
        _inventory.money -= product.price;
        _inventory.inventory.Add(product);

        SetFloat("money", _inventory.money);
        return true;
    }

    public int GetStock(Product product)
    {
        return GetProductStock(product, merchant.id);
    }

    public static int GetProductStock(Product product, string merchantId)
    {
        for (int i = 0; i < product.merchantStockKeys.Count; i++)
        {
            if (merchantId == product.merchantStockKeys[i]) return product.merchantStockValues[i];
        }

        return -1;
    }

    public static void AssignProductStock(Product product, string merchantId, int newstock)
    {
        for (int i = 0; i < product.merchantStockKeys.Count; i++)
        {
            if (merchantId == product.merchantStockKeys[i]) product.merchantStockValues[i] = newstock;
        }
    }

    public static string GetProductOptionalData(Product product, string key)
    {
        for (int i = 0; i < product.optionalDataKeys.Count; i++)
        {
            if (key == product.optionalDataKeys[i]) return product.optionalDataValues[i];
        }

        return null;
    }

    public List<Category> GetAllCategories()
    {
        return merchant.categories;
    }

    public Category GetCategoryByName(string name)
    {
        foreach (var item in merchant.categories)
            if (name == item.name) return item;
    
        return null;
    }

    public List<Product> GetAllProducts()
    {
        List<Product> products = new List<Product>();
        foreach (var category in merchant.categories)
            foreach(var product in category.products)
            {
                product.category = category.name;

                var prod = GetProductWithEvents(product);
                if(prod != null) products.Add(prod);
            }
                

        return products;
    }

    public Product GetProductWithEvents(Product product)
    {
        var parameters = new Parameters().SetData(product.productEvents.parameters);
        foreach (var node in product.productEvents.nodes)
        {
            if(node.nodeType == "Comparison")
            {
                var comparisonDataObject = node.data.FromJson<Dictionary<string, object>>();
                var comparisonDataString = node.data.FromJson<Dictionary<string, string>>();
                Debug.Log(comparisonDataString["parameterName"]);
                var parametersNames = comparisonDataString["parameterName"].Replace("\"", "").Split(',');
                List<int> childrenIDs = new List<int>();

                foreach (var item in comparisonDataString["childrenIDs"].Split(','))
                {
                    var parsed = int.Parse(item);
                    childrenIDs.Add(parsed);
                }

                var result = Compare(
                    parameters,
                    (ComparativeNode.ComparisonType)comparisonDataObject["comparisonType"],
                    (ComparativeNode.ComparisonOperator)comparisonDataObject["comparisonOperator"],
                    parametersNames
                );

                Debug.Log(result);

                if (result)
                {
                    if (childrenIDs[0] > 0)
                    {
                        return GetNewProductWithEvent(FindNodeById(childrenIDs[0], product.productEvents.nodes), product);
                    }
                }
                else
                {
                    if (childrenIDs[1] > 0)
                    {
                        return GetNewProductWithEvent(FindNodeById(childrenIDs[1], product.productEvents.nodes), product);
                    }
                }
            }
        }

        return product;
    }

    public Product GetNewProductWithEvent(ProductEventMapSerializedObject node, Product product)
    {
        if (node == null) return product;

        var data = ProductNode.GetNodeDataAsDictionary(node.data);

        if ((bool)data["dontShow"]) return null;

        if ((string)data["name"] != "") product.name = (string)data["name"];

        if ((string)data["description"] != "") product.description = (string)data["description"];

        if ((bool)data["enablePrice"]) product.price = (float)data["price"];

        if ((bool)data["enableRarity"]) product.rarity = (int)data["rarity"];

        return product;
    }

    public ProductEventMapSerializedObject FindNodeById(int id, List<ProductEventMapSerializedObject> nodes)
    {
        foreach (var item in nodes)
        {
            if (item.id == id) return item; 
        }

        return null;
    }

    public Product GetProductByName(string name)
    {
        foreach (var category in merchant.categories)
            foreach (var product in category.products)
                if (product.name == name)
                {
                    product.category = category.name;
                    return product;
                }

        return null;
    }

    public string GetDialog(string key)
    {
        for (int i = 0; i < merchant.dialogueKeys.Count; i++)
            if (merchant.dialogueKeys[i] == key) return merchant.dialogueValues[i];

        return null;
    }

    //Ejecuta la comparacion correspondiente
    public bool Compare(Parameters parameters, ComparativeNode.ComparisonType activeType, ComparativeNode.ComparisonOperator activeOperator, string[] parameterNames)
    {
        switch (activeType)
        {
            case ComparativeNode.ComparisonType.Float:
                return CompareFloat(parameters, activeOperator, parameterNames);
            case ComparativeNode.ComparisonType.Int:
                return CompareInt(parameters, activeOperator, parameterNames);
            case ComparativeNode.ComparisonType.Bool:
                return GetBool(parameterNames[0], parameters);
            default:
                Debug.LogWarning("Invalid Comparison Type Enum at " + this.ToString());
                return false;
        }
    }

    //Comparacion de floats
    private bool CompareFloat(Parameters parameters, ComparativeNode.ComparisonOperator activeOperator, string[] parameterNames)
    {

        float float1 = GetFloat(parameterNames[0], parameters);
        float float2 = GetFloat(parameterNames[1], parameters);
        switch (activeOperator)
        {
            case ComparativeNode.ComparisonOperator.Equals:
                return float1 == float2;
            case ComparativeNode.ComparisonOperator.NotEqual:
                return !(float1 == float2);
            case ComparativeNode.ComparisonOperator.Lesser:
                return float1 < float2;
            case ComparativeNode.ComparisonOperator.LesserEquals:
                return float1 <= float2;
            case ComparativeNode.ComparisonOperator.Greater:
                return float1 > float2;
            case ComparativeNode.ComparisonOperator.GreaterEquals:
                return float1 >= float2;
            default:
                Debug.LogWarning("Invalid Comparison Operator Enum at " + this.ToString());
                return false;
        }
    }

    //comparacion de ints
    private bool CompareInt(Parameters parameters, ComparativeNode.ComparisonOperator activeOperator, string[] parameterNames)
    {
        float int1 = GetInt(parameterNames[0], parameters);
        float int2 = GetInt(parameterNames[1], parameters);
        switch (activeOperator)
        {
            case ComparativeNode.ComparisonOperator.Equals:
                return int1 == int2;
            case ComparativeNode.ComparisonOperator.NotEqual:
                return !(int1 == int2);
            case ComparativeNode.ComparisonOperator.Lesser:
                return int1 < int2;
            case ComparativeNode.ComparisonOperator.LesserEquals:
                return int1 <= int2;
            case ComparativeNode.ComparisonOperator.Greater:
                return int1 > int2;
            case ComparativeNode.ComparisonOperator.GreaterEquals:
                return int1 >= int2;
            default:
                Debug.LogWarning("Invalid Comparison Operator Enum at " + this.ToString());
                return false;
        }
    }

    public static void SetInt(string key, int value)
    {
        intParameters[key] = value;
    }

    public static void SetFloat(string key, float value)
    {
        floatParameters[key] = value;
    }

    public static void SetBool(string key, bool value)
    {
        boolParameters[key] = value;
    }

    public static int GetInt(string key)
    {
        return GetInt(key);
    }

    public static float GetFloat(string key)
    {
        return GetFloat(key);
    }

    public static bool GetBool(string key)
    {
        return GetBool(key);
    }

    private static int GetInt(string key, Parameters parameters = null)
    {
        if (intParameters.ContainsKey(key)) return intParameters[key];
        else if (parameters != null) return parameters.GetInt(key);
        else return default(int);
    }

    private static float GetFloat(string key, Parameters parameters = null)
    {
        if (floatParameters.ContainsKey(key))  return floatParameters[key];
        else if (parameters != null) return parameters.GetFloat(key);
        else return default(float);
    }

    private static bool GetBool(string key, Parameters parameters = null)
    {
        if (boolParameters.ContainsKey(key)) return boolParameters[key];
        else if (parameters != null) return parameters.GetBool(key);
        else return default(bool);
    }
}
