using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Purchasing;
using DigitalRuby.Tween;

public class JanKenShopItem : MonoBehaviour
{
    [Header("Item Detail")]
    [SerializeField] protected GameObject itemDetail;
    [SerializeField] string tableName = "JanKenShop";
    [SerializeField] protected string productID = "";

    [Header("Elements")]
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] protected TextMeshProUGUI price;
    [SerializeField] TextMeshProUGUI boughtText;
    [SerializeField] GameObject discountObject;
    [SerializeField] TextMeshProUGUI discountText;

    [Header("Containers")]
    [SerializeField] GameObject priceContainer;
    [SerializeField] GameObject boughtContainer;

    [Header("Animations")]
    [SerializeField] [Range(0.1f, 1)] float fadeInTime = 0.5f;

    // Componentes recurrentes
    Button button;
    CanvasGroup canvasGroup;

    // Estado del item
    JankenUp.Deluxe.States currentState = JankenUp.Deluxe.States.available;
    protected bool isAvailable = true;

    // Use this for initialization
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        canvasGroup = GetComponent<CanvasGroup>();
        Localize();
        UpdateCurrentFont();

        // Establecer el precio
        SetPrice();

        // Establecer disponibilidad
        SetIsAvailable();

        // Establecer descuento
        SetDiscount();

        // FadeIn
        Show();
    }

    /// <summary>
    /// Realizar el fade-in del item
    /// </summary>
    void Show()
    {

        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        gameObject.Tween(string.Format("FadeCanvasGroup{0}", gameObject.GetInstanceID()), 0, 1, fadeInTime, TweenScaleFunctions.QuadraticEaseInOut, updateAlpha);
    }

    /// <summary>
    /// Apertura del detalle del personaje
    /// </summary>
    void OnClick()
    {
        ShowDetail();
    }

    /// <summary>
    /// Mostrar los detalles del item
    /// </summary>
    protected virtual void ShowDetail(){}

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(title, tableName, productID);
        LocalizationHelper.Translate(boughtText, JankenUp.Localization.tables.JanKenShop.tableName, JankenUp.Localization.tables.JanKenShop.Keys.already_bought);
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        title.font = mainFont;
        boughtText.font = mainFont;

        // Cambiar el material
        Material mainMaterial = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        title.fontSharedMaterial = mainMaterial;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        title.fontStyle = style;
        boughtText.fontStyle = style;
    }

    /// <summary>
    /// Establece el precio del producto
    /// </summary>
    protected virtual void SetPrice() {}

    /// <summary>
    /// Revisar disponibilidad del item para ser comprado por el jugador
    /// </summary>
    protected virtual void SetIsAvailable(){}

    /// <summary>
    /// Realizar el cambio de disponibilidad del pack
    /// </summary>
    /// <param name="newState"></param>
    public virtual void ChangeState(JankenUp.Deluxe.States newState)
    {
        currentState = newState;

        // Actualizar la apariencia del elemento en base a su estado
        switch (currentState)
        {
            case JankenUp.Deluxe.States.available:
                priceContainer.SetActive(true);
                boughtContainer.SetActive(false);
                break;
            case JankenUp.Deluxe.States.bought:
                priceContainer.SetActive(false);
                boughtContainer.SetActive(true);
                discountObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Revisar si se debe cambiar el estado al coincidir la compra con la ID del item
    /// </summary>
    /// <param name="product"></param>
    public void PurchaseComplete(Product product)
    {
        UpdateBasedOnPurchase(product);
    }

    /// <summary>
    /// Actualizacion del elemento en base a una compra
    /// </summary>
    /// <param name="product"></param>
    protected virtual void UpdateBasedOnPurchase(Product product) {
        if (product.definition.id == productID)
        {
            isAvailable = false;
            ChangeState(JankenUp.Deluxe.States.bought); 
        }
    }

    /// <summary>
    /// Comprobacion del estado de validez del pack
    /// </summary>
    protected virtual void OnEnable()
    {
        SetIsAvailable();
    }

    /// <summary>
    /// Revision en la configuracion remota por si existe un descuento del producto
    /// </summary>
    protected virtual void SetDiscount()
    {
        int discount = JanKenShopJSON.GetDiscount(productID);
        if(discount >= 1 && currentState == JankenUp.Deluxe.States.available)
        {
            discountObject.SetActive(true);
            discountText.text = string.Format("{0}%", discount);
        }
        else
        {
            discountObject.SetActive(false);
        }
    }
}