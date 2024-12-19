using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JankenUp;
using UnityEngine.UI;
using TMPro;

public class JanKenShopCharacters : MonoBehaviour
{
    // References
    SingleModeSession singleModeSession;

    // UI
    [Header("UI")]
    [SerializeField] Button buyButton;
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] Image coinImage;
    [SerializeField] CoinsDisplayer coinsDisplayer;
    [SerializeField] GameObject deluxeOverlayPrefab;

    // Start is called before the first frame update
    void Start()
    {
        singleModeSession = FindObjectOfType<SingleModeSession>();

        // Actualizar el UI del jugador
        SyncUI();
    
    }

    // Actualizacion de los elementos UI
    public void SyncUI()
    {
        // Indicar la cantidad de monedas del jugador
        coinsDisplayer.SetLabel(singleModeSession.GetCoins());
    }

    // Intentar realizar la compra de un personaje
    public void BuyCharacter()
    {
        // Obtener el PJDisplayer
        PJDisplayer pjDisplayer = FindObjectOfType<PJDisplayer>();

        string identifier = pjDisplayer.GetCurrentCharacterIdentifier();
        int price = pjDisplayer.GetCurrentCharacterPrice();
        bool isDeluxeCharacter = pjDisplayer.GetCurrentCharacterIsADeluxe();

        if (isDeluxeCharacter) return;

        // Comprobar si el usuario tiene las monedas necesarias para comprar el item. En caso contrario, se mostrara el mensaje de Deluxe
        if (price <= singleModeSession.GetCoins())
        {
            // Revisar si personaje esta desbloqueado
            if (UnlockedCharacterController.IsUnlocked(identifier)) return;

            // Gastar monedas y guardar los datos del jugador
            singleModeSession.SpendCoins(price);
            singleModeSession.SyncInitialCoins();
            GameController.Save(singleModeSession.GetCoins());
            UnlockedCharacterController.NewUnlockNotShow(identifier);

            // Reproducir sonido de monedas
            MasterSFXPlayer._player.Coins();

            // Actualizar el UI de monedas
            coinsDisplayer.SetLabel(singleModeSession.GetCoins());
            pjDisplayer.UpdateDisplayer();
        }
        else
        {
            // Reproducir sonido de error
            MasterSFXPlayer._player.Error();
            ShowDeluxeOverlay();
        }

    }

    // Mostrar el ovelay de JanKenUpDeluxe
    public void ShowDeluxeOverlay()
    {
        // Si el jugador ya es deluxe o ya se mostro el al menos una vez el deluxe, no seguir
        if (GameController.IsDeluxe() || GameController.ShopCharacterDeluxeOverlayReady()) return;
        GameController.SetShopCharacterDeluxeOverlayReady();
        StopAllCoroutines();
        Instantiate(deluxeOverlayPrefab);
    }
}
