using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelectorController : SceneController
{
    [Header("Components")]
    [SerializeField] Transform gameModesPanel;
    [SerializeField] Button nextModeButton;
    [SerializeField] Button prevModeButton;
    [SerializeField] JKButton nextButton;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI difficultyLabel;
    [SerializeField] Slider difficultySlider;
    [SerializeField] CanvasGroup difficultyCanvasGroup;

    [Header("Options")]
    [SerializeField] bool duelModeAllowed = false;
    [SerializeField] bool duelModeAllowedOnJoystickSupport = false;

    [Header("Prefabs")]
    [SerializeField] List<GameModeCard> gameModePrefabs = new List<GameModeCard>();
    [SerializeField] GameModeCard duelModePrefab;
    GameModeCard currentGameMode;
    int gameModeIndex = 0;

    // Tutorial
    bool tutorialStatus = false;

    // Nivel de dificultad (Siempre presuponer que no se sabe jugar)
    int classicLevel = 0;
    int classicMinLevel = 0;
    int classicMaxLevel = 20;
    int frenzyLevel = 1;
    int frenzyMinLevel = 1;
    int frenzyMaxLevel = 9;

    // Corutinas
    Coroutine updateOptionsMenuCoroutine;

    new void Start()
    {
        base.Start();
        GameController.SetGameplayActive(false);

        // Agregar el modo duelo si esta permitido
        if (duelModeAllowed || (duelModeAllowedOnJoystickSupport && JoystickSupport.Instance.SupportActivated())) gameModePrefabs.Add(duelModePrefab);
        if (!duelModeAllowed && duelModeAllowedOnJoystickSupport) JoystickSupport.onSupportStatusChange += OnJoystickSupportChange;

        // Cargar el ultimo modo de juego seleccionado
        GameMode lastGameMode = GameController.GetLastGameMode();
        int lastIndex = gameModePrefabs.FindIndex(gmp => gmp.gameMode == lastGameMode);
        if (lastIndex != -1) gameModeIndex = lastIndex;
        SelectGameMode();

        nextModeButton.onClick.AddListener(delegate { NextMode(); });
        prevModeButton.onClick.AddListener(delegate { PrevMode(); });
        nextButton.onClickDelegate += GoToGameMode;
        difficultySlider.onValueChanged.AddListener(UpdateDifficultLevel);

        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
        Localize();

        // Reiniciar nivel de dificultad
        classicLevel = GameController.Load().difficultyLevel;
        frenzyLevel = GameController.Load().survivalLevel;
        difficultySlider.value = lastGameMode == GameMode.Classic? ((float)classicLevel / classicMaxLevel) : ((float)frenzyLevel / frenzyMaxLevel);
    }

    /// <summary>
    /// Cambio en el soporte de joystick
    /// </summary>
    /// <param name="value"></param>
    private void OnJoystickSupportChange(bool value)
    {
        if(duelModeAllowedOnJoystickSupport && value)
        {
            if(!gameModePrefabs.Contains(duelModePrefab)) gameModePrefabs.Add(duelModePrefab);
        }
        else if(duelModeAllowedOnJoystickSupport)
        {
            if (gameModePrefabs.Contains(duelModePrefab)) gameModePrefabs.Remove(duelModePrefab);
            if (currentGameMode.gameMode == GameMode.Duel) SelectGameMode();
        }
    }

    /// <summary>
    /// Actualizacion del modo de juego actual
    /// </summary>
    private void SelectGameMode() {
        if (currentGameMode) currentGameMode.ByeBye();
        if (gameModeIndex < 0) gameModeIndex = gameModePrefabs.Count - 1;
        else if (gameModeIndex >= gameModePrefabs.Count) gameModeIndex = 0;
        currentGameMode = Instantiate(gameModePrefabs[gameModeIndex], gameModesPanel);

        if (updateOptionsMenuCoroutine != null) StopCoroutine(updateOptionsMenuCoroutine);

        if (currentGameMode.gameMode != GameMode.Duel)
        {
            updateOptionsMenuCoroutine = StartCoroutine(UpdateOptionsMenu(true));
            if (difficultyCanvasGroup) difficultyCanvasGroup.alpha = 1;
            currentGameMode.SetLevel(currentGameMode.gameMode == GameMode.Classic ? classicLevel : frenzyLevel);
        }
        else
        {
            updateOptionsMenuCoroutine = StartCoroutine(UpdateOptionsMenu(false));
            if (difficultyCanvasGroup) difficultyCanvasGroup.alpha = 0;
        }     
    }

    /// <summary>
    /// Corutina para 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private IEnumerator UpdateOptionsMenu(bool value = true)
    {
        if (optionsMenu)
        {
            while (optionsMenu.IsLoadingOptions()) yield return null;
            optionsMenu.ActiveTutorial(value);
        }
        yield return null;
    }

    /// <summary>
    /// Ir a pantalla de seleccion de personajes
    /// </summary>
    private void GoToGameMode()
    {
        GameController.SetLastGameMode(currentGameMode.gameMode);
        switch (currentGameMode.gameMode)
        {
            case GameMode.Classic:
            case GameMode.Frenzy:
                SceneLoaderManager.Instance.SingleModeSelection();
                break;
            case GameMode.Duel:
                SceneLoaderManager.Instance.DuelModeSelection();
                break;
        }
    }

    /// <summary>
    /// Guardado en controlador del nivel de dificultad ademas de despliegue de feedback
    /// </summary>
    /// <param name="level">Nivel de 0 a 1</param>
    private void UpdateDifficultLevel(float level) {
        classicLevel = Mathf.Clamp((int) Mathf.Floor(level * classicMaxLevel), classicMinLevel, classicMaxLevel);
        frenzyLevel = Mathf.Clamp((int) Mathf.Floor(level * frenzyMaxLevel), frenzyMinLevel, frenzyMaxLevel);
        GameController.SaveDifficultyLevel(classicLevel);
        GameController.SaveDifficultySurvival(frenzyLevel);
        // Cambiar el nivel en el titulo de la card
        currentGameMode.SetLevel(currentGameMode.gameMode == GameMode.Classic ? classicLevel : frenzyLevel);
    }

    /// <summary>
    /// Seleccionar el siguiente modo de juego
    /// </summary>
    public void NextMode()
    {
        UIButtonSFX();
        gameModeIndex++;
        SelectGameMode();
    }

    /// <summary>
    /// Seleccionar el modo de juego anterior
    /// </summary>
    public void PrevMode()
    {
        UIButtonSFX();
        gameModeIndex--;
        SelectGameMode();
    }

    /// <summary>
    /// Ajustar textos
    /// </summary>
    protected override void Localize()
    {
        base.Localize();
        UpdateCurrentFont();
        LocalizationHelper._this.TranslateThis(title, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.selectGameMode);
        LocalizationHelper._this.TranslateThis(difficultyLabel, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.difficulty);
        nextButton.UpdateCurrentFont();
        LocalizationHelper._this.TranslateThis(nextButton.GetText(), JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.next);
    }

    /// <summary>
    /// Actualizacion de la fuente del mensaje y de la localizacion del texto
    /// </summary>
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        Material material = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        title.font = mainFont;
        title.fontSharedMaterial = material;
        difficultyLabel.font = mainFont;
        difficultyLabel.fontSharedMaterial = material;
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
        if (updateOptionsMenuCoroutine != null) StopCoroutine(updateOptionsMenuCoroutine);
        nextButton.onClickDelegate -= GoToGameMode;
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;

        // Agregar el modo duelo si esta permitido
        if (!duelModeAllowed && duelModeAllowedOnJoystickSupport) JoystickSupport.onSupportStatusChange -= OnJoystickSupportChange;
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    protected override bool OnJoystick(JoystickAction action, int playerIndex)
    {
        if (!base.OnJoystick(action, playerIndex)) return false;
        bool canContinue = true;
        switch (action)
        {
            case JoystickAction.R:
            case JoystickAction.A:
                GoToGameMode();
                canContinue = false;
                break;
            case JoystickAction.L:
            case JoystickAction.Escape:
                GoBack();
                canContinue = false;
                break;
            case JoystickAction.Up:
                if(currentGameMode.gameMode != GameMode.Duel)
                {
                    difficultySlider.value += 0.1f;
                }
                canContinue = false;
                break;
            case JoystickAction.Down:
                if (currentGameMode.gameMode != GameMode.Duel) { 
                    difficultySlider.value -= 0.1f;
                }
                canContinue = false;
                break;
            case JoystickAction.Left:
                GameController.SimulateClick(prevModeButton);
                canContinue = false;
                break;
            case JoystickAction.Right:
                GameController.SimulateClick(nextModeButton);
                canContinue = false;
                break;
        }
        return canContinue;
    }
    #endregion
}