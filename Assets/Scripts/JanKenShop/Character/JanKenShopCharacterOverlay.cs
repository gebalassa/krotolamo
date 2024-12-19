using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class JanKenShopCharacterOverlay : OverlayObject
{
    [Header("Containers")]
    [SerializeField] GameObject characterContainer;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI detailText;
    [SerializeField] Text description;

    [Header("Others")]
    [SerializeField] Button closeButton;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] JKButton playButton;
    [SerializeField] GameObject textScrollViewContent;
    [SerializeField] GameObject textScrollVerticalLayout;
    [SerializeField] JanKenCard jankenCard;
    [SerializeField] JKButton buyButton;
    [SerializeField] CoinsCurrencyDisplayer coinsDisplayer;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] [Range(0, 1000f)] float joystickSpeed = 0f;

    [Header("RRSS")]
    [SerializeField] bool rrssButtons = true;
    [SerializeField] GameObject rrssContainer;
    [SerializeField] Button instagramButton;
    [SerializeField] Button tiktokButton;
    [SerializeField] Button twitterButton;
    [SerializeField] Button youtubeButton;
    [SerializeField] Button webtoonButton;
    [SerializeField] Button medibangButton;
    [SerializeField] JKButton shareButton;

    // Data asociada al PJ
    string identifier = "";
    string realIdentifier = "";
    string linkToPlatform = "";
    string url = "";
    bool unlocked = false;
    int characterIndex = 0;
    CharacterConfiguration characterConfiguration;

    // Corutinas
    Coroutine coroutineDescription;

    // Delegados para cuando se cambie de personaje
    public delegate void OnBuyDelegate(string identifier);
    public static event OnBuyDelegate onBuyDelegate;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        // Anadir el listener de cierre
        closeButton.onClick.AddListener(Close);
        prevButton.onClick.AddListener(PrevCharacter);
        nextButton.onClick.AddListener(NextCharacter);
        playButton.onClickDelegate += SelectCharacter;

        // Asociar botones de RRSS
        if (rrssButtons)
        {
            instagramButton.onClick.AddListener(delegate { ClickRRSS("instagram"); });
            tiktokButton.onClick.AddListener(delegate { ClickRRSS("tiktok"); });
            twitterButton.onClick.AddListener(delegate { ClickRRSS("twitter"); });
            youtubeButton.onClick.AddListener(delegate { ClickRRSS("youtube"); });
            webtoonButton.onClick.AddListener(delegate { ClickRRSS("webtoon"); });
            medibangButton.onClick.AddListener(delegate { ClickRRSS("medibang"); });
            shareButton.onClickDelegate += ShareCard;
        }

        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;

        // Habilitar compra de personajes
        buyButton.onClickDelegate += BuyCharacter;
    }

    /// <summary>
    /// Configuracion del overlay del PJ
    /// </summary>
    /// <param name="characterConfiguration"></param>
    /// <param name="animate"></param>
    public void Setup(CharacterConfiguration characterConfiguration, bool animate = true)
    {
        // Reproducir un sonido de victoria
        MasterSFXPlayer._player.PlayOneShot(characterConfiguration.SFXWin());
        this.characterConfiguration = characterConfiguration;

        // Agregar una instancia de objeto al contenedor de personajes
        jankenCard.Setup(characterConfiguration, animate);

        // Obtener el identificador del personaje
        identifier = characterConfiguration.GetParentIdentifier();
        realIdentifier = characterConfiguration.GetIdentifier();
        unlocked = characterConfiguration.IsUnlocked();
        characterIndex = CharacterPool.Instance.GetIndexByIdentifier(realIdentifier);

        // Compra
        if (characterConfiguration.IsUnlocked())
        {
            if (buyButton.isActiveAndEnabled) buyButton.Hide();
        }
        else
        {
            buyButton.gameObject.SetActive(true);
            buyButton.GetText().text = characterConfiguration.GetPrice().ToString();
        }

        // Configurar los botones de redes sociales disponibles
        if (rrssButtons) SetupRRSS();
        else
        {
            rrssContainer.SetActive(false);
            shareButton.gameObject.SetActive(false);
        }

        // Traducir al idioma que corresponda los distintos elementos
        Localize();

        // Configurar boton
        playButton.SetBW(!characterConfiguration.IsUnlocked());
    }

    /// <summary>
    /// Configuracion de RRSS para el personaje
    /// </summary>
    private void SetupRRSS()
    {
        instagramButton.gameObject.SetActive(characterConfiguration.GetInstagram() != "");
        tiktokButton.gameObject.SetActive(characterConfiguration.GetTiktok() != "");
        twitterButton.gameObject.SetActive(characterConfiguration.GetTwitter() != "");
        youtubeButton.gameObject.SetActive(characterConfiguration.GetYoutube() != "");
        webtoonButton.gameObject.SetActive(characterConfiguration.GetWebtoon() != "");
        medibangButton.gameObject.SetActive(characterConfiguration.GetMedibang() != "");

        // Configurar el boton de compartir
        /*if (characterConfiguration.IsUnlocked()) shareButton.gameObject.SetActive(true);
        else if (shareButton.isActiveAndEnabled) shareButton.Hide();*/
    }

    /// <summary>
    /// Apertura del link asociado al personaje
    /// </summary>
    /// <param name="identifier"></param>
    private void ClickRRSS(string identifier = "")
    {
        string url = "";
        switch (identifier) {
            case "instagram":
                url = characterConfiguration.GetInstagram();
                break;
            case "tiktok":
                url = characterConfiguration.GetTiktok();
                break;
            case "twitter":
                url = characterConfiguration.GetTwitter();
                break;
            case "youtube":
                url = characterConfiguration.GetYoutube();
                break;
            case "webtoon":
                url = characterConfiguration.GetWebtoon();
                break;
            case "medibang":
                url = characterConfiguration.GetMedibang();
                break;
        }
        if(url != "") Application.OpenURL(url);
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        // Actualizar la fuente
        UpdateCurrentFont();

        LocalizationHelper.Translate(title, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.characterDetails);

        // Si personaje no esta desbloqueado, incluir al inicio el texto de desbloqueo
        description.text = "";
        if (coroutineDescription != null) StopCoroutine(coroutineDescription);
        if (!unlocked && isActiveAndEnabled) {
            coroutineDescription = StartCoroutine(DescriptionWithUnlockText());
        }
        else
        {
            if (characterConfiguration.GetDescriptionFromConfiguration())
            {
                description.text = characterConfiguration.GetDescriptionForLang(LocalizationSettings.SelectedLocale.Identifier.Code);
            }
            else
            {
                LocalizationHelper.Translate(description, JankenUp.Localization.tables.Characters.tableName, string.Format("{0}_description", identifier));
            }
        }
        LocalizationHelper.Translate(detailText, JankenUp.Localization.tables.JanKenShop.tableName, JankenUp.Localization.tables.JanKenShop.Keys.details);

        playButton.UpdateCurrentFont();
        LocalizationHelper._this.TranslateThis(playButton.GetText(), JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.select);

        // Ajustar el contenido
        if (isActiveAndEnabled) StartCoroutine(AdjustContentHeight());
    }

    /// <summary>
    /// Obtencion del texto del personaje junto con su manera de ser desbloqueado
    /// </summary>
    /// <returns></returns>
    private IEnumerator DescriptionWithUnlockText()
    {
        bool descriptionFromConfiguration = characterConfiguration.GetDescriptionFromConfiguration();

        List<string> tables = new List<string>() {
            JankenUp.Localization.tables.Achievements.tableName,
            JankenUp.Localization.tables.Achievements.tableName,
            JankenUp.Localization.tables.Achievements.tableName
        };
        List<string> keys = new List<string>() {
            JankenUp.Localization.tables.Achievements.Keys.unlockMethod,
            identifier,
            JankenUp.Localization.tables.Achievements.Keys.doubleEnter
        };

        if (!descriptionFromConfiguration)
        {
            tables.Add(JankenUp.Localization.tables.Characters.tableName);
            keys.Add(string.Format("{0}_description", identifier));
        }   

        // Ir solicitando cada traduccion de elementos, asignarlo a un string y luego pasar ese texto al campo de beneficios
        yield return LocalizationHelper._this.TranslateGetText(description, tables, keys);
        description.text = description.text.Replace("__xx__ ", "\n\n");

        if (descriptionFromConfiguration)
        {
            description.text += characterConfiguration.GetDescriptionForLang(LocalizationSettings.SelectedLocale.Identifier.Code);
        }

        if (isActiveAndEnabled) StartCoroutine(AdjustContentHeight());
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

        // Fuente plana
        Font plainFont = FontManager._mainManager.GetPlainFont();
        description.font = plainFont;

        // Cambiar el material
        Material mainMaterial = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        title.fontSharedMaterial = mainMaterial;
        detailText.fontSharedMaterial = mainMaterial;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        title.fontStyle = style;
        detailText.fontStyle = style;
    }

    /// <summary>
    /// Realiza el ajuste del alto para el contenedor de opciones
    /// </summary>
    /// <returns></returns>
    IEnumerator AdjustContentHeight()
    {
        yield return new WaitForEndOfFrame();
        if (textScrollViewContent)
        {
            // Obtener los datos para determinar el alto. Se debe sumar el valor de todos los hijos
            float childHeight = 0;
            foreach (RectTransform rectChild in textScrollVerticalLayout.transform)
            {
                childHeight += rectChild.rect.height;
            }

            VerticalLayoutGroup gridLayout = textScrollVerticalLayout.GetComponent<VerticalLayoutGroup>();
            float pageHeight = childHeight + gridLayout.padding.top + gridLayout.padding.bottom;

            // Actualizar el elementos
            RectTransform rectVerticalLayout = ((RectTransform)textScrollVerticalLayout.transform);
            rectVerticalLayout.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pageHeight);

            RectTransform rectScroll = ((RectTransform)textScrollVerticalLayout.transform);
            rectScroll.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pageHeight);
        }
    }

    /// <summary>
    /// Mostrar la informacion del personaje anterior al actual
    /// </summary>
    private void PrevCharacter() {
        CharacterConfiguration newCharacter = CharacterPool.Instance.GetCharacterInfoByIndex(--characterIndex, true);
        Setup(newCharacter);
    }

    /// <summary>
    /// Mostrar la informacion del personaje siguiente al actual
    /// </summary>
    private void NextCharacter() {
        CharacterConfiguration newCharacter = CharacterPool.Instance.GetCharacterInfoByIndex(++characterIndex, true);
        Setup(newCharacter);
    }

    /// <summary>
    /// Seleccionar el personaje actual. Volver a pantalla anterior
    /// </summary>
    public void SelectCharacter()
    {
        if (!characterConfiguration.IsUnlocked())
        {
            MasterSFXPlayer._player.Error();
            return;
        }
        GameController.SaveCharacterIdentifier(realIdentifier);
        RequestClose();
    }

    /// <summary>
    /// Realizar la compra del personaje actualmente seleccionado
    /// </summary>
    private void BuyCharacter()
    {
        SingleModeSession singleModeSession = FindObjectOfType<SingleModeSession>();
        if (characterConfiguration.IsUnlocked() || !singleModeSession) return;
        if (characterConfiguration.GetPrice() <= singleModeSession.GetCoins())
        {
            // Gastar monedas y guardar los datos del jugador
            singleModeSession.SpendCoins(characterConfiguration.GetPrice());
            singleModeSession.SyncInitialCoins();
            GameController.Save(singleModeSession.GetCoins());
            UnlockedCharacterController.NewUnlockNotShow(characterConfiguration.GetIdentifier());
            OnAddCharacter(characterConfiguration.GetIdentifier());

            // Reproducir sonido de monedas
            MasterSFXPlayer._player.Coins();

            // Actualizar el UI de monedas
            if (coinsDisplayer) coinsDisplayer.OnCoinsCurrencyUpdate();
        }
        else
        {
            // Reproducir sonido de error
            MasterSFXPlayer._player.Error();
        }
    }

    /// <summary>
    /// Actualizacion del contenido cuando un nuevo personaje fue anadido al inventario
    /// </summary>
    /// <param name="identifier"></param>
    void OnAddCharacter(string identifier)
    {
        // Actualizar el personaje en UI si es el agregado
        if (characterConfiguration.GetIdentifier() == identifier)
        {
            Setup(characterConfiguration, false);
            if (onBuyDelegate != null) onBuyDelegate(identifier);
        }
    }

    /// <summary>
    /// Compartir tarjeta de personaje
    /// </summary>
    void ShareCard()
    {
        if(jankenCard.IsAnimationReady()) jankenCard.ShareScreenshot();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Ajustar el contenido
        StartCoroutine(AdjustContentHeight());
    }

    public void OnDestroy()
    {
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;
        playButton.onClickDelegate -= SelectCharacter;
        buyButton.onClickDelegate -= BuyCharacter;
        shareButton.onClickDelegate -= ShareCard;
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    public override bool OnJoystick(JoystickAction action, int playerIndex)
    {
        switch (action)
        {
            case JoystickAction.Left:
                GameController.SimulateClick(prevButton);
                break;
            case JoystickAction.Right:
                GameController.SimulateClick(nextButton);
                break;
            case JoystickAction.Up:
                scrollRect.velocity = new Vector2(0, -joystickSpeed);
                break;
            case JoystickAction.Down:
                scrollRect.velocity = new Vector2(0, joystickSpeed);
                break;
            case JoystickAction.R:
            case JoystickAction.A:
                SelectCharacter();
                break;
            case JoystickAction.B:
                BuyCharacter();
                break;
        }
        return false;
    }
    #endregion

}