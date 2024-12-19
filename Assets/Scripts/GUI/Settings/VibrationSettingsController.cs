using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VibrationSettingsController : MonoBehaviour, SettingsOptionInterface
{
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI vibrateLabel;
    [SerializeField] Text vibrateYesLabel;
    [SerializeField] Text vibrateNoLabel;

    // CanvasGroup
    CanvasGroup leftButtonCanvasGroup;
    CanvasGroup rightButtonCanvasGroup;

    private void Start(){
        leftButton.onClick.AddListener(delegate { Decrease(); });
        rightButton.onClick.AddListener(delegate { Increase(); });
        leftButtonCanvasGroup = leftButton.GetComponent<CanvasGroup>();
        rightButtonCanvasGroup = rightButton.GetComponent<CanvasGroup>();
        if (PlayersPreferencesController.CanVibrate()) Decrease();
        else Increase();
        Localize();
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
    }

    /// <summary>
    /// Seleccionar opcion izquierda
    /// </summary>
    /// <param name="times"></param>
    public void Decrease(float times = 1) {
        PlayerPrefs.SetInt(PlayersPreferencesController.VIBRATE, 1);
        leftButtonCanvasGroup.alpha = 1;
        rightButtonCanvasGroup.alpha = .5f;
    }

    /// <summary>
    /// Seleccionar opcion derecha
    /// </summary>
    /// <param name="times"></param>
    public void Increase(float times = 1) {
        PlayerPrefs.SetInt(PlayersPreferencesController.VIBRATE, 0);
        leftButtonCanvasGroup.alpha = .5f;
        rightButtonCanvasGroup.alpha = 1;
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(vibrateLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.vibration);
        LocalizationHelper.Translate(vibrateYesLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.yes);
        LocalizationHelper.Translate(vibrateNoLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.no);
        UpdateCurrentFont();
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        vibrateLabel.font = mainFont;

        Font plainFont = FontManager._mainManager.GetPlainFont();
        vibrateYesLabel.font = plainFont;
        vibrateNoLabel.font = plainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        vibrateLabel.fontStyle = style;
    }

    /// <summary>
    /// Remover suscripciones
    /// </summary>
    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localize;
    }

}