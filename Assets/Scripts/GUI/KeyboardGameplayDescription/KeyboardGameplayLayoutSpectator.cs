using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class KeyboardGameplayLayoutSpectator : MonoBehaviour
{
    [Header("Joystick")]
    [SerializeField] TextMeshProUGUI buttonUpLabel;
    [SerializeField] TextMeshProUGUI buttonDownLabel;
    [SerializeField] TextMeshProUGUI buttonLeftLabel;
    [SerializeField] TextMeshProUGUI buttonRightLabel;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI titleOverlay;
    [SerializeField] KeyboardLayout keyboardLayout;

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
    }

    /// <summary>
    /// Mostrar el mensaje en pantalla
    /// </summary>
    public void Localize()
    {
        LocalizationHelper.Translate(titleOverlay, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.button_layout_spectator);
        LocalizationHelper.Translate(buttonUpLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.support_player_left);
        LocalizationHelper.Translate(buttonDownLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.support_player_right);
        LocalizationHelper.Translate(buttonLeftLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.throw_object_player_left);
        LocalizationHelper.Translate(buttonRightLabel, JankenUp.Localization.tables.Joystick.tableName, JankenUp.Localization.tables.Joystick.Keys.throw_object_player_right, null, null, delegate {
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
        buttonUpLabel.font = mainFont;
        buttonDownLabel.font = mainFont;
        buttonLeftLabel.font = mainFont;
        buttonRightLabel.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        buttonUpLabel.fontStyle = style;
        buttonDownLabel.fontStyle = style;
        buttonLeftLabel.fontStyle = style;
        buttonRightLabel.fontStyle = style;
    }

    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localize;
    }

    /// <summary>
    /// Ajuste de contenido
    /// </summary>
    protected  void OnEnable()
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
        keyboardLayout.OnJoystick(action, playerIndex);
        return true;
    }
    #endregion
}
