using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static JankenUp.Deluxe;
using TMPro;

public class JanKenShopPackOverlay : OverlayObject
{
    [Header("Setup")]
    [SerializeField] string tableName = "JanKenShop";
    [SerializeField] protected string productID = "";
    [SerializeField] List<string> benefitsTable;
    [SerializeField] List<string> benefitsKey;

    [Header("Containers")]
    [SerializeField] GameObject elementsContainer;
    [SerializeField] GameObject illustrationsContainer;

    [Header("Pack Content")]
    [SerializeField] List<GameObject> packContent;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI detailText;
    [SerializeField] TextMeshProUGUI buyButtonText;
    [SerializeField] Text buyButtonPrice;
    [SerializeField] Text detailDescription;

    [Header("Others")]
    [SerializeField] Button closeButton;
    [SerializeField] Button buyButton;
    [SerializeField] GameObject confettiParticle;
    [SerializeField] [Range(2, 10)]  float timeBetweenIllustrations = 4f;

    // Para ilustraciones
    Image[] illustrations;
    List<RectTransform> rectTransforms = new List<RectTransform>();
    List<float> rectTransformOriginalX = new List<float>();
    Coroutine changeIllustrationCoroutine;

    // Utiles
    Pack selectedPack;

    // Para beneficios
    List<string> deluxeBenefits = new List<string>() { "deluxe_benefit_0", "deluxe_benefit_1", "deluxe_benefit_2", "deluxe_benefit_3", "deluxe_benefit_4" };

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        // Ajustar detalles
        FillDetails();

        InstantiateContent();

        // Actualizar la fuente
        UpdateCurrentFont();

        // Anadir el listener de cierre
        closeButton.onClick.AddListener(Close);

