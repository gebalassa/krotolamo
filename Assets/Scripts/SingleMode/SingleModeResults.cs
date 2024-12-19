using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
using DigitalRuby.Tween;
using UnityEngine.Analytics;
using UnityEngine.Localization.Settings;

public class SingleModeResults : SceneController
{

    [Header("Results")]
    [SerializeField] TextMeshProUGUI levelValue;
    [SerializeField] TextMeshProUGUI victoriesValue;
    [SerializeField] TextMeshProUGUI defeatsValue;
    [SerializeField] TextMeshProUGUI tiesValue;
    [SerializeField] TextMeshProUGUI coinsValue;
    [SerializeField] TextMeshProUGUI scoreValue;
    [SerializeField] TextMeshProUGUI maxComboValue;
    [SerializeField] GameObject scoreNewRecord;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI levelLabel;
    [SerializeField] TextMeshProUGUI victoriesLabel;
    [SerializeField] TextMeshProUGUI defeatsLabel;
    [SerializeField] TextMeshProUGUI tiesLabel;
    [SerializeField] TextMeshProUGUI coinsLabel;
    [SerializeField] TextMeshProUGUI scoreLabel;
    [SerializeField] TextMeshProUGUI maxComboLabel;

    [Header("Entonces")]
    [SerializeField] GameObject entoncesJokeLevel;
    [SerializeField] GameObject entoncesJokeVictories;
    [SerializeField] GameObject entoncesJokeDefeats;
    [SerializeField] GameObject entoncesJokeTies;
    [SerializeField] GameObject entoncesJokeCoins;
    [SerializeField] GameObject entoncesJokeMaxCombo;

    [Header("Others")]
    [SerializeField] bool rrssButtons = true;
    [SerializeField] JanKenCard janKenCard;
    [SerializeField] JKButton shareButton;
    [SerializeField] JanKenShop janKenShop;
    [SerializeField] List<JoystickUIElement> optionButtons = new List<JoystickUIElement>();
    [SerializeField] [Range(0, 3)] int focusIndex = 2;

    // Numero para la talla del 11 (o 13)
    List<string> entoncesValidNumbers = new List<string>() { "11", "13" };

    // Sesión de juego
    SingleModeSession singleModeSession;

    // Configuraciones para nuevo record
    float showNewRecordTime = .4f;

    // Componentes
    Image newRecordIndicator;
    Text newRecordText;

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
            GameController.Save(singleModeSession.GetCoins());
            janKenCard.Setup(singleModeSession.GetPlayer().GetComponent<CharacterConfiguration>(), false, true);
            // Configuraciones especiales de la card
            janKenCard.SetQlPoints(singleModeSession.GetScore());
            janKenCard.SetAttacks(singleModeSession.GetAttackSequence());
        }

        // Enviar la data al leaderboard de pisos y de puntaje
        if (singleModeSession)
        {
            // Floor Master
            //Social.ReportScore(singleModeSession.GetLevel(), JankenUp.Leaderboards.FLOORMASTER, (bool success) => {});

            // Puntaje máximo
            //Social.ReportScore(singleModeSession.GetScore(), JankenUp.Leaderboards.MAXSCORE, (bool success) => {});

            // Intraterrestial
            //Social.ReportScore(singleModeSession.GetLevel(), JankenUp.Leaderboards.INTRATERRESTIAL, (bool success) => {});

            // MaxCombo
            //Social.ReportScore(singleModeSession.GetMaxCombo(), JankenUp.Leaderboards.MAXCOMBO, (bool success) => {});
        }

        UpdateCurrentFont();

        // Indicar la data a las analiticas
        Analytics.CustomEvent("SingleResults", new Dictionary<string, object>
        {
            { "floor", singleModeSession.GetLevel() },
            { "score", singleModeSession.GetScore() }
        });

        // Asociar boton de compartir card en RRSS
        if (rrssButtons) shareButton.onClickDelegate += ShareCard;
        else shareButton.gameObject.SetActive(false);

        if (JoystickSupport.Instance.SupportActivated()) FocusSelectedOption();
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
        scoreLabel.font = mainFont;
        maxComboLabel.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;

        levelLabel.fontStyle = style;
        victoriesLabel.fontStyle = style;
        defeatsLabel.fontStyle = style;
        tiesLabel.fontStyle = style;
        coinsLabel.fontStyle = style;
        scoreLabel.fontStyle = style;
        maxComboLabel.fontStyle = style;
    }

    // Configuración de los resultados
    private void ConfigResults() {

        if (singleModeSession)
        {
            levelValue.text = (singleModeSession.GetLevel()).ToString();
            victoriesValue.text = singleModeSession.GetVictories().ToString();
            defeatsValue.text = singleModeSession.GetDefeats().ToString();
            tiesValue.text = singleModeSession.GetTies().ToString();
            int coins = singleModeSession.GetSessionCoins();
            coinsValue.text = coins > 0? ( "+" + coins.ToString()) : coins.ToString();
            scoreValue.text = singleModeSession.GetScore().ToString();
            maxComboValue.text = singleModeSession.GetMaxCombo().ToString();
            if (singleModeSession.IsNewScoreRecord()) ShowNewRecord();

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

    // Realizar los ajustes para volver a jugar con el mismo PJ
    public void Retry()
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

    // Realizar los ajustes para volver a jugar con el mismo PJ
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
        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        slm.SingleModeSelection();
    }

    /// <summary>
    /// Ir a modo survival de juego
    /// </summary>
    public void Survival()
    {
        // Resetear los valores
        if (singleModeSession)
        {
            singleModeSession.Reset();
        }

        // Ir a la pantalla de juego
        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        slm.SurvivalMode();
    }

    // Mostrar indicador de nuevo record
    private void ShowNewRecord()
    {
        if (!newRecordIndicator)
        {
            newRecordIndicator = scoreNewRecord.GetComponent<Image>();
            newRecordText = scoreNewRecord.transform.Find("Text").GetComponent<Text>();
        }

        // Realizar aparicion del ataque
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            newRecordIndicator.color = t.CurrentValue;
            newRecordText.color = t.CurrentValue;
        };

        Color clear = new Color(1, 1, 1, 0);

        // Hacer fade in fadeout
        newRecordIndicator.gameObject.Tween(string.Format("FadeInMove{0}", newRecordIndicator.GetInstanceID()), clear, Color.white, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateColor)
            .ContinueWith(new ColorTween().Setup(Color.white, clear, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateColor))
            .ContinueWith(new ColorTween().Setup(clear, Color.white, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateColor));

    }

    // Actualiza todos los elementos ligados a un translate
    protected override void Localize()
    {
        LocalizationHelper.Translate(levelLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.floor);
        LocalizationHelper.Translate(victoriesLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.victories);
        LocalizationHelper.Translate(defeatsLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.defeats);
        LocalizationHelper.Translate(tiesLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.ties);
        LocalizationHelper.Translate(coinsLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.coins);
        LocalizationHelper.Translate(scoreLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.score);
        LocalizationHelper.Translate(maxComboLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.maxCombo);
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
