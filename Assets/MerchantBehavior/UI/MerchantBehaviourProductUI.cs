using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MerchantBehaviourProductUI : MonoBehaviour {
    public Image productImage;
    public Text productName;
    public Text productPrice;
    public Text productCategory;
    Action<Product> buyCallback;
    Product product;

    public void Run(Product product, Action<Product> buyCallback)
    {
        this.product = product;
        this.buyCallback = buyCallback;
        if (product.image) productImage.overrideSprite = Sprite.Create(product.image as Texture2D, new Rect(0, 0, product.image.width, product.image.height), new Vector2(0.5f, 0.5f));
        productName.text = product.name;
        if (product.price > 0) productPrice.text = "$" + product.price;
        else productPrice.text = "FREE";
        productCategory.text = product.category;
    }

    public void OnClick()
    {
        buyCallback(product);
    }
}
