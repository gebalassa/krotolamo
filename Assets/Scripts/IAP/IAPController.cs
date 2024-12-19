using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPController : MonoBehaviour,IStoreListener
{
    private IStoreController controller;
    private IExtensionProvider extensions;

    // Mantener el singleton
    public static IAPController _this;

    // Indicacion de si fue inicializado el controlador (Caso exitoso o no exitoso)
    public static bool isInitialized = false;

    // Servicios de Unity
    public string environment = "production";

    // Singleton
    protected void Awake()
    {
        int length = FindObjectsOfType<IAPController>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
            _this = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        // Inicializar los servicios de Unity
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception){
            Debug.LogError(exception);
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(JankenUp.Deluxe.legacyProduct, ProductType.NonConsumable);
        
        // Packs
        foreach (JankenUp.Deluxe.Pack pack in JankenUp.Deluxe.packsList)
        {
            builder.AddProduct(pack.productID, ProductType.NonConsumable);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;

        // Marcar Deluxe si existe
        if (IsDeluxe()) GameController.SetIsDeluxe();

        isInitialized = true;
    }
     
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        isInitialized = true;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        isInitialized = true;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        return PurchaseProcessingResult.Complete;
    }

    // Comprobar que al menos un paquete de la version deluxe haya sido comprado
    public bool IsDeluxe() {
        bool isDeluxe = false;

        // Analizar si es una cuenta Deluxe clasica
        Product product = null;
        bool legacyDeluxe = IsLegacyDeluxe();
        isDeluxe = legacyDeluxe;

        // En caso de no serlo, analizar los packs
        foreach (JankenUp.Deluxe.Pack pack in JankenUp.Deluxe.packsList)
        {
            product = this.controller != null ? this.controller.products.WithID(pack.productID) : null;
            bool purchasedPack = product != null ? product.hasReceipt : false;
            isDeluxe = isDeluxe || purchasedPack;
            if (purchasedPack || legacyDeluxe) GameController.AddPlayerPack(pack);
        }

        return isDeluxe;
    }

    /// <summary>
    /// Revision de si el jugador cuenta con el legacy deluxe
    /// </summary>
    /// <returns></returns>
    private bool IsLegacyDeluxe()
    {
        // Analizar si es una cuenta Deluxe clasica
        Product product = this.controller != null ? this.controller.products.WithID(JankenUp.Deluxe.legacyProduct) : null;
        bool legacyDeluxe = product != null ? product.hasReceipt : false;
        return legacyDeluxe;
    }

    // Obtener el precio de un pack
    public string PackPrice(JankenUp.Deluxe.Pack pack)
    {
        Product product = this.controller != null ? this.controller.products.WithID(pack.productID) : null;
        return product != null ? product.metadata.localizedPriceString : "";
    }

    /// <summary>
    /// Obtencion del precio de un elemento en especifico
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public string ItemPrice(string itemId)
    {
        Product product = this.controller != null ? this.controller.products.WithID(itemId) : null;
        return product != null ? product.metadata.localizedPriceString : "";
    }

    /// <summary>
    /// Revision de la disponibilidad de un pack para ser comprado
    /// </summary>
    /// <returns></returns>
    public bool PackIsAvailable(JankenUp.Deluxe.Pack pack)
    {
        // Revisar si el usuario es parte del legacy
        if (IsLegacyDeluxe()) return false;

        // Revisar si el jugador ya cuenta con el pack
        if (GameController.PlayerHasThisPack(pack.productID)) return false;

        // Revisar si existe un comprobante de transaccion
        Product product = this.controller != null ? this.controller.products.WithID(pack.productID) : null;
        bool isAvailable = product != null ? !product.hasReceipt : false;

        return isAvailable;
    }

    // Revisar si ya fue inicializado o no
    public static bool IsInitialized()
    {
        return isInitialized;
    }
}