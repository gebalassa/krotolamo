using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using DigitalRuby.Tween;

public class SettingsOverlay : OverlayObject
{
    [Header("UI")]
    [SerializeField] Image panelImage;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform contentContainer;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] GameObject languageSection;
    [SerializeField] GameObject musicSection;
    [SerializeField] GameObject sfxSection;
    [SerializeField] GameObject controlsSection;
    [SerializeField] GameObject vibrationSection;
    [SerializeField] GameObject refereesfxSection;

    [Header("Available sections")]
    [SerializeField] bool languageActive = true;
    [SerializeField] bool musicActive = true;
    [SerializeField] bool sfxActive = true;
    [SerializeField] bool controlsActive = true;
    [SerializeField] bool vibrationActive = true;
    [SerializeField] bool refereesfxActive = true;

    [Header("Prefabs")]
    [SerializeField] GameObject backCanvas;
    GameObject backCanvasInstance;

    [Header("Others")]
    [SerializeField] Color panelColorIfNoBackground = new Color(0, 0, 0, .5f);

    // Utiles
    List<CanvasGroup> activeOptions = new List<CanvasGroup>();
    int focusIndex = 0;

    protected override void Start()
    {
        base.Start();

        // Si no existe un BackCanvas, agregar
        if (!FindObjectOfType<BackCanvas>()) backCanvasInstance = Instantiate(backCanvas);

        // Deshabilitar secciones
        if (!languageActive) languageSection.SetActive(false);
        if (!musicActive) musicSection.SetActive(false);
        if (!sfxActive) musicSection.SetActive(false);
        if (!controlsActive) controlsSection.SetActive(false);
        if (!vibrationActive) vibrationSection.SetActive(false);
        if (!refereesfxActive) refereesfxSection.SetActive(false);

        foreach (Transform t in contentContainer)
        {
            if (t.gameObject.activeSelf) activeOptions.Add(t.GetComponent<CanvasGroup>());
        }

        Localize();

        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;

        if (JoystickSupport.Instance.SupportActivated()) FocusOption();

        // En caso de no existir fondo, cambiar color de panel
        if (!FindObjectOfType<MovementBackground>()) panelImage.color = panelColorIfNoBackground;
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(title, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.settings);
        UpdateCurrentFont();
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        Material mainMaterial = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        title.font = mainFont;
        title.fontSharedMaterial = mainMaterial;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        title.fontStyle = style;
    }

    /// <summary>
    /// Focalizacion en la opcion del menu
    /// </summary>
    private void FocusOption()
    {
        if (focusIndex < 0 || focusIndex >= activeOptions.Count) focusIndex = 0;
        for (int i = 0; i < activeOptions.Count; i++){
            if(i == focusIndex)
            {
                ScrollTo(activeOptions[i].transform.localPosition);
                activeOptions[i].alpha = 1;
                activeOptions[i].interactable = true;
            }
            else
            {
                activeOptions[i].alpha = 0.5f;
                activeOptions[i].interactable = false;
            }
        }
    }

    /// <summary>
    /// Se centra el scroll en el elemento indicado
    /// </summary>
    /// <param name="itemPosition"></param>
    private void ScrollTo(Vector2 itemPosition)
    {
        float verticalNormalizedPosition = (float)System.Math.Round(Mathf.Abs(itemPosition.y / ((RectTransform)contentContainer).rect.height), 1);
        System.Action<ITween<float>> updateScroll = (t) =>
        {
            if (scrollRect) scrollRect.verticalNormalizedPosition = t.CurrentValue;
        };

        float toNormalizedPosition = 1 - verticalNormalizedPosition;
        scrollRect.gameObject.Tween(string.Format("Scroll{0}", scrollRect.GetInstanceID()), scrollRect.verticalNormalizedPosition, toNormalizedPosition, .1f, TweenScaleFunctions.QuadraticEaseOut, updateScroll);
    }

    /// <summary>
    /// Focus en la opcion anterior segun indice
    /// </summary>
    private void PrevOption() {
        if (focusIndex <= 0) return;
        focusIndex--;
        FocusOption();
    }

    /// <summary>
    /// Focus en la opcion siguiente segun indice
    /// </summary>
    private void NextOption() {
        if (focusIndex >= activeOptions.Count - 1) return;
        focusIndex++;
        FocusOption();
    }

    /// <summary>
    /// Disminuir el valor de la opcion actualmente seleccionada (Depende del tipo opcion actual)
    /// </summary>
    private void DecreaseCurrentOption() {
        activeOptions[focusIndex].GetComponent<SettingsOptionInterface>().Decrease();
    }

    /// <summary>
    /// Aumenta el valor de la opcion actualmente seleccionada (Depende del tipo de opcion actual)
    /// </summary>
    private void IncreaseCurrentOption() {
        activeOptions[focusIndex].GetComponent<SettingsOptionInterface>().Increase();
    }

    /// <summary>
    /// Quitar desuscripciones
    /// </summary>
    private void OnDestroy()
    {
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;
        if (backCanvasInstance != null) Destroy(backCanvasInstance);
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    /// <returns>Indica si la accion puede continuar a los hijos</returns>
    public override bool OnJoystick(JoystickAction action, int playerIndex)
    {
        switch (action)
        {
            case JoystickAction.L:
            case JoystickAction.Escape:
                if (backCanvasInstance) backCanvasInstance.GetComponent<BackCanvas>().Back();
                else
                {
                    BackCanvas backCanvas = FindObjectOfType<BackCanvas>();
                    if (backCanvas) backCanvas.Back();
                }
                break;
            case JoystickAction.Up:
                PrevOption();
                break;
            case JoystickAction.Down:
                NextOption();
                break;
            case JoystickAction.Left:
                DecreaseCurrentOption();
                break;
            case JoystickAction.Right:
                IncreaseCurrentOption();
                break;
        }
        
        return false;
    }
    #endregion
}