using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleModeSelectionController : SceneController
{
    [SerializeField] JKButton nextButton;
    [SerializeField] PJSelectorOverlay selectorOverlay;
    [SerializeField] [Range(0, 1)] float openDoorsAfter = 0.25f;

    // Flag para indicar que ya se realizo la conexion con GooglePlayGames
    bool isCheckGooglePlayGames = false;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        GameController.SetGameplayActive(true);

        openDoor = false;
        nextButton.onClickDelegate += CanPlay;
        PJSelectorOverlay.onSelectionDelegate += OnSelection;
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
        Localize();

        // Iniciar espera para abrir puertas
        StartCoroutine(WaitForOpenDoors());
    }

    /// <summary>
    /// Apertura de puertas post espera de tiempo
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForOpenDoors()
    {
        yield return new WaitForSeconds(openDoorsAfter);
        openDoor = true;
    }

    /// <summary>
    /// Cuando las puertas esteb abiertas, conectar a GooglePlayGames
    /// </summary>
    new void Update()
    {
        base.Update();

        if (isReady && !isCheckGooglePlayGames)
        {
            isCheckGooglePlayGames = true;
            GooglePlayGamesController._this.FirstTimeSetup();
        }
    }

    /// <summary>
    /// Revisar si es posible jugar con el personaje actualmente seleccionado
    /// </summary>
    public void CanPlay() {
        if(selectorOverlay && selectorOverlay.GetCurrentCharacter())
        {
            if (selectorOverlay.GetCurrentCharacter().IsUnlocked())
            {
                // Realizar una limpieza de los players en el pool
                CharacterPool.Instance.ResetPlayerList(new List<int>{ 0 });
                CharacterPool.Instance.SetPlayersAllowedToDuel(new List<int> { 0 });

                SingleModeSession sms = FindObjectOfType<SingleModeSession>();
                if (sms) sms.SetPlayer(CharacterPool.Instance.Get(selectorOverlay.GetCurrentCharacter().GetIdentifier()));
                UIButtonSFX();
                switch (GameController.GetLastGameMode())
                {
                    case GameMode.Frenzy:
                        SceneLoaderManager.Instance.SurvivalMode();
                        break;
                    default:
                        SceneLoaderManager.Instance.SingleMode();
                        break;
                }
                    
                
            }
            else
            {
                MasterSFXPlayer._player.Error();
            }
        }
    }

    /// <summary>
    /// Ajustar textos
    /// </summary>
    protected override void Localize()
    {
        base.Localize();
        nextButton.UpdateCurrentFont();
        LocalizationHelper._this.TranslateThis(nextButton.GetText(), JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.play);
    }

    /// <summary>
    /// Evento llamado a cambio de personaje
    /// </summary>
    /// <param name="characterConfiguration"></param>
    public void OnSelection(CharacterConfiguration characterConfiguration)
    {
        nextButton.SetBW(!characterConfiguration.IsUnlocked());
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
        GameController.SetGameplayActive(false);
        nextButton.onClickDelegate -= CanPlay;
        PJSelectorOverlay.onSelectionDelegate -= OnSelection;
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;
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
                CanPlay();
                canContinue = false;
                break;
            case JoystickAction.L:
                GoBack();
                canContinue = false;
                break;
            case JoystickAction.X:
                selectorOverlay.ShowCharacterMoreInformation();
                canContinue = false;
                break;
            case JoystickAction.B:
                selectorOverlay.BuyCharacter();
                canContinue = false;
                break;
            case JoystickAction.Up:
            case JoystickAction.Down:
            case JoystickAction.Left:
            case JoystickAction.Right:
                canContinue = selectorOverlay.MoveTo(action);
                break;
        }
        return canContinue;
    }
    #endregion

}
