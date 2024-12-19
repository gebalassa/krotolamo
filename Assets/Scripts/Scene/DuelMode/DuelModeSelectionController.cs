using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DuelModeSelectionController : SceneController
{
    [SerializeField] JKButton nextButton;
    [SerializeField] PJMultipleSelectorOverlay selectorOverlay;
    [SerializeField] [Range(0, 1)] float openDoorsAfter = 0.25f;
    [SerializeField] [Range(0, 8)] int minPlayersToPlay = 2;

    // Flag para indicar que ya se realizo la conexion con GooglePlayGames
    bool isCheckGooglePlayGames = false;

    // Utiles
    bool isGoingToDuelMode = false;
    List<int> playersReady = new List<int>();

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        GameController.SetGameplayActive(true);
        GameController.SetSimulation(false);

        openDoor = false;
        nextButton.onClickDelegate += CanPlay;
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
        Localize();

        // Iniciar espera para abrir puertas
        StartCoroutine(WaitForOpenDoors());

        // Suscripcion a entrada/salida de jugadores
        JoystickSupport.onPlayerJoinDelegate += OnPlayerJoin;
        JoystickSupport.onPlayerLeftDelegate += OnPlayerLeft;
        PJSelectorDisplayer.onJoinedDelegate += OnPJSelectorDisplayerJoined;

        CheckPlayers();
    }

    // TODO: Borrar
    IEnumerator Simulate()
    {
        yield return null;
        for(int i = 0; i < 8; i++)
        {
            selectorOverlay.Join(i);
            yield return new WaitForSeconds(.5f);
        }

        int maxChanges = 30;
        int currentChanges = 0;
        while (maxChanges >= currentChanges) {
            if(Random.Range(0, 2) == 1) selectorOverlay.MoveTo(JoystickAction.Right, Random.Range(0, 8));
            else selectorOverlay.MoveTo(JoystickAction.Left, Random.Range(0, 8));
            yield return new WaitForSeconds(.1f);
            currentChanges++;
        }
    }

    /// <summary>
    /// Apertura de puertas post espera de tiempo
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForOpenDoors()
    {
        yield return new WaitForSeconds(openDoorsAfter);
        openDoor = true;
        // TODO: Borrar
        //StartCoroutine(Simulate());
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
    public void CanPlay(int player = 0) {
        if (CheckPlayers())
        {
            if (playersReady.Contains(player)) return;
            else
            {
                MarkPlayerAsReady(player);
                UIButtonSFX();

                if (playersReady.Count == selectorOverlay.CountJoined())
                {
                    // Realizar una limpieza de los players en el pool
                    CharacterPool.Instance.ResetPlayerList(selectorOverlay.GetJoined());
                    CharacterPool.Instance.SetPlayersAllowedToDuel(selectorOverlay.GetJoined());

                    isGoingToDuelMode = true;
                    SceneLoaderManager.Instance.DuelMode();
                }
            }
        }
        else
        {
            MasterSFXPlayer._player.Error();
        }
    }

    /// <summary>
    /// Alias para llamada por boton
    /// </summary>
    public void CanPlay(){
        CanPlay(0);
    }

    /// <summary>
    /// Agrega al jugador a la lista de jugadores listos
    /// </summary>
    /// <param name="player"></param>
    private void MarkPlayerAsReady(int player = 0)
    {
        if (!playersReady.Contains(player))
        {
            playersReady.Add(player);
            selectorOverlay.GetSelectorsCharacters(player).Select();
        }

    }

    /// <summary>
    /// Remover a los jugadores de la lista de jugadores listos
    /// </summary>
    /// <param name="player"></param>
    private void RemovePlayerAsReady(int player = 0)
    {
        if (playersReady.Contains(player))
        {
            playersReady.Remove(player);
            selectorOverlay.GetSelectorsCharacters(player).Select(false);
        }
    }

    /// <summary>
    /// Revisar la cantidad de jugadores conectados y activar boton si cuenta con el minimo
    /// </summary>
    private bool CheckPlayers(){
        bool ready = JoystickSupport.Instance.GetPlayerCount() >= minPlayersToPlay && selectorOverlay.CountJoined() >= minPlayersToPlay;
        nextButton.SetBW(!ready);
        return ready;
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

    protected new void OnDestroy()
    {
        base.OnDestroy();
        if(!isGoingToDuelMode) GameController.SetGameplayActive(false);
        nextButton.onClickDelegate -= CanPlay;
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;
        JoystickSupport.onPlayerJoinDelegate -= OnPlayerJoin;
        JoystickSupport.onPlayerLeftDelegate -= OnPlayerLeft;
        PJSelectorDisplayer.onJoinedDelegate -= OnPJSelectorDisplayerJoined;
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

        // Unir si no esta unido
        canContinue = selectorOverlay.Join(playerIndex);

        if (canContinue)
        {
            switch (action)
            {
                case JoystickAction.R:
                case JoystickAction.A:
                    CanPlay(playerIndex);
                    canContinue = false;
                    break;
                case JoystickAction.L:
                    GoBack();
                    canContinue = false;
                    break;
                case JoystickAction.X:
                    // TODO: X to confirm
                    //selectorOverlay.ShowCharacterMoreInformation();
                    canContinue = false;
                    break;
                case JoystickAction.Left:
                case JoystickAction.Right:
                    RemovePlayerAsReady(playerIndex);
                    selectorOverlay.MoveTo(action, playerIndex);
                    canContinue = false;
                    break;
            }
        }

        return canContinue;
    }

    /// <summary>
    /// Reaccion de conexion de jugador
    /// </summary>
    public void OnPlayerJoin(int playerIndex)
    {
        CheckPlayers();
    }

    /// <summary>
    /// Reaccion de desconexion de jugador
    /// </summary>
    public void OnPlayerLeft(int playerIndex)
    {
        selectorOverlay.Left(playerIndex);
        CheckPlayers();
    }

    /// <summary>
    /// Union efectiva a una caja de jugador
    /// </summary>
    public void OnPJSelectorDisplayerJoined()
    {
        CheckPlayers();
    }

    #endregion

}
