using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;
using UnityEngine.Analytics;
using System;
using JankenUp;
using UnityEngine.Localization.Settings;

public class SingleModeRoundComplete : SceneController
{

    [Header("Results")]
    [SerializeField] TextMeshProUGUI levelValue;
    [SerializeField] TextMeshProUGUI victoriesValue;
    [SerializeField] TextMeshProUGUI defeatsValue;
    [SerializeField] TextMeshProUGUI tiesValue;
    [SerializeField] TextMeshProUGUI coinsValue;
    [SerializeField] TextMeshProUGUI roundValue;
    [SerializeField] TextMeshProUGUI maxComboValue;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI levelLabel;
    [SerializeField] TextMeshProUGUI victoriesLabel;
    [SerializeField] TextMeshProUGUI defeatsLabel;
    [SerializeField] TextMeshProUGUI tiesLabel;
    [SerializeField] TextMeshProUGUI coinsLabel;
    [SerializeField] TextMeshProUGUI roundLabel;
    [SerializeField] TextMeshProUGUI maxComboLabel;

    [Header("UI")]
    [SerializeField] CoinsDisplayer coinsDisplayer;

    [Header("Entonces")]
    [SerializeField] GameObject entoncesJokeLevel;
    [SerializeField] GameObject entoncesJokeVictories;
    [SerializeField] GameObject entoncesJokeDefeats;
    [SerializeField] GameObject entoncesJokeTies;
    [SerializeField] GameObject entoncesJokeCoins;
    [SerializeField] GameObject entoncesJokeMaxCombo;

    [Header("Others")]
    [SerializeField] JanKenShop janKenShop;
    [SerializeField] List<JoystickUIElement> optionButtons = new List<JoystickUIElement>();
    [SerializeField] [Range(0, 2)] int focusIndex = 1;

    // Numero para la talla del 11 (o 13)
    List<string> entoncesValidNumbers = new List<string>() { "11", "13" };

    // Sesión de juego
    SingleModeSession singleModeSession;

    // Monedas ganadas en la ronda
    int coinsFromRound = 0;

    // Flag para indicar que ya se realizo la conexion con GooglePlayGames
    bool isCheckGooglePlayGames = false;
    private static bool firstTimeGooglePlay = false;

    new void Start()
    {
        base.Start();

        // Agregar una sesion de juego
        GameController.IncreasePlaySessions();

        // Obtener la sesión
        singleModeSession = FindObjectOfType<SingleModeSession>();

        // Configurar los resultados
        ConfigResults();

        // Guardar las monedas obtenidas
        if (singleModeSession)
        {
            // Registrar el personaje usado si es la primera ronda
            if (singleModeSession.GetCurrentRound() == 1 && Tutorial.IsTutorialReady())
            {
                Analytics.CustomEvent(AnalyticsEvents.GetEventString(AnalyticsEvents.Events.OfflineSelectedCharacter), new Dictionary<string, object> {
                    {"character", singleModeSession.GetPlayer().GetComponent<CharacterConfiguration>().GetIdentifier()}
                });
            }

            singleModeSession.IncreaseCurrentRound();
            GameController.Save(singleModeSession.GetCoins());
        }

        // Enviar la data al leaderboard de pisos y de puntaje
        ReportScore();

        UpdateCurrentFont();

        // Indicar la data a las analiticas
        Analytics.CustomEvent("SingleResults", new Dictionary<string, object>
        {
            { "floor", singleModeSession.GetLevel() },
            { "score", singleModeSession.GetScore() }
        });

        if (JoystickSupport.Instance.SupportActivated()) FocusSelectedOption();
    }

    // Cuando las puertas esteb abiertas, conectar a GooglePlayGames
    new void Update()
    {
        base.Update();

        if (isReady && !isCheckGooglePlayGames)
        {
            isCheckGooglePlayGames = true;
            GooglePlayGamesController._this.FirstTimeSetup(() =>
           {
               firstTimeGooglePlay = true;
               ReportScore();
           });
        }
    }

