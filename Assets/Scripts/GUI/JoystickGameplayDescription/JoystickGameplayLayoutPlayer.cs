using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JoystickGameplayLayoutPlayer : MonoBehaviour
{
    [Header("Joystick")]
    [SerializeField] TextMeshProUGUI buttonXLabel;
    [SerializeField] TextMeshProUGUI buttonALabel;
    [SerializeField] TextMeshProUGUI buttonBLabel;
    [SerializeField] TextMeshProUGUI buttonYLabel;
    [SerializeField] TextMeshProUGUI buttonLLabel;
    [SerializeField] TextMeshProUGUI buttonRLabel;
    [SerializeField] TextMeshProUGUI buttonZLLabel;
    [SerializeField] TextMeshProUGUI buttonZRLabel;
    [SerializeField] TextMeshProUGUI tipHoldToFeintLabel;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI titleOverlay;
    [SerializeField] JoystickLayout joystickLayout;
    [SerializeField] CanvasGroup footer;

    [Header("Others")]
    [SerializeField] bool showFooter = false;

    [Header("Containers")]
    [SerializeField] HorizontalLayoutGroup contentLayout;
    [SerializeField] VerticalLayoutGroup buttonsContainerLayout;
    [SerializeField] List<HorizontalLayoutGroup> buttonsLayout;

    Coroutine adjustContentCoroutine;

    // Util
    int currentLayout = 0;

    protected void Start()
    {
        Localize();
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;

        footer.alpha = showFooter ? 1 : 0;
    }

    /// <summary>
    /// Mostrar el mensaje en pantalla
    /// </summary>
    public void Localize()
    {
        LocalizationHelper.Translate(titleOverlay, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.button_layout_player);

        AttackOptions attackOptions = FindObjectOfType<AttackOptions>(true);
        LocalizationHelper.Translate(buttonXLabel, JankenUp.Localization.tables.Joystick.tableName, attackOptions ? attackOptions.GetTopAttackName() : JankenUp.Localization.tables.Joystick.Keys.guu);
        LocalizationHelper.Translate(buttonALabel, JankenUp.Localization.tables.Joystick.tableName, attackOptions ? attackOptions.GetMiddleAttackName() : JankenUp.Localization.tables.Joystick.Keys.paa);
        LocalizationHelper.Translate(buttonBLabel, JankenUp.Localization.tables.Joystick.tableName, attackOptions ? attackOptions.GetBottomAttackName() : JankenUp.Localization.tables.Joystick.Keys.choki);

        LocalizationHelper.Translate(tipHoldToFeintLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.hold_to_feint);

        LocalizationHelper.Translate(buttonYLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.illuminati);
        LocalizationHelper.Translate(buttonLLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.time_master);
        LocalizationHelper.Translate(buttonRLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.magic_wand);
        LocalizationHelper.Translate(buttonZLLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.super_attack);
        LocalizationHelper.Translate(buttonZRLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.super_attack, null, null, delegate {
            if (isActiveAndEnabled)
            {
                if (adjustContentCoroutine != null) StopCoroutine(adjustContentCoroutine);
                StartCoroutine(AdjustContent());
            }
        });
        UpdateCurrentFont();
    }

    /// <summary>
    /// Ajuste de contenidos de botones. Funciona, por que? No se, pero lo hace
    /// </summary>
    /// <returns></returns>
    private IEnumerator AdjustContent()
    {
        contentLayout.enabled = false;
        buttonsContainerLayout.enabled = false;
        foreach (HorizontalLayoutGroup horizontalLayoutGroup in buttonsLayout)
        {
            horizontalLayoutGroup.enabled = false;
        }
        yield return new WaitForEndOfFrame();
        foreach (HorizontalLayoutGroup horizontalLayoutGroup in buttonsLayout)
        {
            horizontalLayoutGroup.enabled = true;
        }
        yield return new WaitForEndOfFrame();
        buttonsContainerLayout.enabled = true;
        yield return new WaitForEndOfFrame();
        contentLayout.enabled = true;
        yield return new WaitForEndOfFrame();
        contentLayout.enabled = false;
        yield return new WaitForEndOfFrame();
        contentLayout.enabled = true;
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        Material material = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        titleOverlay.font = mainFont;
        titleOverlay.fontSharedMaterial = material;
        buttonXLabel.font = mainFont;
        buttonALabel.font = mainFont;
        buttonBLabel.font = mainFont;
        buttonYLabel.font = mainFont;
        buttonLLabel.font = mainFont;
        buttonRLabel.font = mainFont;
        buttonZLLabel.font = mainFont;
        buttonZRLabel.font = mainFont;
        tipHoldToFeintLabel.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        buttonXLabel.fontStyle = style;
        buttonALabel.fontStyle = style;
        buttonBLabel.fontStyle = style;
        buttonYLabel.fontStyle = style;
        buttonLLabel.fontStyle = style;
        buttonRLabel.fontStyle = style;
        buttonZLLabel.fontStyle = style;
        buttonZRLabel.fontStyle = style;
        tipHoldToFeintLabel.fontStyle = style;
    }

    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localize;
    }

    /// <summary>
    /// Ajuste de contenido
    /// </summary>
    protected void OnEnable()
    {
        if (adjustContentCoroutine != null) StopCoroutine(adjustContentCoroutine);
        StartCoroutine(AdjustContent());
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    public bool OnJoystick(JoystickAction action, int playerIndex)
    {
        joystickLayout.OnJoystick(action, playerIndex);
        return true;
    }
    #endregion
}
