using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MerchantBehaviorUI : MonoBehaviour
{

    public GameObject productPrefab;
    public GameObject productScreen;
    public Transform parent;
    private List<GameObject> productsGO = new List<GameObject>();
    public Image merchantImage;
    public Text merchantName;
    public Text moneyText;
    private Func<Product, string, bool> buyCallback;
    private Func<float> getMoney;
    private Func<Product, int> getStock;
    private Merchant merchant;
    Func<List<Product>> getProducts;
    public bool closed = true;

    public void Run()
    {
        Run(getProducts, merchant, buyCallback, getMoney, getStock);
    }

    public void Run(Func<List<Product>> getProducts, Merchant merchant, Func<Product, string, bool> buyCallback, Func<float> getMoney, Func<Product, int> getStock)
    {
        closed = false;
        productScreen.SetActive(true);
        this.getStock = getStock;
        this.getMoney = getMoney;
        this.getProducts = getProducts;
        this.buyCallback = buyCallback;
        this.merchant = merchant;

        Clear();
        foreach (var product in getProducts())
        {
            if (getStock(product) < 1) continue;
            GameObject _productGO = Instantiate(productPrefab);
            _productGO.transform.SetParent(parent);
            productsGO.Add(_productGO);

            _productGO.GetComponent<MerchantBehaviourProductUI>().Run(product, OnBuy);
        }

        merchantImage.overrideSprite = Sprite.Create(merchant.image as Texture2D, new Rect(0, 0, merchant.image.width, merchant.image.height), new Vector2(0.5f, 0.5f));
        merchantName.text = merchant.name;
        moneyText.text = "$" + getMoney();
    }

    public void OnBuy(Product product)
    {
        if (!buyCallback(product, merchant.id)) Debug.Log("Not enough money");
        Run();
    }

    public void OnClose()
    {
        productScreen.SetActive(false);
        closed = true;
    }

    public void Clear()
    {
        foreach (var prod in productsGO) Destroy(prod);
    }
}
