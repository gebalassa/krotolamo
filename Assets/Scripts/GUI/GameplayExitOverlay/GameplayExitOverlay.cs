using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameplayExitOverlay : OverlayObject
{
    [Header("UI")]
    [SerializeField] Image panelImage;
    [SerializeField] TextMeshProUGUI message;
    [SerializeField] JKButton yesButton;
    [SerializeField] JKButton noButton;

    [Header("Others")]
    [SerializeField] Color panelColorIfNoBackground = new Color(0, 0, 0, .5f);
    [SerializeField] List<JoystickUIElement> optionButtons = new List<JoystickUIElement>();
    [SerializeField] [Range(0, 1)] int focusIndex = 1;
    [SerializeField] bool exitGame = false;

    protected override void Start()
    {
        base.Start();

        Localize();

        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;

        // En caso de no existir fondo, cambiar color de panel
        if (!FindObjectOfType<MovementBackground>()) panelImage.color = panelColorIfNoBackground;

        yesButton.onClickDelegate += ExitGameplay;
        noButton.onClickDelegate += Back;

        if(JoystickSupport.Instance.SupportActivated()) FocusSelectedOption();
    }

    /// <summary>
    /// Mostrar el mensaje en pantalla
    /// </summary>
    public void Localize()
    {
        LocalizationHelper.Translate(message, JankenUp.Localization.tables.Online.tableName, JankenUp.Localization.tables.Online.Keys.exit);
        LocalizationHelper.Translate(yesButton.GetText(), JankenUp.Localization.tables.Online.tableName, JankenUp.Localization.tables.Online.Keys.yes);
        LocalizationHelper.Translate(noButton.GetText(), JankenUp.Localization.tables.Online.tableName, JankenUp.Localization.tables.Online.Keys.no);
        UpdateCurrentFont();
        yesButton.UpdateCurrentFont();
        noButton.UpdateCurrentFont();
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        message.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        message.fontStyle = style;
    }

    /// <summary>
    /// Salida a pantalla de resultados
    /// </summary>
    private void ExitGameplay() {
        if (exitGame)
        {
            Application.Quit();
        }
        else
        {
            SingleModeSession singleModeSession = FindObjectOfType<SingleModeSession>();
            if (singleModeSession)
            {
                if (singleModeSession.IsInClassicMode()) SceneLoaderManager.Instance.SingleModeResults();
                else if (singleModeSession.IsInSurvivalMode()) SceneLoaderManager.Instance.SurvivalModeResults();
                else if (singleModeSession.IsInDuelMode()) SceneLoaderManager.Instance.DuelModeSelection();
            }
        }
    }

    /// <summary>
    /// Utilizar la funcion de la scenecontroller
    /// </summary>
    private void Back() {
        SceneController sceneController = FindObjectOfType<SceneController>();
        if (sceneController) sceneController.GoBack();
    }

    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= Localize;
        yesButton.onClickDelegate -= ExitGameplay;
        noButton.onClickDelegate -= Back;
    }

    /// <summary>
    /// Cambiar al anterior idioma en base a la posicion del focus
    /// </summary>
    private void Decrease()
    {
        if (focusIndex <= 0 || (focusIndex - 1) < 0) return;
        focusIndex--;
        FocusSelectedOption();
    }

    /// <summary>
    /// Cambiar al siguiente idioma en base a la posicion del focus
    /// </summary>
    private void Increase()
    {
        if (focusIndex >= optionButtons.Count - 1 || (focusIndex + 1) > optionButtons.Count - 1) return;
        focusIndex++;
        FocusSelectedOption();
    }

    /// <summary>
    /// Destacar la opcion seleccionada en el menu de opciones
    /// </summary>
    private void FocusSelectedOption()
    {
        if (focusIndex < 0 || focusIndex >= optionButtons.Count) focusIndex = 0;
        for (int i = 0; i < optionButtons.Count; i++)
        {
            optionButtons[i].SetInteractable(focusIndex == i);
        }
    }

    /// <summary>
    /// Hacer click sobre el boton seleccionado de las opciones
    /// </summary>
    private void ClickCurrentOption()
    {
        if (focusIndex < 0 || focusIndex >= optionButtons.Count) return;
        optionButtons[focusIndex].GetButton().onClick.Invoke();
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
                Decrease();
                break;
            case JoystickAction.Right:
                Increase();
                break;
            case JoystickAction.A:
                ClickCurrentOption();
                break;
        }
        return false;
    }
    #endregion
}
