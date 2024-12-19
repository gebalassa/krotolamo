using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using TMPro;

public class LanguageController : MonoBehaviour, SettingsOptionInterface
{

    [Header("Lenguajes Disponibles")]
    [SerializeField] List<LangFlag> langFlags = new List<LangFlag>();
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] RectTransform flagsContainer;

    [Header("Times")]
    [SerializeField] float timeToFade = .5f;
    [SerializeField] float timeToChange = 3f;

    [Header("Otros")]
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] bool standalone = false;

    bool firstTime = true;
    string currentLangCode = "en";

    // Eventos
    public delegate void OnLanguageChange();
    public static event OnLanguageChange onLanguageChangeDelegate;

    // Utiles
    int focusIndex = 0;
    CanvasGroup mainCanvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        // Marcar el idioma seleccionado
        StartCoroutine(WaitForLocalization());
        if(standalone) JoystickSupport.onJoystick += OnJoystick;
        
        Localize();
        // Suscribirse al cambio de idioma
        onLanguageChangeDelegate += Localize;
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(label, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.language);
        UpdateCurrentFont();
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        label.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        label.fontStyle = style;
    }

    // Esperar hasta que la localizacion este cargada
    private IEnumerator WaitForLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;

        foreach(LangFlag langFlag in langFlags)
        {
            langFlag.GetComponent<Button>().onClick.AddListener(delegate { OnFlagClicked(langFlag.GetLangCode(), true); });
        }

        OnFlagClicked(LocalizationSettings.SelectedLocale.Identifier.Code, false);
        firstTime = false;
    }

    /// <summary>
    /// Cambio de idioma
    /// </summary>
    /// <param name="langCode"></param>
    /// <param name="notifyListeners"></param>
    public void OnFlagClicked(string langCode, bool notifyListeners = true, bool close = false)
    {
        // Cambiar el idioma base
        UnityEngine.Localization.Locale locale = LocalizationSettings.AvailableLocales.GetLocale(langCode);
        if (locale == null || langCode != locale.Identifier) return;

        currentLangCode = langCode;
        focusIndex = langFlags.FindIndex(lf => lf.GetLangCode() == currentLangCode);
        ScrollTo(langFlags[focusIndex].transform.localPosition);

        LocalizationSettings.SelectedLocale = locale;
        PlayersPreferencesController.SetCurrentLanguage(langCode);

        foreach (LangFlag langFlag in langFlags)
        {
            langFlag.GetComponent<CanvasGroup>().alpha = currentLangCode == langFlag.GetLangCode() ? 1 : .5f;
        }

        // Indicar al controlador de opciones que debe actualizar las fuentes
        OptionsAdjustment optionsAdjustment = FindObjectOfType<OptionsAdjustment>();
        if (optionsAdjustment) optionsAdjustment.UpdateCurrentFont();

        // Si existe un overlay, llamar y cerrar
        LanguageOverlay languageOverlay = FindObjectOfType<LanguageOverlay>();
        if (languageOverlay && !firstTime && close) languageOverlay.Close();

        // Llamar a los suscritos al evento de cambio de idioma
        if (onLanguageChangeDelegate != null && notifyListeners) onLanguageChangeDelegate();
    }

    /// <summary>
    /// Polimorfismo para llamar a eventos
    /// </summary>
    /// <param name="langCode"></param>
    public void OnFlagClicked(string langCode, bool close = false)
    {
        OnFlagClicked(langCode, true, close);
    }

    /// <summary>
    /// Cambiar al anterior idioma en base a la posicion del focus
    /// </summary>
    public void Decrease(float times = 1)
    {
        int timesInt = (int)times;
        if (focusIndex <= 0 || (focusIndex - timesInt) < 0) return;
        LangFlag langFlag = langFlags[focusIndex - timesInt];
        OnFlagClicked(langFlag.GetLangCode(), false);
    }

    /// <summary>
    /// Cambiar al siguiente idioma en base a la posicion del focus
    /// </summary>
    public void Increase(float times = 1)
    {
        int timesInt = (int)times;
        if (focusIndex >= langFlags.Count - 1 || (focusIndex + timesInt) > langFlags.Count - 1) return;
        LangFlag langFlag = langFlags[focusIndex + timesInt];
        OnFlagClicked(langFlag.GetLangCode(), false);
    }

    /// <summary>
    /// Se centra el scroll en el elemento indicado
    /// </summary>
    /// <param name="itemPosition"></param>
    private void ScrollTo(Vector2 itemPosition)
    {
        float horizontalNormalizedPosition = (float)System.Math.Round(Mathf.Abs(itemPosition.x / flagsContainer.rect.width), 1);
        System.Action<ITween<float>> updateScroll = (t) =>
        {
            if (scrollRect) scrollRect.horizontalNormalizedPosition = t.CurrentValue;
        };

        float toNormalizedPosition = horizontalNormalizedPosition;
        scrollRect.gameObject.Tween(string.Format("Scroll{0}", scrollRect.GetInstanceID()), scrollRect.horizontalNormalizedPosition, toNormalizedPosition, .1f, TweenScaleFunctions.QuadraticEaseOut, updateScroll);
    }

    // Mostrar/Ocultar los elementos
    public void Fade(bool show)
    {
        if (!mainCanvasGroup) mainCanvasGroup = GetComponent<CanvasGroup>();

        // Cambio de opacidad de elemento general
        System.Action<ITween<float>> UpdateContainerAlpha = (t) =>
        {
            if (mainCanvasGroup) mainCanvasGroup.alpha = t.CurrentValue;
        };

        gameObject.Tween("FadeActive", (show? 0 : 1), (!show ? 0 : 1), timeToFade, TweenScaleFunctions.QuadraticEaseInOut, UpdateContainerAlpha);
    }

    /// <summary>
    /// Remover suscripciones
    /// </summary>
    private void OnDestroy()
    {
        if(standalone) JoystickSupport.onJoystick -= OnJoystick;
        onLanguageChangeDelegate -= Localize;
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    /// <returns>Indica si la accion puede continuar a los hijos</returns>
    protected virtual bool OnJoystick(JoystickAction action, int playerIndex)
    {
        switch (action)
        {
            case JoystickAction.Left:
                Decrease();
                break;
            case JoystickAction.Right:
                Increase();
                break;
            case JoystickAction.Up:
                Decrease(langFlags.Count / 2);
                break;
            case JoystickAction.Down:
                Increase(langFlags.Count / 2);
                break;
            case JoystickAction.X:
            case JoystickAction.A:
            case JoystickAction.Y:
            case JoystickAction.B:
                LangFlag langFlag = langFlags[focusIndex];
                OnFlagClicked(langFlag.GetLangCode(), true);
                break;
        }

        return false;
    }
    #endregion

}
