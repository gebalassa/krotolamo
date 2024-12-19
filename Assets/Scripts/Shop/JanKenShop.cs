using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JankenUp;
using UnityEngine.UI;

public class JanKenShop : MonoBehaviour
{
    // References
    SingleModeSession singleModeSession;

    // Items que pueden ser comprados
    [Header("Items")]
    [SerializeField] List<ShopItem> shopItems = new List<ShopItem>();

    [Header("Others")]
    [SerializeField] CoinsCurrencyDisplayer coinsDisplayer;


    // Start is called before the first frame update
    void Start()
    {
        singleModeSession = FindObjectOfType<SingleModeSession>();

        // Actualizar el UI del jugador
        SyncUI();

        // Agregar a cada boton de los items la habilidad para comprar
        foreach (ShopItem item in shopItems)
        {
            item.GetButton().onClick.AddListener(delegate {
                BuyItem(item);
            });
        }

        // Ocultar shop si es que no se ha completado el nivel tutorial necesario
        if (!GameController.LoadTutorials().tutorials.Contains(Tutorial.GetSuperPowerLevel()))
        {
            gameObject.SetActive(false);
            coinsDisplayer.gameObject.SetActive(false);
        }

    }

    // Actualizacion de los elementos UI
    public void SyncUI()
    {
        // Indicar la cantidad de monedas del jugador
        if (coinsDisplayer) coinsDisplayer.OnCoinsCurrencyUpdate();

        // Indicar en cada item la cantidad actual que se tiene
        ConfigItems();
    }

    // Configurar los items
    private void ConfigItems()
    {
        foreach(ShopItem item in shopItems)
        {
            int howMany = 0;
            switch (item.GetSuperPower())
            {
                case SuperPowers.TimeMaster:
                    howMany = singleModeSession.GetTimeMaster();
                    break;
                case SuperPowers.MagicWand:
                    howMany = singleModeSession.GetMagicWand();
                    break;
                case SuperPowers.JanKenUp:
                    howMany = singleModeSession.GetJanKenUp();
                    break;
            }

            item.SetHowManyText(howMany);
            if (item.GetHowMany() >= JankenUp.Shop.limitPerItem) item.SetInteractable(false);
        }
    }

    // Intentar realizar la compra de un item
    private void BuyItem(ShopItem item)
    {
        // Comprobar si el usuario tiene las monedas necesarias para comprar el item. En caso contrario, se mostrara el mensaje de Deluxe
        if (item.GetPrice() <= singleModeSession.GetCoins())
        {
            // Si la cantidad relacionada al item supera el maximo permitido de compras
            if (item.GetHowMany() >= JankenUp.Shop.limitPerItem) return;

            // Gastar monedas y guardar los datos del jugador
            singleModeSession.SpendCoins(item.GetPrice());
            singleModeSession.SyncInitialCoins();
            GameController.Save(singleModeSession.GetCoins());
            GameController.ChangeSuperPower(item.GetSuperPower(), 1);
            singleModeSession.AddPurchaseSuperPower(item.GetSuperPower(),1);

            // Reproducir sonido de monedas
            MasterSFXPlayer._player.Coins();

            // Actualizar el UI de monedas
            if (coinsDisplayer) coinsDisplayer.OnCoinsCurrencyUpdate();

            // Anadir uno al despliegue de items
            item.SetHowManyText(item.GetHowMany() + 1);

            if (item.GetHowMany() >= JankenUp.Shop.limitPerItem) item.SetInteractable(false);
        }
        else
        {
            // Reproducir sonido de error
            MasterSFXPlayer._player.Error();
        }

    }

    /// <summary>
    /// Invocacion para la compra de un elemento
    /// </summary>
    /// <param name="superPower"></param>
    public void InvokeBuyItem(SuperPowers superPower) {
        if (!gameObject.activeSelf) return;
        ShopItem shopItem = shopItems.Find(si => si.GetSuperPower() == superPower);
        if (shopItem){
            Button button = shopItem.GetButton();
            if (button.interactable) button.onClick.Invoke();
        }
    
    }
}
