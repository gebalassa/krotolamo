using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsPositionSettingsController : MonoBehaviour, SettingsOptionInterface
{
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI controlsLabel;
    [SerializeField] Text controlLeftLabel;
    [SerializeField] Text controlRightLabel;

    // CanvasGroup
    CanvasGroup leftButtonCanvasGroup;
    CanvasGroup rightButtonCanvasGroup;

    private void Start(){
        leftButton.onClick.AddListener(delegate { Decrease(); });
        rightButton.onClick.AddListener(delegate { Increase(); });
        leftButtonCanvasGroup = leftButton.GetComponent<CanvasGroup>();
        rightButtonCanvasGroup = rightButton.GetComponent<CanvasGroup>();
        if (PlayerPrefs.GetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, PlayersPreferencesController.CONTROLPOSITIONKEYDEFAULT) == 0) Decrease();
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
        PlayerPrefs.SetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, 0);
        leftButtonCanvasGroup.alpha = 1;
        rightButtonCanvasGroup.alpha = .5f;
    }

    /// <summary>
    /// Seleccionar opcion derecha
    /// </summary>
    /// <param name="times"></param>
    public void Increase(float times = 1) {
        PlayerPrefs.SetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, 1);
        leftButtonCanvasGroup.alpha = .5f;
        rightButtonCanvasGroup.alpha = 1;
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(controlsLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.controls);
        LocalizationHelper.Translate(controlLeftLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.left);
        LocalizationHelper.Translate(controlRightLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.right);
        UpdateCurrentFont();
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        controlsLabel.font = mainFont;

        Font plainFont = FontManager._mainManager.GetPlainFont();
        controlLeftLabel.font = plainFont;
        controlRightLabel.font = plainFont;
    }

    /// <summary>
    /// Remover suscripciones
    /// </summary>
    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localize;
    }

}