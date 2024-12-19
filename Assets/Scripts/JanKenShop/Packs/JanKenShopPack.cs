using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using static JankenUp.Deluxe;

public class JanKenShopPack : JanKenShopItem
{
    Pack selectedPack;

    /// <summary>
    /// Carga del pack asociado al ID
    /// </summary>
    void LoadPack()
    {
        if (selectedPack != null) return;
        selectedPack = JankenUp.Deluxe.packsList.Find(p => p.productID == productID);
    }

    /// <summary>
    /// Establece el precio del pack
    /// </summary>
    protected override void SetPrice()
    {
        base.SetPrice();

        // Obtener el pack correspondiente a la key
        LoadPack();
        if (selectedPack != null && IAPController._this)
        {
            price.text = IAPController._this.PackPrice(selectedPack);
        }
    }

    /// <summary>
    /// Revisar la disponibilidad el pack
    /// A diferencia de los items normales, se considera si el jugador cuenta con el Deluxe original
    /// </summary>
    protected override void SetIsAvailable()
    {
        // Obtener el pack correspondiente a la key
        LoadPack();
        if (selectedPack != null && IAPController._this)
        {
            isAvailable = IAPController._this.PackIsAvailable(selectedPack);
        }

        // Cambiar el estado segun corresponda
        ChangeState(isAvailable? States.available : States.bought);

    }

    /// <summary>
    /// Abrir el pack con el detalle del pack
    /// </summary>
    protected override void ShowDetail()
    {
        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();

        // Iniciar el detalle
        LoadPack();
        if (itemDetail && selectedPack != null)
        {
            sceneController.SwitchUI(OpenDetail());
        }
    }

    IEnumerator OpenDetail()
    {
        JanKenShopPackOverlay jksOverlay = Instantiate(itemDetail).GetComponent<JanKenShopPackOverlay>();
        jksOverlay.Setup(selectedPack, isAvailable);
        yield return null;
    }

    /// <summary>
    /// Override para agregar el contenido al dispositivo del jugador
    /// </summary>
    /// <param name="product"></param>
    protected override void UpdateBasedOnPurchase(Product product)
    {
        base.UpdateBasedOnPurchase(product);
        if (product.definition.id == productID)
        {
            // Agregar el pack comprado al listado del jugador
            GameController.AddPlayerPack(selectedPack);

            // Indicar que es un jugador Deluxe y adjuntar el pack comprado
            GameController.SetIsDeluxe();
        }
    }
}