    // Reporte del score obtenido por el usuario
    private void ReportScore()
    {
        if (singleModeSession)
        {
            // Floor Master
            //Social.ReportScore(singleModeSession.GetLevel(), JankenUp.Leaderboards.FLOORMASTER, (bool success) => { });

            // Puntaje máximo
            //Social.ReportScore(singleModeSession.GetScore(), JankenUp.Leaderboards.MAXSCORE, (bool success) => { });

            // Intraterrestial
            //Social.ReportScore(singleModeSession.GetLevel(), JankenUp.Leaderboards.INTRATERRESTIAL, (bool success) => { });

            // MaxCombo
            //Social.ReportScore(singleModeSession.GetMaxCombo(), JankenUp.Leaderboards.MAXCOMBO, (bool success) => { });
        }

        // Si existen achievements que no han sido reportados, reportar aca. Solo correra si fue configurado por primera vez desde esta pantalla
        if (GooglePlayGamesController._this && firstTimeGooglePlay) StartCoroutine(ReportingDelayedAchievements());

    }

    // Reporte de los achivements no reportados
    private IEnumerator ReportingDelayedAchievements()
    {
        yield return new WaitForSeconds(1f);

        // Solicitar todos los achievements de GooglePlay
        List<string> readyAchievements = singleModeSession.GetReadyAchievements();

        foreach (string achievement in readyAchievements)
        {
            GooglePlayGamesController._this.ReportAchievement(achievement, 100.0f);
        }
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        levelLabel.font = mainFont;
        victoriesLabel.font = mainFont;
        defeatsLabel.font = mainFont;
        tiesLabel.font = mainFont;
        coinsLabel.font = mainFont;
        roundLabel.font = mainFont;
        roundValue.font = mainFont;
        maxComboLabel.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;

        levelLabel.fontStyle = style;
        victoriesLabel.fontStyle = style;
        defeatsLabel.fontStyle = style;
        tiesLabel.fontStyle = style;
        coinsLabel.fontStyle = style;
        roundLabel.fontStyle = style;
        roundValue.fontStyle = style;
        maxComboLabel.fontStyle = style;
    }

    // Configuración de los resultados
    private void ConfigResults()
    {

        if (singleModeSession)
        {
            levelValue.text = (singleModeSession.GetLevel()).ToString();
            victoriesValue.text = singleModeSession.GetVictories().ToString();
            defeatsValue.text = singleModeSession.GetDefeats().ToString();
            tiesValue.text = singleModeSession.GetTies().ToString();
            coinsFromRound = singleModeSession.GetSessionCoins();
            coinsValue.text = coinsFromRound > 0 ? ("+" + coinsFromRound.ToString()) : coinsFromRound.ToString();
            maxComboValue.text = singleModeSession.GetMaxCombo().ToString();

            // Sincronizar las monedas ganadas hasta el momento
            singleModeSession.SyncInitialCoins();

            EntoncesJokeExec();
        }

    }

