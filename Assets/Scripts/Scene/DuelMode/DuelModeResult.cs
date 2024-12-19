using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
using DigitalRuby.Tween;
using UnityEngine.Analytics;
using System;
using JankenUp;
using UnityEngine.Localization.Settings;

public class DuelModeResult : SceneController
{

    [Header("Results")]
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] TextMeshProUGUI victoriesValue;
    [SerializeField] TextMeshProUGUI scoreValue;
    [SerializeField] TextMeshProUGUI tiesValue;
    [SerializeField] TextMeshProUGUI maxComboValue;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI rankLabel;
    [SerializeField] TextMeshProUGUI victoriesLabel;
    [SerializeField] TextMeshProUGUI scoreLabel;
    [SerializeField] TextMeshProUGUI tiesLabel;
    [SerializeField] TextMeshProUGUI maxComboLabel;

    [Header("Entonces")]
    [SerializeField] GameObject entoncesJokeVictories;
    [SerializeField] GameObject entoncesJokeScore;
    [SerializeField] GameObject entoncesJokeTies;
    [SerializeField] GameObject entoncesJokeMaxCombo;

    [Header("Others")]
    [SerializeField] bool rrssButtons = true;
    [SerializeField] JanKenCard janKenCard;
    [SerializeField] JKButton shareButton;
    [SerializeField] List<JoystickUIElement> optionButtons = new List<JoystickUIElement>();
    [SerializeField] [Range(0, 3)] int focusIndex = 2;
    [SerializeField] Button nextModeButton;
    [SerializeField] Button prevModeButton;
    [SerializeField] [Range(0,1000)] int scoreFactor = 999;

    // Numero para la talla del 11 (o 13)
    List<string> entoncesValidNumbers = new List<string>() { "11", "13" };

    // Sesión de juego
    SingleModeSession singleModeSession;

    // Utiles
    int focusPlayer = 0;
    List<int> ranking = new List<int>();
    int scoreMin = 11;
    int scoreMax = 99999;
    int currentPlayerScore = 0;

    new void Start()
    {
        base.Start();

        // Agregar una sesion de juego
        GameController.IncreasePlaySessions();
        GameController.SetGameplayActive(true);

        // Obtener la sesión
        singleModeSession = FindObjectOfType<SingleModeSession>();

        // Obtener el listado de jugadores
        ranking = singleModeSession.GetPlayersRanking();

        // Configurar los resultados
        ConfigResults();
        UpdateCurrentFont();

        // Asociar boton de compartir card en RRSS
        if (rrssButtons) shareButton.onClickDelegate += ShareCard;
        else shareButton.gameObject.SetActive(false);

        // Cambio de jugador
        nextModeButton.onClick.AddListener(delegate { NextPlayer(); });
        prevModeButton.onClick.AddListener(delegate { PrevPlayer(); });

        if (JoystickSupport.Instance.SupportActivated()) FocusSelectedOption();
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        playerName.font = mainFont;
        rankLabel.font = mainFont;
        victoriesLabel.font = mainFont;
        scoreLabel.font = mainFont;
        tiesLabel.font = mainFont;
        rankLabel.font = mainFont;
        maxComboLabel.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        playerName.fontStyle = style;
        rankLabel.fontStyle = style;
        victoriesLabel.fontStyle = style;
        scoreLabel.fontStyle = style;
        tiesLabel.fontStyle = style;
        rankLabel.fontStyle = style;
        maxComboLabel.fontStyle = style;
    }

    /// <summary>
    /// Seleccionar el siguiente jugador del ranking
    /// </summary>
    public void NextPlayer()
    {
        UIButtonSFX();
        focusPlayer++;
        ConfigResults();
    }

    /// <summary>
    /// Seleccionar el jugador del ranking anterior
    /// </summary>
    public void PrevPlayer()
    {
        UIButtonSFX();
        focusPlayer--;
        ConfigResults();
    }

    // Configuración de los resultados
    private void ConfigResults() {

        if (singleModeSession){

            // Revision del focus player
            if (focusPlayer < 0) focusPlayer = ranking.Count - 1;
            else if (focusPlayer >= ranking.Count) focusPlayer = 0;

            // Obtener el ranking actual
            int playerIndex = ranking[focusPlayer];
            var playerData = new[] { new { player = playerIndex + 1 } };
            LocalizationHelper.FormatTranslate(playerName, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.player_x, playerData);

            var playerRanking= new[] { new { rank = focusPlayer + 1 } };
            LocalizationHelper.FormatTranslate(rankLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.rank_x, playerRanking);

            // Calcular el score: (Victorias + (.5 * Empates)) * Combos * Factor
            currentPlayerScore =
                Mathf.Clamp(
                    (int) (singleModeSession.GetPlayerVictory(playerIndex) + singleModeSession.GetPlayerTies(playerIndex) * .5f) * singleModeSession.GetPlayerCombos(playerIndex) * scoreFactor,
                    scoreMin,
                    scoreMax);

            victoriesValue.text = singleModeSession.GetPlayerVictory(playerIndex).ToString();
            scoreValue.text = currentPlayerScore.ToString();
            tiesValue.text = singleModeSession.GetPlayerTies(playerIndex).ToString();
            maxComboValue.text = singleModeSession.GetPlayerCombos(playerIndex).ToString();

            EntoncesJokeExec();

            // Configuraciones especiales de la card
            janKenCard.Setup(singleModeSession.GetPlayer(playerIndex).GetComponent<CharacterConfiguration>(), true, true);
            janKenCard.SetCardNumber(focusPlayer + 1);
            janKenCard.SetQlPoints(currentPlayerScore);
            janKenCard.SetAttacks(singleModeSession.GetPlayerAttackSequence(playerIndex));
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
            int playerIndex = ranking[focusPlayer];

            // Calculos de los ultimos 2 digitos (si existen)
            string victories = singleModeSession.GetPlayerVictory(playerIndex).ToString()[^(singleModeSession.GetPlayerVictory(playerIndex).ToString().Length > 1? 2 : 1 )..];
            string score = currentPlayerScore.ToString()[^(currentPlayerScore.ToString().Length > 1 ? 2 : 1)..];
            string ties = singleModeSession.GetPlayerTies(playerIndex).ToString()[^(singleModeSession.GetPlayerTies(playerIndex).ToString().Length > 1? 2 : 1 )..];
            string maxCombo = singleModeSession.GetPlayerCombos(playerIndex).ToString()[^(singleModeSession.GetPlayerCombos(playerIndex).ToString().Length > 1? 2 : 1 )..];

            // Activacion de las tallas
            entoncesJokeVictories.SetActive(entoncesValidNumbers.Contains(victories));
            entoncesJokeScore.SetActive(entoncesValidNumbers.Contains(score));
            entoncesJokeTies.SetActive(entoncesValidNumbers.Contains(ties));
            entoncesJokeMaxCombo.SetActive(entoncesValidNumbers.Contains(maxCombo));
        }
    }

    /// <summary>
    /// Realizar los ajustes para volver a jugar con el mismo PJ
    /// </summary>
    public void Retry()
    {
        // Resetear los valores
        if (singleModeSession)
        {
            singleModeSession.Reset();
        }

        // Ir a la pantalla de juego
        SceneLoaderManager.Instance.DuelMode();
    }

    /// <summary>
    /// Realizar los ajustes para volver a jugar con el mismo PJ
    /// </summary>
    public void Home()
    {
        // Resetear los valores
        if (singleModeSession)
        {
            Destroy(singleModeSession.gameObject);
        }

        GameController.SetGameplayActive(false);
        // Ir al menú
        SceneLoaderManager.Instance.MainScreen();
    }

    /// <summary>
    /// Ir a seleccion de personaje
    /// </summary>
    public void CharacterSelection()
    {
        // Resetear los valores
        if (singleModeSession)
        {
            singleModeSession.Reset();
        }

        // Ir al seleccion
        SceneLoaderManager.Instance.DuelModeSelection();
    }

    /// <summary>
    /// Ir a modo clasico de juego
    /// </summary>
    public void Classic()
    {
        // Resetear los valores
        if (singleModeSession)
        {
            singleModeSession.Reset();
        }

        // Ir a la pantalla de juego
        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        slm.SingleMode();
    }

    // Actualiza todos los elementos ligados a un translate
    protected override void Localize()
    {
        LocalizationHelper.Translate(victoriesLabel, JankenUp.Localization.tables.Online.tableName, JankenUp.Localization.tables.Online.Keys.kos);
        LocalizationHelper.Translate(scoreLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.score);
        LocalizationHelper.Translate(tiesLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.ties);
        LocalizationHelper.Translate(maxComboLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.maxCombo);

        // TODO: Ranking
        //LocalizationHelper.Translate(timeLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.time);
    }

    /// <summary>
    /// Compartir tarjeta de personaje
    /// </summary>
    void ShareCard()
    {
        if (janKenCard.IsAnimationReady()) janKenCard.ShareScreenshot();
    }

    /// <summary>
    /// Quitar delegados
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        shareButton.onClickDelegate -= ShareCard;
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
        for (int i = 0; i < optionButtons.Count; i++){
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
            case JoystickAction.L:
                GameController.SimulateClick(prevModeButton);
                break;
            case JoystickAction.R:
                GameController.SimulateClick(nextModeButton);
                break;
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
