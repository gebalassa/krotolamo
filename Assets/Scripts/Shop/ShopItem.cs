using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] SuperPowers superPower;
    [SerializeField] int price;
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI howManyText;

    int howMany = 0;
    CanvasGroup canvasGroup;

    // Obtencion el superpoder a comprar
    public SuperPowers GetSuperPower()
    {
        return superPower;
    }

    // Obtencion del precio
    public int GetPrice()
    {
        return price;
    }

    // Obtencion del boton de compra
    public Button GetButton()
    {
        return button;
    }

    // Obtencion el indicador de cuantos se tiene
    public TextMeshProUGUI GetHowManyText()
    {
        return howManyText;
    }

    // Obtencion de la cantidad actual
    public int GetHowMany()
    {
        return howMany;
    }

    // Cambio en el texto de la cantidad que se tiene
    public void SetHowManyText(int howMany)
    {
        this.howMany = howMany;
        howManyText.text = string.Format("x{0}", this.howMany); 
    }

    /// <summary>
    /// Vuelve interactuable o no al elemento
    /// </summary>
    /// <param name="value"></param>
    public void SetInteractable(bool value = true)
    {
        button.interactable = value;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup) canvasGroup.alpha = value ? 1 : .5f;
    }
}