    /// <summary>
    /// Si se esta jugando en chileno, argentino o espanol, hacer la talla del 11
    /// </summary>
    private void EntoncesJokeExec()
    {
        UnityEngine.Localization.Locale locale = LocalizationSettings.Instance.GetSelectedLocale();
        bool doTheJoke = false;
        switch (locale.Identifier.Code)
        {
            case "es":
            case "es-CL":
            case "es-AR":
                doTheJoke = true;
                break;
        }

        // Hacer la talla. Cualquier numero terminado en las bases indicadas debe ser considerado
        if (doTheJoke)
        {
            // Calculos de los ultimos 2 digitos (si existen)
            string level = singleModeSession.GetLevel().ToString()[^(singleModeSession.GetLevel().ToString().Length > 1 ? 2 : 1)..];
            string victories = singleModeSession.GetVictories().ToString()[^(singleModeSession.GetVictories().ToString().Length > 1 ? 2 : 1)..];
            string defeats = singleModeSession.GetDefeats().ToString()[^(singleModeSession.GetDefeats().ToString().Length > 1 ? 2 : 1)..];
            string ties = singleModeSession.GetTies().ToString()[^(singleModeSession.GetTies().ToString().Length > 1 ? 2 : 1)..];
            string coins = singleModeSession.GetCoins().ToString()[^(singleModeSession.GetCoins().ToString().Length > 1 ? 2 : 1)..];
            string maxCombo = singleModeSession.GetMaxCombo().ToString()[^(singleModeSession.GetMaxCombo().ToString().Length > 1 ? 2 : 1)..];

            // Activacion de las tallas
            entoncesJokeLevel.SetActive(entoncesValidNumbers.Contains(level));
            entoncesJokeVictories.SetActive(entoncesValidNumbers.Contains(victories));
            entoncesJokeDefeats.SetActive(entoncesValidNumbers.Contains(defeats));
            entoncesJokeTies.SetActive(entoncesValidNumbers.Contains(ties));
            entoncesJokeCoins.SetActive(entoncesValidNumbers.Contains(coins));
            entoncesJokeMaxCombo.SetActive(entoncesValidNumbers.Contains(maxCombo));
        }
    }

    // Continuar con la siguiente ronda de niveles
    public void Continue()
    {
        // Indicar que ads no ha sido visto para proximo round
        singleModeSession.SetAdsWasWatched(false);

        // Ir a la pantalla de juego
        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        slm.SingleMode();
    }

    // Volver al inicio
    public void Home()
    {
        // Resetear los valores
        if (singleModeSession)
        {
            Destroy(singleModeSession.gameObject);
        }

        // Ir al menú
        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        slm.MainScreen();
    }

    // Actualiza todos los elementos ligados a un translate
    protected override void Localize()
    {
        LocalizationHelper.Translate(levelLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.floor);
        LocalizationHelper.Translate(victoriesLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.victories);
        LocalizationHelper.Translate(defeatsLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.defeats);
        LocalizationHelper.Translate(tiesLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.ties);
        LocalizationHelper.Translate(coinsLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.coins);
        LocalizationHelper.Translate(roundLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.bravo);
        LocalizationHelper.Translate(maxComboLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.maxCombo);

        // Numero de ronda
        if (!singleModeSession) singleModeSession = FindObjectOfType<SingleModeSession>();
        var roundString = new[] { new { round = singleModeSession.GetCurrentRound() } };
        LocalizationHelper.FormatTranslate(roundValue, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.round, roundString);
    }

    /// <summary>
    /// Cambiar al anterior idioma en base a la posicion del focus
    /// </summary>
    private void Decrease(float times = 1)
    {
        int timesInt = (int)times;
        if (focusIndex <= 0 || (focusIndex - timesInt) < 0) return;
        focusIndex -= timesInt;
        FocusSelectedOption();
    }

    /// <summary>
    /// Cambiar al siguiente idioma en base a la posicion del focus
    /// </summary>
    private void Increase(float times = 1)
    {
        int timesInt = (int)times;
        if (focusIndex >= optionButtons.Count - 1 || (focusIndex + timesInt) > optionButtons.Count - 1) return;
        focusIndex += timesInt;
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
    protected override bool OnJoystick(JoystickAction action, int playerIndex)
    {
        if (!base.OnJoystick(action, playerIndex)) return false;

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
            case JoystickAction.L:
                if (janKenShop.isActiveAndEnabled) janKenShop.InvokeBuyItem(SuperPowers.TimeMaster);
                break;
            case JoystickAction.R:
                if (janKenShop.isActiveAndEnabled) janKenShop.InvokeBuyItem(SuperPowers.MagicWand);
                break;
            case JoystickAction.ZL:
            case JoystickAction.ZR:
                if (janKenShop.isActiveAndEnabled) janKenShop.InvokeBuyItem(SuperPowers.JanKenUp);
                break;
        }
        return false;

    }
    #endregion
}