        // Iniciar la rutina para cambiar de beneficios
        StartCoroutine(ChangeBenefits());
    }

    /// <summary>
    /// Rellenar los datos relacionaods con el pack
    /// </summary>
    void FillDetails(){}

    /// <summary>
    /// Configuracion del overlay
    /// <param name="available"></param>
    /// </summary>
    public void Setup(Pack pack, bool available)
    {
        selectedPack = pack;
        ChangeAvailability(available);

        // Mostrar el precio del pack
        buyButtonPrice.text = IAPController._this? IAPController._this.PackPrice(selectedPack) : "";

        // Traducir al idioma que corresponda los distintos elementos
        Localize();
    }

    /// <summary>
    /// Cambia el estado del elemento en base a su disponibilidad de compra
    /// </summary>
    /// <param name="available"></param>
    void ChangeAvailability(bool available, float fadeAfter = 0)
    {
        StartCoroutine(ChangeAvailabilityCoroutine(available, fadeAfter));
    }

    /// <summary>
    /// Corutina para evitar errores de deshabilitar el boton antes de completar la compra por parte de la IAP
    /// </summary>
    /// <param name="available"></param>
    /// <param name="fadeAfter"></param>
    /// <returns></returns>
    IEnumerator ChangeAvailabilityCoroutine(bool available, float fadeAfter = 0)
    {
        if(fadeAfter != 0)
        {
            yield return new WaitForSeconds(fadeAfter);
        }
        buyButton.gameObject.SetActive(available);
    }

    /// <summary>
    /// Agregar los distintos elementos del pack al overlay
    /// </summary>
    void InstantiateContent()
    {
        foreach(GameObject gameObject in packContent)
        {
            Instantiate(gameObject, elementsContainer.transform);
        }
    }

    /// <summary>
    /// Llamado cuando la compra es exitosa
    /// </summary>
    public void PurchaseComplete()
    {
        // Sonido de moneditas
        MasterSFXPlayer._player.Coins();

        // Agregar el pack comprado al listado del jugador
        GameController.AddPlayerPack(selectedPack);

        // Indicar que es un jugador Deluxe y adjuntar el pack comprado
        GameController.SetIsDeluxe();

        // Logro de GooglePlay
        if (GooglePlayGamesController._this) GooglePlayGamesController._this.ReportAchievement(JankenUp.Achievements.deluxe, 100f);

        /// TODO: Realizar un metodo general para las escenas y asi mantener solo una llamada de deluxe a la escena activa

        // Si existe en el menu, hacer desaparecer el boton
        MenuController menu = FindObjectOfType<MenuController>();
        if (menu) menu.DeluxeReady();

        // Si existe una sesion, checkear el deluxe
        SingleModeSession sms = FindObjectOfType<SingleModeSession>();
        if (sms) sms.CheckDeluxe();

        // Si se esta en una sesion de juego modo clasico, seguir
        SingleModeController smc = FindObjectOfType<SingleModeController>();
        if (smc) smc.AfterPurchaseDeluxe();

        // Si se esta en una sesion de juego modo survival, seguir
        SurvivalModeController survivalmc = FindObjectOfType<SurvivalModeController>();
        if (survivalmc) survivalmc.AfterPurchaseDeluxe();

        // Si se esta en la seleccion del modo single player
        SingleModeSelectionControllerType smcType = FindObjectOfType<SingleModeSelectionControllerType>();
        if (smcType) smcType.AfterPurchaseDeluxe();

        // Challas
        GeneralParticle confetti = Instantiate(confettiParticle).GetComponent<GeneralParticle>();
        confetti.Play();

        ChangeAvailability(false, 0.1f);
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(title, tableName, productID);
        LocalizationHelper.Translate(detailText, JankenUp.Localization.tables.JanKenShop.tableName, JankenUp.Localization.tables.JanKenShop.Keys.details);
        LocalizationHelper.Translate(buyButtonText, JankenUp.Localization.tables.JanKenShop.tableName, JankenUp.Localization.tables.JanKenShop.Keys.take_my_money);
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        title.font = mainFont;
        detailText.font = mainFont;
        buyButtonText.font = mainFont;

        // Cambios para la descripcion de la tienda
        Font plainFont = FontManager._mainManager.GetPlainFont();
        detailDescription.font = plainFont;

        // Cambiar el material
        Material mainMaterial = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        title.fontSharedMaterial = mainMaterial;
        detailText.fontSharedMaterial = mainMaterial;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        title.fontStyle = style;
        detailText.fontStyle = style;
        buyButtonText.fontStyle = style;
    }

    /// <summary>
    /// Rutina que va mostrando todos los beneficios del pack (Incluye los beneficios del Deluxe)
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeBenefits()
    {
        // Unir los beneficios deluxe con los beneficios del pack
        foreach (string deluxeBenefit in deluxeBenefits)
        {
            benefitsTable.Add(JankenUp.Localization.tables.Deluxe.tableName);
            benefitsKey.Add(deluxeBenefit);
        }

        // Ir solicitando cada traduccion de elementos, asignarlo a un string y luego pasar ese texto al campo de beneficios
        yield return LocalizationHelper._this.TranslateGetText(detailDescription, benefitsTable, benefitsKey);
    }

    /// <summary>
    /// Cambio de la ilustracion asociada al pack
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeIllustration()
    {
        // Obtener los datos de las ilustraciones solo 1 vez
        if(illustrations == null)
        {
            illustrations = illustrationsContainer.GetComponentsInChildren<Image>();
            rectTransforms = new List<RectTransform>();
            rectTransformOriginalX = new List<float>();
        }
    
        // Dejar todas ocultas
        foreach (Graphic illu in illustrations)
        {
            illu.CrossFadeAlpha(0, 0, true);
            rectTransforms.Add(illu.rectTransform);
            rectTransformOriginalX.Add(illu.rectTransform.localPosition.x);
        }

        // Si es solo una, mantener fija en pantalla
        if (illustrations.Length == 1) {
            illustrations[0].CrossFadeAlpha(1, 0, true);
        }
        else if(illustrations.Length > 1)
        {
            // De lo contrario, ir cambiando de ilustracion
            int index = 0;
            int lastIndex = 0;

            while (true)
            {
                if (index >= illustrations.Length) index = 0;

                // Cambiar el color de las ilustaciones
                if(index != lastIndex) illustrations[lastIndex].CrossFadeAlpha(0, timeBetweenIllustrations / 2, false);
                illustrations[index].CrossFadeAlpha(1, timeBetweenIllustrations / 2, false);

                RectTransform rectTransform = rectTransforms[index];
                float originalX = rectTransformOriginalX[index];

                // Mover las ilustraciones
                System.Action<ITween<float>> updatePosition = (t) =>
                {
                    if (rectTransform) rectTransform.localPosition = new Vector3(t.CurrentValue, rectTransform.localPosition.y, rectTransform.localPosition.z);
                };
                
                // Hacer fade in y mostrar elemento
                illustrations[index].gameObject.Tween(string.Format("Move{0}", illustrations[index].GetInstanceID()), originalX - 40, originalX + 40, timeBetweenIllustrations * 2, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

                yield return new WaitForSeconds(timeBetweenIllustrations);
                lastIndex = index++;

            }

        }

        yield return null;
        
    }

    /// <summary>
    /// Habilitacion del elemento
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (changeIllustrationCoroutine != null) StopCoroutine(changeIllustrationCoroutine);
        if (illustrationsContainer.transform.childCount > 0) changeIllustrationCoroutine = StartCoroutine(ChangeIllustration());
    }

    /// <summary>
    /// Deshabilitacion del elemento
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (changeIllustrationCoroutine != null) StopCoroutine(changeIllustrationCoroutine);
    }

}