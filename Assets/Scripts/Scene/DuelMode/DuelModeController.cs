using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DuelModeController : GamePlayScene
{
    [Header("UI")]
    [SerializeField] LivesDisplayer playerOneLivesDisplayer;
    [SerializeField] LivesDisplayer playerTwoLivesDisplayer;
    [SerializeField] ComboDisplayer comboDisplayerPlayerOne;
    [SerializeField] ComboDisplayer comboDisplayerPlayerTwo;
    [SerializeField] TimeDisplayer timeDisplayer;
    [SerializeField] AttackStack playerOneStack;
    [SerializeField] AttackStack playerTwoStack;
    [SerializeField] MiniAvatarListDisplayer miniAvatarListDisplayer;
    [SerializeField] GameObject simulationCanvas;
    [SerializeField] AlertCanvas alertCanvas;
    [SerializeField] Canvas UICanvas;

    [Header("Referee")]
    [SerializeField] Referee referee;
    int steps = 6;
    int minSteps = 3;
    int currentCountdownSteps = -1;

    [Header("Actors")]
    [SerializeField] GameObject playerOne;
    [SerializeField] GameObject playerTwo;

    // Posiciones iniciales de los personajes
    Vector3 playerOneInitialPosition;
    Vector3 playerTwoInitialPosition;

    [Header("SFX")]
    [SerializeField] AudioClip attackSelection;
    [SerializeField] AudioClip illuminatiSFX;
    [SerializeField] AudioClip spendCoinsSuccess;
    [SerializeField] AudioClip spendCoinsFail;
    [SerializeField] AudioClip destinyLevelSFX;
    [SerializeField] AudioClip strongPunchSFX;

    [Header("Times")]
    [SerializeField] [Range(0, 1f)] float doorsClosingFor = 0.2f;
    [SerializeField] [Range(0, 1f)] float difficultyChangeTime = 1f;
    [SerializeField] [Range(0, 1f)] float superJanKenUpTime = 1f;

    // Componentes recurrentes
    CharacterInGameController playerOneController;
    CharacterInGameController playerTwoController;
    AudioSource audioSource;
    InGameSequence inGameSequence;

    [Header("Survival")]
    [SerializeField] Vector3 spawnPlayerOnePosition;
    [SerializeField] Vector3 spawnPlayerTwoPosition;
    [SerializeField] int spawnGravity = 12;
    [SerializeField] int spawnPostGravity = 1;
    [SerializeField] PhysicsMaterial2D spawnMaterial;
    [SerializeField] PhysicsMaterial2D spawnPostMaterial;
    [SerializeField] int timeFinalWait = 1;
    [SerializeField] float timeToChangeGravity = .5f;
    [SerializeField] int duelLives = 1;
    [SerializeField] float minMusicPitch = 1f;
    [SerializeField] float maxMusicPitch = 1.1f;
    [SerializeField] float musicPitchStartAt = 10;
    [SerializeField] float changeMusicPitchTime = .5f;
    [SerializeField] float showAlertAtSeconds = 10;

    [Header("Simulation")]
    [SerializeField] [Range(2, 100)] int simulationMinPlayer = 2;
    [SerializeField] [Range(2, 100)] int simulationMaxPlayer = 8;
    [SerializeField] [Range(1, 10)] int simulationMinTimeToSelectOption = 1;
    [SerializeField] [Range(1, 10)] int simulationMaxTimeToSelectOption = 2;
    bool simulation = false;

    [Header("Others")]
    [SerializeField] GameObject singleModeSessionPrefab;

    [Header("Spectators")]
    [SerializeField] GameObject confettiLeft;
    [SerializeField] GameObject confettiRight;
    [SerializeField] List<GameObject> projectilePrefabs = new List<GameObject>();
    [SerializeField] [Range(0,10)] float hornCooldown = 2f;
    [SerializeField] [Range(0,10)] float corncobCooldown = 1f;

    [Header("PlayerNotificacion")]
    [SerializeField] bool showPlayerNotification = true;
    [SerializeField] GameObject playerNotificationLeftPrefab;
    [SerializeField] GameObject playerNotificationRightPrefab;

    // Utiles para espectadores
    List<int> spectatorsHornActive = new List<int>();
    List<int> spectatorsCorncobActive = new List<int>();

    float deltaMusicPitch = 0;

    // Dificultad
    int playerOneTurn = 0;
    int playerTwoTurn = 0;
    int totalTurns = 1;
    Attacks[,] turnsAttacks = new Attacks[1, 2];

    // Sesión singlePlayer
    SingleModeSession singleModeSession;

    // Ataques de los personajes
    Attacks playerOneAttack;
    Attacks playerTwoAttack;

    // Matriz de resultados
    int[,] results = new int[5, 5] {
        { 0, -1, 1, -1, -1 }, { 1, 0, -1, -1, -1 }, { -1, 1, 0, -1, -1 }, { 1, 1, 1, 0, -1 }, { 1, 1, 1, 1, 0 }
    };

    // Indicación de primera configuración de nivel
    bool firstConfiguration = true;

    // Impedir entrar al opciones si ya se escogio ataque y aun no se define ganador 
    bool moveSelected = false;

    // Arbitro ya indico que esta lista para empezar la partida (Util para los triple shoot)
    bool refereeFirstShoutReady = false;

    // Configuracion del usuario
    int playerPosition = 0;

    // Variables para modo survival
    bool timeOut = false;
    bool arenaConfigReady = false;

    // Para pausas
    bool attackSequenceInCourse = false;
    bool isSuperAttackExecuting = false;
    bool isGoingToResult = false;
    bool isGoingToMainScreen = false;
    bool isGoingToResultScreen = false;

    // Configuraciones modo duelo
    int livesPerPlayer = 1;
    int minLivesPerPlayer = 3;
    int maxLivesPerPlayer = 11;
    int timePerDuel = 90;
    int minTimePerDuel = 10;
    int currentRound = 1;
    bool superJankenUsed = false;
    bool configuringNextLevel = true;
    int targetPlayerIndex = 0;
    int playerOneIndex = 0;
    int playerTwoIndex = 0;
    int playerOneCombo = 0;
    int playerTwoCombo = 0;
    int streakNecessary = 5;
    int minStreakNecessary = 5;
    int maxStreakNecessary = 5;
    bool showedFirstRound = false;
    List<int> playerRandomOrder = new List<int>();
    JankenUp.SinglePlayer.LevelType.Type levelType = JankenUp.SinglePlayer.LevelType.Type.Normal;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        GameController.SetGameplayActive(true);

        // Revisar si hay simulacion
        if (GameController.GetSimulation()) SetSimulation();

        // Generar la lista de jugadores desordenada
        GeneratePlayerRandomOrder();

        // Eventos de localizacion
        LanguageController.onLanguageChangeDelegate += Localize;

        // Impedir que la pantalla se vaya a negro
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Componentes
        audioSource = GetComponent<AudioSource>();

        // Calcular la maxima vida por jugador
        int playerInDuel = simulation ? CharacterPool.Instance.GetPlayersCount() : (CharacterPool.Instance.GetPlayersAllowedToDuel().Count > 2 ? CharacterPool.Instance.GetPlayersAllowedToDuel().Count : 0);
        livesPerPlayer = Mathf.Clamp(maxLivesPerPlayer - playerInDuel, minLivesPerPlayer, maxLivesPerPlayer);

        // Streak necesarios para ganar vida o poderes
        streakNecessary = Mathf.Clamp(maxStreakNecessary - playerInDuel, minStreakNecessary, maxStreakNecessary);

        // Obtener la sessión de juego asociada.
        singleModeSession = FindObjectOfType<SingleModeSession>();
        if (!singleModeSession)
        {
            singleModeSession = Instantiate(singleModeSessionPrefab).GetComponent<SingleModeSession>();
            singleModeSession.SetNoInitialSetup();
        }
        singleModeSession.SetIsShowingResults(false);
        singleModeSession.ConfigDuelMode(livesPerPlayer);

        // Obtener el secuenciados
        inGameSequence = FindObjectOfType<InGameSequence>();

        // Configurar las vidas del jugador
        playerOneLivesDisplayer.SetTotalLives(livesPerPlayer, livesPerPlayer);
        playerTwoLivesDisplayer.SetTotalLives(livesPerPlayer, livesPerPlayer);

        // Configurar streak de los jugadores
        playerOneLivesDisplayer.SetTotalStreak(streakNecessary, 0);
        playerTwoLivesDisplayer.SetTotalStreak(streakNecessary, 0);

        // Activar el despliegue de superpoderes
        playerOneLivesDisplayer.ShowSuperPowers();
        playerTwoLivesDisplayer.ShowSuperPowers();

        // Inicializar el displayer de Avatares
        miniAvatarListDisplayer.Init();

        // Configurar el nivel
        ConfigLevel();

        // Actualizar los textos
        UpdateCurrentFont();

        // Calcular diferencia de niveles para le pitch de la musica
        deltaMusicPitch = (maxMusicPitch - minMusicPitch) / musicPitchStartAt;

        // Configurar los tiempos del timer si es que ya se habian seteado en la sesion
        timeDisplayer.SetRemainingTime(timePerDuel);
        timeDisplayer.SetElapsedTime(0);

        // Referee no puede desaparecer (a menos que se utilice un super en el ultimo round)
        referee.SetImmuneToDisappear(true);

        // Solo mostrar notificaciones de personajes si hay mas de 3 jugadores
        if (showPlayerNotification && !singleModeSession.AtLeastXPlayersAlive(3)) showPlayerNotification = false;

        // Iniciar partida
        StartCoroutine(StartMatch());
    }

    /// <summary>
    /// Configurar simulacion de jugadores
    /// </summary>
    private void SetSimulation()
    {
        simulation = true;
        simulationCanvas.SetActive(true);
        simulationCanvas.GetComponentInChildren<Button>().onClick.AddListener(delegate {
            if(!isGoingToMainScreen) GoToMainScreen();
        });

        // Bloquear el menu de opciones
        if (optionsMenu) optionsMenu.enabled = false;

        int maxPlayers = Random.Range(simulationMinPlayer, simulationMaxPlayer + 1);

        CharacterPool.Instance.ResetPlayerList();

        for (int i = 0; i < maxPlayers; i++)
        {
            CharacterPool.Instance.SetCurrentCharacterIdentifier(CharacterPool.Instance.Get(Random.Range(0, CharacterPool.Instance.Length())).GetComponent<CharacterConfiguration>().GetIdentifier(), i);
        }
    }

    /// <summary>
    /// Simulacion para lanzar objetos
    /// </summary>
    /// <returns></returns>
    private IEnumerator SimulateObjects()
    {
        do {
            EmitHorn(Random.Range(0, playerRandomOrder.Count), Random.Range(0, 2) == 1);
            ThrowCorbcob(Random.Range(0, playerRandomOrder.Count), Random.Range(0, 2) == 1);
            yield return new WaitForSeconds(hornCooldown);
        } while (singleModeSession.AtLeastXPlayersAlive() && !timeOut);
    }

    /// <summary>
    /// Realizar ataques de jugadores de manera aleatoria
    /// </summary>
    /// <returns></returns>
    private IEnumerator SimulationAttacksCoroutine()
    {
        do
        {
            // Simular lanzamiento de objetos/apoyo
            StartCoroutine(SimulateObjects());

            int timeToWait = Random.Range(simulationMinTimeToSelectOption, simulationMaxTimeToSelectOption);
            yield return new WaitForSeconds(timeToWait);

            // Aplicar para todos los turnos
            for (int i = 0; i < totalTurns; i++)
            {
                // Revisar si jugador tiene super poder y randomizar utilizarlo
                int playerOneOption = -1;
                int playerTwoOption = -1;

                // De manera arbitraria, si el random es igual a X, se utilizara el ataque
                if (singleModeSession.GetPlayerMagicWand(playerOneIndex) > 0 && Random.Range(0, 5) == 1) playerOneOption = (int) Attacks.MagicWand;
                else if (singleModeSession.GetPlayerSuperJanken(playerOneIndex) > 0 && Random.Range(0, 5) == 2) playerOneOption = (int)Attacks.JanKenUp;

                if (singleModeSession.GetPlayerMagicWand(playerTwoIndex) > 0 && Random.Range(0, 5) == 3) playerTwoOption = (int)Attacks.MagicWand;
                else if (singleModeSession.GetPlayerSuperJanken(playerTwoIndex) > 0 && Random.Range(0, 5) == 4) playerTwoOption = (int)Attacks.JanKenUp;

                SelectOption(playerOneOption, playerOneIndex);
                SelectOption(playerTwoOption, playerTwoIndex);
            }
            
        } while (singleModeSession.AtLeastXPlayersAlive() && !timeOut);

    }

    /// <summary>
    /// Generacion del orden al azar de jugadores a participar
    /// </summary>
    private void GeneratePlayerRandomOrder()
    {
        playerRandomOrder = simulation ? Enumerable.Range(0, CharacterPool.Instance.GetPlayersCount()).ToList() : CharacterPool.Instance.GetPlayersAllowedToDuel();
        if (playerRandomOrder.Count <= 2) return;
        
        int count = playerRandomOrder.Count;
        int last = count - 1;
        for (int i = 0; i < count - 1; ++i){
            int r = Random.Range(i, count);
            int tmp = playerRandomOrder[i];
            playerRandomOrder[i] = playerRandomOrder[r];
            playerRandomOrder[r] = tmp;
        }
    }

    /// <summary>
    /// Revision si se debe tocar la musica especial asignada al personaje. De lo contrario, ejecutar codigo normal
    /// </summary>
    protected override void PlayMusic()
    {
        StartCoroutine(WaitForPlayersOrder());
    }

    private IEnumerator WaitForPlayersOrder()
    {
        while (playerRandomOrder.Count == 0) yield return null;
        int musicFromIndex = playerRandomOrder[Random.Range(0, playerRandomOrder.Count)];
        CharacterConfiguration playerConf = CharacterPool.Instance.GetCurrentCharacter(musicFromIndex).GetComponent<CharacterConfiguration>();
        if (playerConf.HasCustomMusic()) currentMusic = playerConf.GetCharacterMusic();
        base.PlayMusic();
    }

    // Override de Update para poder cambiar el pitch de la musica
    new void Update()
    {
        if (openDoor)
        {
            openDoor = false;
            StartCoroutine(SetReady());
        }


        if (!timeOut && singleModeSession.AtLeastXPlayersAlive()) CheckForTime();

        // Agregar el tiempo trasncurrido a la sesion
        if (timeDisplayer.IsTimerRunning())
        {
            singleModeSession.SetTimeElapsed(timeDisplayer.GetTimeElapsed());
            singleModeSession.SetTimeRemaining(timeDisplayer.GetTimeRemaining());
        }
    }

    /// <summary>
    /// Revision de si aun queda tiempo en la partida
    /// </summary>
    private void CheckForTime()
    {
        float currentTime = timeDisplayer.GetTimeRemaining();
        if (currentTime <= musicPitchStartAt && currentTime > 0)
            MasterAudioPlayer._player.ChangePitchAudio(Mathf.Clamp((musicPitchStartAt - currentTime) * deltaMusicPitch + minMusicPitch, minMusicPitch, maxMusicPitch));
        else MasterAudioPlayer._player.ChangePitchAudio(minMusicPitch);
    }

    /// <summary>
    /// Reconfigurar la escena de juego
    /// </summary>
    private void ConfigLevel()
    {
        configuringNextLevel = true;

        // Actualizar el UI
        UpdateUI();

        // Revisar la cantidad de vidas
        singleModeSession.CheckForLives();

        // Obtener arena
        if (!arena) arena = FindObjectOfType<ArenaController>();

        // Realizar reemplazo de los actores en pantalla
        if (firstConfiguration)
        {
            // Configurar a los jugadores
            ReplaceActors();

            // Comprobar si alguno de los jugadores necesita un NPC asociado
            ActorsNeedSupportNpc();
        }
        else
        {
            // Indicar al secuenciador quien es el nuevo objeto de la izquierda
            if (inGameSequence) inGameSequence.SetActors(playerOne, playerTwo);

            // Revisar que tipo de juego sera
            totalTurns = (singleModeSession.GetLives(playerOneIndex) <= 1 || singleModeSession.GetLives(playerTwoIndex) <= 1) ? 3 : 1;
            levelType = totalTurns == 3 ? JankenUp.SinglePlayer.LevelType.Type.Triple : JankenUp.SinglePlayer.LevelType.Type.Normal;

        }

        // Resetear al arbitro
        referee.ChangeState(InGameStates.Stand);
        playerOneController.ChangeState(InGameStates.Stand);
        playerTwoController.ChangeState(InGameStates.Stand);

        // Resetear el stack de ataques
        ResetStackAttacks();

        // Configurar arena
        if (!arenaConfigReady)
        {
            arenaConfigReady = true;
            arena.SetNotAllowedSpecialNPC(null, true);
            arena.Config();
        }

        // Mostrar el UI innecesario
        ToggleAttackUI(true);

        // Indicar que no se ha seleccionado ataque
        moveSelected = false;
        refereeFirstShoutReady = false;
        //pauseButton.SetActive(true);
        configuringNextLevel = false;
    }

    /// <summary>
    /// Cerrar las puertas y reconfigurar el juego
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReStartMatch()
    {
        TransitionDoors doors = FindObjectOfType<TransitionDoors>();

        // Configurar nivel
        ConfigLevel();

        yield return StartCoroutine(doors.Open());

        // Iniciar partida
        StartCoroutine(StartMatch());
    }

    // Inicio del juego
    private IEnumerator StartMatch()
    {
        // Indicar que joysticks deben vibrar
        JoystickSupport.Instance.SetGamepadToVibrate(new List<int> { playerOneIndex, playerTwoIndex });

        // Si no esta aún abiertas las puertas, esperar
        while (!isReady) yield return null;

        // Mostrar primera ronda
        if (!showedFirstRound)
        {
            showedFirstRound = true;
            yield return StartCoroutine(ShowRound());
        }

        // Revisar si el referee debe anunciar algo segun el tipo de juego
        if (!refereeFirstShoutReady)
        {
            // Vibrar para indicar a los jugadores quienes participan
            if(JoystickSupport.Instance.SupportActivated()) VibrationController.Instance.Vibrate();

            switch (levelType)
            {
                case JankenUp.SinglePlayer.LevelType.Type.Triple:
                    timeDisplayer.Toggle(false);
                    yield return referee.Announcement(JankenUp.SinglePlayer.LevelType.Triple.announcement, 2);
                    timeDisplayer.Toggle(true);
                    yield return timeDisplayer.GetTimeToToggle();
                    break;
            }
            refereeFirstShoutReady = true;
        }

        // Mostrar las opciones de ataque
        ToggleAttackUI(true);

        // Iniciar countdown
        StartCoroutine(CountdownSequence());
    }

    // Actualización de la UI de vidas, monedas
    private void UpdateUI()
    {
        if (singleModeSession)
        {
            playerOneLivesDisplayer.SetAvatar(singleModeSession.GetPlayer(playerOneIndex).GetComponent<CharacterConfiguration>().GetAvatar());
            playerOneLivesDisplayer.SetCurrentLives(singleModeSession.GetLives(playerOneIndex), true);
            playerTwoLivesDisplayer.SetAvatar(singleModeSession.GetPlayer(playerTwoIndex).GetComponent<CharacterConfiguration>().GetAvatar());
            playerTwoLivesDisplayer.SetCurrentLives(singleModeSession.GetLives(playerTwoIndex), true);
            playerOneLivesDisplayer.SetCurrentStreak(singleModeSession.GetPlayerStreak(playerOneIndex));
            playerTwoLivesDisplayer.SetCurrentStreak(singleModeSession.GetPlayerStreak(playerTwoIndex));

            // Actualizar los superpoderes
            playerOneLivesDisplayer.ActivateMagicWand(singleModeSession.GetPlayerMagicWand(playerOneIndex) != 0);
            playerOneLivesDisplayer.ActivateSuperJanken(singleModeSession.GetPlayerSuperJanken(playerOneIndex) != 0);
            playerTwoLivesDisplayer.ActivateMagicWand(singleModeSession.GetPlayerMagicWand(playerTwoIndex) != 0);
            playerTwoLivesDisplayer.ActivateSuperJanken(singleModeSession.GetPlayerSuperJanken(playerTwoIndex) != 0);

            // Revisar por vidas
            singleModeSession.CheckForLives();
        }
    }

    // Actualización del jugador y de la CPU
    private void ReplaceActors()
    {
        // Obtener la sessión de juego asociada. Instanciar el player correctamente
        if (singleModeSession)
        {
            if (firstConfiguration)
            {
                firstConfiguration = false;

                // Obtener las posiciones iniciales de los personajes
                playerOneInitialPosition = playerOne.transform.position;
                playerTwoInitialPosition = playerTwo.transform.position;
                playerOneIndex = playerRandomOrder[targetPlayerIndex++];

                GameObject newPlayer = Instantiate(
                    singleModeSession.GetPlayer(playerOneIndex),
                    playerOne.transform.position,
                    Quaternion.identity
                );

                // Destruir el jugador actual y asignar nueva referencia
                newPlayer.transform.parent = playerOne.transform.parent;
                Destroy(playerOne);
                playerOne = newPlayer;
                playerOneController = playerOne.GetComponent<CharacterInGameController>();
                playerOneController.Flip(playerPosition == 1 ? -1 : 1);
            }

            // Posicion correcta del jugador
            playerOne.transform.position = playerOneInitialPosition;
            playerTwoIndex = playerRandomOrder[targetPlayerIndex++];

            // Realizar lo mismo con la CPU
            GameObject randomCPU = Instantiate(
                singleModeSession.GetPlayer(playerTwoIndex),
                playerTwo.transform.position,
                Quaternion.identity
                );
            randomCPU.transform.parent = playerTwo.transform.parent;
            Destroy(playerTwo);
            playerTwo = randomCPU;
            playerTwoController = playerTwo.GetComponent<CharacterInGameController>();
            playerTwoController.Flip(playerPosition == 0 ? -1 : 1);

            // Posicion correcta del adversario
            playerTwo.transform.position = playerTwoInitialPosition;

            // Colocar avatares
            playerOneLivesDisplayer.SetAvatar(singleModeSession.GetPlayer(playerOneIndex).GetComponent<CharacterConfiguration>().GetAvatar());
            playerTwoLivesDisplayer.SetAvatar(singleModeSession.GetPlayer(playerTwoIndex).GetComponent<CharacterConfiguration>().GetAvatar());
        }

        // Añadir control
        playerOneController.SetControl(true);
        playerTwoController.SetControl(true);

        // Si el arbitro esta como jugador o contrincante, activar el backup
        referee.ToggleBackup(CharacterPool.Instance.Get("referee").GetComponent<CharacterConfiguration>().IsUnlocked());

        // Volver a la normalidad al jugador en caso de que haya sido mandado a volar (Lo mismo con el referee)
        referee.Reappear();
        playerOneController.Reappear();

        // Indicar al secuenciador quien es el nuevo objeto de la izquierda
        if (inGameSequence) inGameSequence.SetActors(playerOne, playerTwo);

        SetPlayersName();
        UpdateUI();

        // Revisar los NPC a ocultar
        List<string> npcIdentifiers = new List<string> { playerOneController.GetIdentifier(), playerTwoController.GetIdentifier() };
        arena.HideNPCInMatch(npcIdentifiers);
    }

    /// <summary>
    /// Setea el nombre de los jugadores
    /// </summary>
    private void SetPlayersName()
    {
        var playerOneData = new[] { new { player = playerOneIndex + 1 } };
        LocalizationHelper.FormatTranslate(playerOneLivesDisplayer.GetPlayerLabel(), JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.player_x, playerOneData);

        var playerTwoData = new[] { new { player = playerTwoIndex + 1 } };
        LocalizationHelper.FormatTranslate(playerTwoLivesDisplayer.GetPlayerLabel(), JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.player_x, playerTwoData);
    }

    /// <summary>
    /// Reaparicion del jugador eliminado
    /// </summary>
    /// <param name="player2"></param>
    /// <returns></returns>
    private GameObject SpawnPlayer(bool player2 = false)
    {
        if (timeOut) return null;

        // Remover el colider2d del jugador target
        Destroy(player2 ? playerTwo.GetComponent<Collider2D>() : playerOne.GetComponent<Collider2D>());

        // Realizar instanciacion de jugadoren base al ultimo indice
        List<int> indexNotToConsider = new List<int>();
        if (singleModeSession.AtLeastXPlayersAlive(3))
        {
            indexNotToConsider.Add(playerOneIndex);
            indexNotToConsider.Add(playerTwoIndex);
        }
        else indexNotToConsider.Add(player2 ? playerOneIndex : playerTwoIndex);

        // Aumentar el streak del ganador
        if (player2)
        {
            singleModeSession.AddPlayersStreak(playerOneIndex);
            singleModeSession.ResetPlayerStreak(playerTwoIndex);
            CheckStrikeForPlayer(false);
        }
        else
        {
            singleModeSession.AddPlayersStreak(playerTwoIndex);
            singleModeSession.ResetPlayerStreak(playerOneIndex);
            CheckStrikeForPlayer(true);
        }

        // Actualizar las vidas para los displayer
        miniAvatarListDisplayer.Refresh(playerOneIndex);
        miniAvatarListDisplayer.Refresh(playerTwoIndex);

        if (singleModeSession.AtLeastXPlayersAlive())
        {
            // Encontrar el indice del siguiente jugador con vida (Si o Si hay 2 jugadores con vida)
            while (true){
                if (targetPlayerIndex >= playerRandomOrder.Count) targetPlayerIndex = 0;
                if (singleModeSession.GetLives(playerRandomOrder[targetPlayerIndex]) > 0 && !indexNotToConsider.Contains(playerRandomOrder[targetPlayerIndex])) break;
                targetPlayerIndex++;
            }

            GameObject newPlayer = Instantiate( singleModeSession.GetPlayer(playerRandomOrder[targetPlayerIndex]),
            player2 ? spawnPlayerTwoPosition : spawnPlayerOnePosition,
            Quaternion.identity
            );
            newPlayer.transform.parent = player2 ? playerTwo.transform.parent : playerOne.transform.parent;
            CharacterInGameController characterInGamecontroller = newPlayer.GetComponent<CharacterInGameController>();
            characterInGamecontroller.Flip(player2 ? -1 : 1);

            // Posicion correcta del jugador
            newPlayer.transform.position = player2 ? spawnPlayerTwoPosition : spawnPlayerOnePosition;

            // Alterar el rigidbody para una caida rapida
            Rigidbody2D newPlayerRigidbody2D = newPlayer.GetComponent<Rigidbody2D>();
            newPlayerRigidbody2D.sharedMaterial = spawnMaterial;
            newPlayerRigidbody2D.gravityScale = spawnGravity;

            // Indicar al controlador que debe hacer el cambio de material al tocar plataforma
            characterInGamecontroller.AlterRigidBodyOnContact(spawnPostMaterial, spawnPostGravity);

            // Indicar el indice para el jugador
            if (player2)
            {
                playerTwoCombo = 0;
                playerTwoIndex = playerRandomOrder[targetPlayerIndex++];
            }
            else
            {
                playerOneCombo = 0;
                playerOneIndex = playerRandomOrder[targetPlayerIndex++];
            }

            List<string> npcIdentifiers = new List<string> { characterInGamecontroller.GetIdentifier() };
            arena.HideNPCInMatch(npcIdentifiers);

            return newPlayer;
        }
        else
        {
            Destroy(player2 ? playerTwo : playerOne);
            return null;
        }

    }

    /// <summary>
    /// Terminar la aparicion del nuevo jugador
    /// </summary>
    /// <param name="newPlayer"></param>
    /// <param name="player2"></param>
    private void FinishSpawnPlayer(GameObject newPlayer, bool player2 = false)
    {
        if (!newPlayer) return;

        // Destruir y asignar la nueva CPU
        Destroy(player2 ? playerTwo : playerOne);
        if (player2 && singleModeSession.GetLives(playerTwoIndex) > 0)
        {
            playerTwo = newPlayer;
            playerTwoController = playerTwo.GetComponent<CharacterInGameController>();
            playerTwoController.SetControl(true);
        }
        else if (singleModeSession.GetLives(playerOneIndex) > 0)
        {
            playerOne = newPlayer;
            playerOneController = playerOne.GetComponent<CharacterInGameController>();
            playerOneController.SetControl(true);
        }

        SetPlayersName();

        // Revisar si el nuevo CPU necesita soporte
        ActorsNeedSupportNpc();

        // Realizar la notificacion de jugadores
        if (showPlayerNotification) ShowPlayerNotification(player2);

        // Solo mostrar notificaciones si quedan >2 personajes con vida
        if (showPlayerNotification && !singleModeSession.AtLeastXPlayersAlive(3)) showPlayerNotification = false;
    }

    /// <summary>
    /// Muestra la notificacion del nuevo jugador 
    /// </summary>
    /// <param name="player2"></param>
    private void ShowPlayerNotification(bool player2 = false)
    {
        PlayerNotification playerNotification = Instantiate(player2 ? playerNotificationRightPrefab : playerNotificationLeftPrefab, UICanvas.transform).GetComponent<PlayerNotification>();
        playerNotification.Setup(player2 ? playerTwoController.GetComponent<CharacterConfiguration>() : playerOneController.GetComponent<CharacterConfiguration>(),
            player2 ? playerTwoIndex : playerOneIndex);
    }

    /// <summary>
    /// Revision de si la racha actual constituye un checkpoint para el jugador
    /// </summary>
    /// <param name="player2"></param>
    private void CheckStrikeForPlayer(bool player2 = false) {
        int targetIndex = player2 ? playerTwoIndex : playerOneIndex;
        int currentStreak = singleModeSession.GetPlayerStreak(targetIndex);
        if(currentStreak >= streakNecessary)
        {
            singleModeSession.ResetPlayerStreak(targetIndex);

            // Otorgar vida o poder
            if(singleModeSession.GetLives(targetIndex) == livesPerPlayer)
            {
                // Ver si es posible obtener un poder (Siempre por orden se obtiene una varita magica y si no, un super)
                SuperPowers superPower = SuperPowers.MagicWand;
                bool gainedSuperpower = false;

                if (singleModeSession.GetPlayerMagicWand(targetIndex) == 0) {
                    gainedSuperpower = true;
                    singleModeSession.AddPlayerMagicWand(targetIndex);
                }
                else if(singleModeSession.GetPlayerSuperJanken(targetIndex) == 0)
                {
                    gainedSuperpower = true;
                    superPower = SuperPowers.JanKenUp;
                    singleModeSession.AddPlayerSuperJanken(targetIndex);
                }

                if (gainedSuperpower)
                {
                    MasterSFXPlayer._player.WinSuperPower();
                    if (player2) playerTwoController.StartShowGetSuperPower(superPower);
                    else playerOneController.StartShowGetSuperPower(superPower);
                }
                
            }
            else
            {
                singleModeSession.AddPlayerLives(targetIndex);
                miniAvatarListDisplayer.Refresh(targetIndex);

                // Nota: Se utilizo la misma funcion de superpoder para desplegar la vida
                if (player2) playerTwoController.StartShowGetSuperPower(SuperPowers.JanKenUp, true);
                else playerOneController.StartShowGetSuperPower(SuperPowers.JanKenUp, true);
            }

            if (player2) playerTwoLivesDisplayer.StartFullStrike();
            else playerOneLivesDisplayer.StartFullStrike();

        }
    }

    /// <summary>
    /// Selección de ataque
    /// </summary>
    /// <param name="attack"></param>
    public void SelectOption(int attack, int playerIndex = 0)
    {
        // Solo los jugadores en juego pueden indicar un ataque
        if (configuringNextLevel || timeOut || !TransitionDoors._this.IsTotallyOpen() || singleModeSession.GetIsShowingResults() ||
            (playerIndex != playerOneIndex && playerIndex != playerTwoIndex)) return;

        // Si no quedan turnos
        if ( (playerIndex == playerOneIndex && playerOneTurn >= totalTurns)
            || (playerIndex == playerTwoIndex && playerTwoTurn >= totalTurns)) return;

        // Revisar que tipo de movimiento es
        Attacks playerPreAttack = IntToAttack(attack);

        // Si el ataque es un MagicWand o MegaPunch, revisar si es posible utilizarlo
        bool isPossible = true;

        if (playerPreAttack == Attacks.MagicWand)
        {
            isPossible = CheckMagicWand(playerIndex);
        }
        else if (playerPreAttack == Attacks.JanKenUp)
        {
            isPossible = CheckSuperJanken(playerIndex);
        }

        if (!isPossible) return;

        // Indicar que se ha seleccionado ataque
        moveSelected = true;

        // Reproducir sonido de selección
        audioSource.PlayOneShot(attackSelection);

        // Si esta el anuncio del arbrito, desactivar
        referee.HideAnnouncement();

        // Guardar el ataque del jugador
        if (playerIndex == playerTwoIndex)
        {
            playerTwoAttack = playerPreAttack;

            // Si fue un superataque, marcar todos los turnos
            if (playerPreAttack == Attacks.JanKenUp)
            {
                playerTwoTurn = 0;
                for (int i = 0; i < totalTurns; i++)
                {
                    StackAttacks(playerTwoAttack, true);
                    playerTwoTurn++;
                }
            }
            else
            {
                StackAttacks(playerTwoAttack, true);
                playerTwoTurn++;
            }

            if (playerTwoTurn >= totalTurns) playerTwoLivesDisplayer.ShowReady();
        }
        else
        {
            playerOneAttack = playerPreAttack;

            // Si fue un superataque, marcar todos los turnos
            if (playerPreAttack == Attacks.JanKenUp)
            {
                playerOneTurn = 0;
                for (int i = 0; i < totalTurns; i++)
                {
                    StackAttacks(playerOneAttack);
                    playerOneTurn++;
                }
            }
            else
            {

                StackAttacks(playerOneAttack);
                playerOneTurn++;
            }

            if (playerOneTurn >= totalTurns) playerOneLivesDisplayer.ShowReady();
        }

        // ¿Quedan turnos?
        if (playerOneTurn >= totalTurns && playerTwoTurn >= totalTurns)
        {
            currentCountdownSteps = -1;

            // Detener la coroutine de conteo e iniciar la coroutine ataque
            StopAllCoroutines();

            // Mostrar tiempo si esta oculto
            timeDisplayer.Toggle(true);

            // Iniciar la siguiente rutina
            StartCoroutine(AttackSequence());
        }

    }

    /// <summary>
    /// Revision de posibilidad de usar varita magica por parte de un jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    private bool CheckMagicWand(int playerIndex = 0)
    {
        if (!TransitionDoors._this.IsTotallyOpen() || (singleModeSession && singleModeSession.GetIsShowingResults())) return false;

        // Ver si es posible utilizar el MagicWand
        if (singleModeSession.SubstractMagicWand(playerIndex))
        {
            return true;
        }
        else
        {
            // Reproducir sonido de erroneo
            MasterSFXPlayer._player.Error();
            return false;
        }
    }

    /// <summary>
    /// Seleccion de SuperJanKenUP
    /// </summary>
    /// <param name="playerIndex"></param>
    public bool CheckSuperJanken(int playerIndex = 0)
    {
        if (!TransitionDoors._this.IsTotallyOpen() || (singleModeSession && singleModeSession.GetIsShowingResults())) return false;

        // Ver si es posible utilizar el MagicWand
        if (singleModeSession.SubstractSuperJanken(playerIndex))
        {
            superJankenUsed = true;
            return true;
        }
        else
        {
            // Reproducir sonido de erroneo
            MasterSFXPlayer._player.Error();
            return false;
        }
    }

    /// <summary>
    ///  Resetear el contador
    /// </summary>
    public void ResetCountdown()
    {
        // Detener la coroutine de conteo e iniciar la coroutine ataque
        StopAllCoroutines();

        // Iniciar nuevamente el contador
        StartCoroutine(CountdownSequence());
    }

    /// <summary>
    /// Secuencia que indica cuanto tiempo queda para seleccionar el ataque
    /// </summary>
    /// <returns></returns>
    public IEnumerator CountdownSequence()
    {
        while (overlayObjects.Count > 0) yield return null;

        // Iniciar el parpadeo
        if(levelType == JankenUp.SinglePlayer.LevelType.Type.Triple || timeDisplayer.GetTimeRemaining() <= showAlertAtSeconds) alertCanvas.Blink();

        // Mostrar el UI innecesario
        ToggleAttackUI(true);

        // Iniciar el contador de tiempo
        timeDisplayer.StartTimer();

        if(simulation) StartCoroutine(SimulationAttacksCoroutine());

        // Ir a siguiente paso
        if (currentCountdownSteps == -1) currentCountdownSteps = steps;
        for (int i = currentCountdownSteps; i > 0; i--)
        {
            if (!timeOut)
            {
                referee.PlaySFX();
                referee.Step(i);
                currentCountdownSteps--;
                yield return new WaitForSeconds(1);
            }
        }
        currentCountdownSteps = -1;

        // Se acabo el tiempo, elegir al azar un ataque
        if (!timeOut)
        {
            for (int i = 0; i < totalTurns; i++)
            {
                SelectOption(-1, playerOneIndex);
                SelectOption(-1, playerTwoIndex);
            }
        }
        else StartCoroutine(GoToResults());

    }

    /// <summary>
    /// Convertir un int a un ataque enumerado
    /// </summary>
    /// <param name="attack"></param>
    /// <returns></returns>
    private Attacks IntToAttack(int attack)
    {
        Attacks transformed = Attacks.Rock;

        // Si es negativo, seleccionar de manera random. Solo se toma en consideracion los ataques normales que van del 0 a 2
        if (attack == -1) attack = Random.Range(0, 3);

        // Transformar a ataque
        switch (attack)
        {
            case 0:
                transformed = Attacks.Rock;
                break;
            case 1:
                transformed = Attacks.Paper;
                break;
            case 2:
                transformed = Attacks.Scissors;
                break;
            case 3:
                transformed = Attacks.MagicWand;
                break;
            case 4:
                transformed = Attacks.JanKenUp;
                break;
        }

        return transformed;

    }

    /// <summary>
    /// Convertir un int a un ataque enumerado además de verificar que no sea igual al pivote
    /// </summary>
    /// <param name="attack"></param>
    /// <param name="pivot"></param>
    /// <returns></returns>
    private Attacks IntToAttack(int attack, Attacks pivot)
    {
        Attacks transformed;
        do
        {
            transformed = IntToAttack(attack);
        } while (transformed == pivot);

        return transformed;
    }

    /// <summary>
    /// Secuencia donde se muestra los ataques y se indica el ganador
    /// </summary>
    /// <returns></returns>
    public IEnumerator AttackSequence()
    {
        // Detener la alerta en el Canvas
        alertCanvas.Stop();

        // Esconder fintas
        HideFeint();

        // Esconder readys
        playerOneLivesDisplayer.HideReady();
        playerTwoLivesDisplayer.HideReady();

        // Indicar que se esta viendo los ataques
        attackSequenceInCourse = true;
        singleModeSession.SetIsShowingResults(true);

        // Detener el contador de tiempo si queda la nada de tiempo
        float timeRemaining = timeDisplayer.GetTimeRemaining();
        if (superJankenUsed || (0 < timeRemaining && timeRemaining <= 3)) timeDisplayer.StopTimer();

        // Ver quién ganó
        InGameStates result = CalcWholeResult();

        // Agregar estadisticas
        if (result.Equals(InGameStates.Draw)) {
            singleModeSession.AddPlayersTie(playerOneIndex);
            singleModeSession.AddPlayersTie(playerTwoIndex);
        }
        else
        {
            singleModeSession.AddPlayerVictory(result.Equals(InGameStates.Win) ? playerOneIndex : playerTwoIndex);
            singleModeSession.AddPlayerDefeat(result.Equals(InGameStates.Win) ? playerTwoIndex : playerOneIndex);
        }

        // Quitar el UI innecesario
        ToggleAttackUI(false);

        // Quitar el dialogo del referee
        referee.ShowDialog(false);

        // Restar vida al jugador que corresponda
        if (!result.Equals(InGameStates.Draw)) singleModeSession.SubtractLives(result == InGameStates.Win ? playerTwoIndex : playerOneIndex);

        if (superJankenUsed)
        {
            // Rutina de superJanken
            yield return CoroutineJanKenUp(turnsAttacks);

            // Mostrar combos
            ShowCombos();
        }
        else
        {
            // Por cada turno, mostrar el ataque correspondiente
            for (int i = 0; i < totalTurns; i++)
            {
                // Obtener el resultado parcial
                InGameStates parcialResult = CalcResult(turnsAttacks[i, 0], turnsAttacks[i, 1]);

                // Indicar a los actores su ataque
                playerOneController.ChangeCurrentAttack(turnsAttacks[i, 0]);
                playerTwoController.ChangeCurrentAttack(turnsAttacks[i, 1]);

                // Si es un ataque tipo destino o es el ultimo tiro de una jugada triple, se debe eliminar a uno de los participantes
                //bool strong = result != InGameStates.Draw;
                bool strong = (levelType == JankenUp.SinglePlayer.LevelType.Type.Triple && i == totalTurns - 1 && result != InGameStates.Draw)
                || (levelType == JankenUp.SinglePlayer.LevelType.Type.Normal && result != InGameStates.Draw);

                if (inGameSequence) yield return StartCoroutine(inGameSequence.ParcialGameCourutine(2));

                if (strong) PlayStrongHit(levelType == JankenUp.SinglePlayer.LevelType.Type.Triple || levelType == JankenUp.SinglePlayer.LevelType.Type.Destiny);

                playerOneStack.ChangeAttack(i, turnsAttacks[i, 0]);
                playerTwoStack.ChangeAttack(i, turnsAttacks[i, 1]);

                // Mostrar combos
                ShowCombos();

                // Calcular el step correspondiente para la animacion de la camara
                int cameraStep = totalTurns > 1 ? i + 1 : 0;

                // Ver que resultado enviar: Si es una jugada por turno, solo al final se envia el resultado global
                InGameStates resultToSend = totalTurns == 1 || (i == totalTurns - 1) ? result : parcialResult;

                // Hacer aparecer al jugador que fue eliminado (Win player 2 Lose player 1)
                GameObject newPlayer = null;
                if (!result.Equals(InGameStates.Draw) && i == totalTurns - 1) {
                    // Actualizar displayers de vida
                    miniAvatarListDisplayer.Refresh(playerOneIndex);
                    miniAvatarListDisplayer.Refresh(playerTwoIndex);
                    newPlayer = SpawnPlayer(result.Equals(InGameStates.Win));
                }

                // Ver resultado de la jugada y activar en player o cpu
                switch (parcialResult)
                {
                    case InGameStates.Win:
                        ActivateStackAttack(i, playerOneStack);
                        break;
                    case InGameStates.Lose:
                        ActivateStackAttack(i, playerTwoStack);
                        break;
                }

                if (inGameSequence) yield return StartCoroutine(inGameSequence.FinishParcialGameRoutine(strong, resultToSend, cameraStep));

                if (!result.Equals(InGameStates.Draw) && i == totalTurns - 1) FinishSpawnPlayer(newPlayer, result.Equals(InGameStates.Win));
            }
        }

        // Si solo hay 1 jugador con vidas
        if (!singleModeSession.AtLeastXPlayersAlive()) timeDisplayer.StopTimer();

        // Si fue un empate, iniciar nuevamente el conteo
        if (result.Equals(InGameStates.Draw))
        {
            // Resetear los pj a su estado stand
            referee.ChangeState(InGameStates.Stand);
            playerOneController.ChangeState(InGameStates.Stand);
            playerTwoController.ChangeState(InGameStates.Stand);

            // Resetear ataques
            ResetStackAttacks();

            // Iniciar countdown si queda tiempo
            if (!singleModeSession.AtLeastXPlayersAlive() || timeOut) StartCoroutine(GoToResults());
            else StartCoroutine(CountdownSequence());

        }
        else
        {
            // Considerar vidas de todos los jugadores
            if (!singleModeSession.AtLeastXPlayersAlive() || timeOut) StartCoroutine(GoToResults());
            else StartCoroutine(ReStartMatch());

        }

        // Actualización de UI
        UpdateUI();

        // Indicar que no se ha seleccionado ataque
        moveSelected = false;
        attackSequenceInCourse = false;
        singleModeSession.SetIsShowingResults(false);

    }

    /// <summary>
    /// Remover las fintas de los jugadores
    /// </summary>
    private void HideFeint()
    {
        playerOneController.HideFeint();
        playerTwoController.HideFeint();
    }

    /// <summary>
    /// Actualizar UI de combos
    /// </summary>
    private void ShowCombos()
    {
        // Mostrar combos
        if (playerOneCombo == 0) comboDisplayerPlayerOne.Hide();
        else comboDisplayerPlayerOne.SetValue(playerOneCombo);
        if (playerTwoCombo == 0) comboDisplayerPlayerTwo.Hide();
        else comboDisplayerPlayerTwo.SetValue(playerTwoCombo);
    }

    /// <summary>
    /// Calculo del resultado de la jugada
    /// </summary>
    /// <param name="attackOne"></param>
    /// <param name="attackTwo"></param>
    /// <returns></returns>
    public InGameStates CalcResult(Attacks attackOne, Attacks attackTwo)
    {
        // Utilizar la matriz de resultados y ver el valor. Se relaciona a Attacks
        int resultInt = results[(int)attackOne, (int)attackTwo];
        InGameStates result;
        switch (resultInt)
        {
            case 1:
                playerOneCombo++;
                playerTwoCombo = 0;
                result = InGameStates.Win;
                break;
            case -1:
                playerOneCombo = 0;
                playerTwoCombo++;
                result = InGameStates.Lose;
                break;
            case 0:
            default:
                playerOneCombo = 0;
                playerTwoCombo = 0;
                result = InGameStates.Draw;
                break;
        }

        if (singleModeSession)
        {
            // Agregar a la secuencia de ataques de los jugadores
            singleModeSession.AddPlayersAttack(attackOne, playerOneIndex);
            singleModeSession.AddPlayersAttack(attackTwo, playerTwoIndex);

            // Guardar los combos
            singleModeSession.AddPlayersCombos(playerOneCombo, playerOneIndex);
            singleModeSession.AddPlayersCombos(playerTwoCombo, playerTwoIndex);

        }

        return result;
    }

    /// <summary>
    /// Calculo del resultado total según los turnos
    /// </summary>
    /// <returns></returns>
    public InGameStates CalcWholeResult()
    {
        // Resultados generales
        int playerCount = 0;

        string p1 = "";
        string p2 = "";
        for (int i = 0; i < totalTurns; i++)
        {
            p1 += turnsAttacks[i, 0];
            p2 += turnsAttacks[i, 1];
        }

        // Por cada turno, mostrar el ataque correspondiente
        for (int i = 0; i < totalTurns; i++)
        {
            // Indicar a los actores su ataque
            playerCount += results[(int)turnsAttacks[i, 0], (int)turnsAttacks[i, 1]];
        }

        // Convertir a 1 si gano, -1 si perdio o 0 si empato
        playerCount = playerCount > 0 ? 1 : (playerCount < 0 ? -1 : 0);

        // No se consideraran los empates para los resultados globales de las peleas
        if (singleModeSession && playerCount != 0) singleModeSession.AddResultsSequence(playerCount);

        switch (playerCount)
        {
            case 1:
                return InGameStates.Win;
            case -1:
                return InGameStates.Lose;
            case 0:
            default:
                return InGameStates.Draw;
        }

    }

    /// <summary>
    /// Apilar los ataques de los jugadores. Si el valor de attack viene como -1, significa que el ataque del jugador fue al azar.
    /// </summary>
    /// <param name="playerSelectedAttack"></param>
    private void StackAttacks(Attacks playerSelectedAttack, bool player2 = false)
    {
        // Guardar los ataques
        turnsAttacks[player2 ? playerTwoTurn : playerOneTurn, player2 ? 1 : 0] = playerSelectedAttack;
    }

    /// <summary>
    /// Elimina los elementos del stack de ataque
    /// </summary>
    private void ResetStackAttacks()
    {
        playerOneTurn = 0;
        playerTwoTurn = 0;
        turnsAttacks = new Attacks[totalTurns, 2];
        if (playerOneStack) playerOneStack.Reset();
        if (playerTwoStack) playerTwoStack.Reset();
        superJankenUsed = false;
    }

    /// <summary>
    /// Activar indice X en el stack Y
    /// </summary>
    /// <param name="index"></param>
    /// <param name="stack"></param>
    private void ActivateStackAttack(int index, AttackStack stack)
    {
        if (stack) stack.ActiveAttack(index);
    }

    /// <summary>
    /// Obtener la cantidad de turnos actuales
    /// </summary>
    /// <returns></returns>
    public int CountTurns()
    {
        return totalTurns;
    }

    /// <summary>
    /// Mostrar/Ocultar las opciones de ataque
    /// </summary>
    /// <param name="show"></param>
    public void ToggleAttackUI(bool show)
    {
        //attackOptions.gameObject.SetActive(show);
    }

    /// <summary>
    /// Actualiza todos los elementos ligados a un translate
    /// </summary>
    protected override void Localize() {
        SetPlayersName();
    }

    /// <summary>
    /// Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    public void UpdateCurrentFont() { }

    /// <summary>
    /// Mostrar la obtencion de un superpoder en uno de los jugadores
    /// </summary>
    /// <param name="superPower"></param>
    public void ShowGetSuperPower(SuperPowers superPower, bool player2 = false)
    {
        MasterSFXPlayer._player.WinSuperPower();
        StartCoroutine(player2 ? playerTwoController.ShowGetSuperPower(superPower) : playerOneController.ShowGetSuperPower(superPower));
        UpdateAttackUI();
    }

    /// <summary>
    /// Corutina para la ejecucion del los superpoderes
    /// </summary>
    /// <param name="turnsAttacks"></param>
    /// <returns></returns>
    private IEnumerator CoroutineJanKenUp(Attacks[,] turnsAttacks)
    {
        // Detener la alerta en el Canvas
        alertCanvas.Stop();

        // Detener el tiempo
        timeDisplayer.StopTimer();

        currentCountdownSteps = -1;
        isSuperAttackExecuting = true;

        // Obtencion de los elemento UI del gameplay
        StartGetUIGameplay();
        yield return null;

        // Ocultar toda la UI
        HideUI();

        // Ocultar UI innecesaria
        ToggleAttackUI(false);
        //TogglePlayerFeintController(false);
        referee.HideAnnouncement();

        // Disminuir el volumen de la musica
        MasterAudioPlayer._player.FadeAudioSourceSuper(false);

        // Revisar los ataques de los jugadores en busca de un superpoder
        bool playerOneSuper = false;
        bool playerTwoSuper = false;
        for(int i = 0; i < totalTurns; i++)
        {
            playerOneSuper = playerOneSuper || turnsAttacks[i, 0] == Attacks.JanKenUp;
            playerTwoSuper = playerTwoSuper || turnsAttacks[i, 1] == Attacks.JanKenUp;
        }

        // Obtener si para el tipo de juego actual se debe eliminar el arbitro del campo de juego
        bool byebyeReferee = !singleModeSession.AtLeastXPlayersAlive();
        if(byebyeReferee) referee.SetImmuneToDisappear(false);

        // Guardar los combos
        if(playerOneSuper && !playerTwoSuper)
        {
            playerOneCombo++;
            playerTwoCombo = 0;
        }
        else if(!playerOneSuper && playerTwoSuper)
        {
            playerOneCombo = 0;
            playerTwoCombo++;
        }
        else
        {
            playerOneCombo = 0;
            playerTwoCombo = 0;
        }

        singleModeSession.AddPlayersCombos(playerOneCombo, playerOneIndex);
        singleModeSession.AddPlayersCombos(playerTwoCombo, playerTwoIndex);

        // Indicar la ejecucion del super al InGameSequence
        yield return StartCoroutine(inGameSequence.ExecuteSuperJanKenUP(playerOneController, playerTwoController, playerOneSuper, playerTwoSuper, byebyeReferee, false));

        // Hacer aparecer al jugador que fue eliminado (Win player 2 Lose player 1)
        if (!playerOneSuper || !playerTwoSuper) FinishSpawnPlayer(SpawnPlayer(!playerTwoSuper), !playerTwoSuper);

        // Mostrar toda la UI
        ShowUI();

        // Aumentar el volumen de la musica
        MasterAudioPlayer._player.FadeAudioSourceSuper(true);

        isSuperAttackExecuting = false;
    }

    /// <summary>
    /// Reproducir sonido de super golpe
    /// </summary>
    /// <param name="crowd"></param>
    public void PlayStrongHit(bool crowd)
    {
        if (crowd) MasterSFXPlayer._player.PlayOneShot(destinyLevelSFX);
        audioSource.PlayOneShot(strongPunchSFX);
    }

    /// <summary>
    /// Finalizacion del tiempo de juego
    /// </summary>
    public override void TimeOut()
    {
        // Indicar que ya no puede recibir ataques
        timeOut = true;

        // Quitar las opciones de pantalla
        ToggleAttackUI(false);
    }

    /// <summary>
    /// Ir a los resultados de la partida
    /// </summary>
    /// <returns></returns>
    private IEnumerator GoToResults()
    {
        // Esconder fintas jugadores
        HideFeint();

        // Quitar mostrar resultados de ataque
        singleModeSession.SetIsShowingResults(false);

        // Detener la alerta en el Canvas
        alertCanvas.Stop();

        // Tween para la aumentar y disminuir el tiempo
        System.Action<ITween<float>> updateTimeAndPitch = (t) =>
        {
            MasterAudioPlayer._player.ChangePitchAudio(t.CurrentValue);
        };

        // Ejecutar la desaceleracion de la musica
        gameObject.Tween(string.Format("Deceleration{0}", GetInstanceID()), MasterAudioPlayer._player.GetPitchAudio(), minMusicPitch, changeMusicPitchTime, TweenScaleFunctions.QuadraticEaseOut, updateTimeAndPitch);

        // Determinar ganar en base a las vidas de los jugadores
        int playerOneLives = singleModeSession.GetLives(playerOneIndex);
        int playerTwoLives = singleModeSession.GetLives(playerTwoIndex);
        bool playerOneStillAlive = playerOneLives > 0;
        bool playerTwoStillAlive = playerTwoLives > 0;
        bool playersHaveSameLives = playerOneLives == playerTwoLives;

        // Si al menos 2 jugadores estan con vida, continuar con duelos
        if (singleModeSession.AtLeastXPlayersAlive())
        {
            referee.ChangeState(InGameStates.Draw);
            // Espera final
            if (inGameSequence) yield return StartCoroutine(inGameSequence.FinalWait());
            playerOneController.ChangeState(InGameStates.Stand);
            playerTwoController.ChangeState(InGameStates.Stand);

            // Indicar nueva ronda
            ++currentRound;
            yield return StartCoroutine(ShowRound());

            // Siguiente Ronda
            timeOut = false;
            steps = Math.Clamp(steps / 2, minSteps, steps);
            timePerDuel = Math.Clamp(timePerDuel / 2, minTimePerDuel, timePerDuel);
            timeDisplayer.SetRemainingTime(timePerDuel);

            // Iniciar partida
            StartCoroutine(ReStartMatch());
        }
        else
        {
            isGoingToResult = true;

            if ((playerOneStillAlive && !playerTwoStillAlive) || (playerOneStillAlive && playerOneLives > playerTwoLives))
            {
                // Jugador 1 gano el duelo
                referee.ChangeState(InGameStates.Win);
                singleModeSession.AddPlayerToRanking(playerOneIndex);
                // Espera final
                if (inGameSequence) yield return StartCoroutine(inGameSequence.FinalWait());
                playerOneController.ChangeState(InGameStates.Win);
                if (playerTwoController) playerTwoController.ChangeState(InGameStates.Lose);
            }
            else
            {
                // Jugador 2 gano el duelo
                referee.ChangeState(InGameStates.Lose);
                singleModeSession.AddPlayerToRanking(playerTwoIndex);
                // Espera final
                if (inGameSequence) yield return StartCoroutine(inGameSequence.FinalWait());
                if (playerOneController) playerOneController.ChangeState(InGameStates.Lose);
                playerTwoController.ChangeState(InGameStates.Win);
            }
            yield return new WaitForSeconds(timeFinalWait);

            if (simulation) GoToMainScreen();
            else
            {
                isGoingToResultScreen = true;
                SceneLoaderManager.Instance.DuelModeResults();
            }
        }

    }

    /// <summary>
    /// Ir a la pantalla principal
    /// </summary>
    private void GoToMainScreen()
    {
        if (isGoingToMainScreen) return;
        isGoingToMainScreen = true;

        // Detener la alerta en el Canvas
        alertCanvas.Stop();

        // Cualquier accion vuelve al main menu
        simulation = false;
        simulationCanvas.SetActive(false);
        GameController.SetSimulation(false);
        SceneLoaderManager.Instance.MainScreen();
    }

    /// <summary>
    /// Muestra en arbitro la ronda actual
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowRound()
    {
        var roundData = new[] { new { round = currentRound } };
        yield return timeDisplayer.GetTimeToToggle();
        referee.ChangeState(InGameStates.Stand);
        timeDisplayer.Toggle(false);
        yield return referee.Announcement(JankenUp.Localization.tables.InGame.Keys.round, roundData);
        timeDisplayer.Toggle(true);
        yield return timeDisplayer.GetTimeToToggle();
    }

    // Al pausar el juego, deberia mostrarse la pantalla de pausa
    void OnApplicationPause(bool pauseStatus) { }

    /// <summary>
    /// Comprobacion de si uno de los actores actuales necesita la presencia de un crowd NPC en la arena
    /// </summary>
    private void ActorsNeedSupportNpc()
    {
        JankenUp.NPC.Identifiers playerOneSupportNpc = playerOneController.GetSupportNPCIdentifier();
        JankenUp.NPC.Identifiers playerTwoSupportNpc = playerTwoController.GetSupportNPCIdentifier();

        if (playerOneSupportNpc != JankenUp.NPC.Identifiers.None) arena.CheckForSupportNPC(playerOneSupportNpc, playerOneController);
        if (playerTwoSupportNpc != JankenUp.NPC.Identifiers.None) arena.CheckForSupportNPC(playerTwoSupportNpc, playerTwoController);
    }

    /// <summary>
    /// Actualizacion del UI de ataque
    /// </summary>
    public override void UpdateAttackUI()
    {
        base.UpdateAttackUI();
    }

    /// <summary>
    /// Consulta la posiblidad de abrir menu
    /// </summary>
    /// <returns></returns>
    public override bool CanToggleMenu()
    {
        // Si se esta ejecutando un super ataque no permitir la pausa o ya se selecciono ataque
        return !isSuperAttackExecuting && !moveSelected && !isGoingToResult && !attackSequenceInCourse && !TransitionDoors._this.IsInTransition();
    }

    /// <summary>
    /// Recepcion del estado del menu
    /// </summary>
    /// <param name="state"></param>
    protected override void OnToggleMenu(OptionsMenu.OptionsMenuStates state)
    {
        bool pauseState = state == OptionsMenu.OptionsMenuStates.Open;

        // Cambiar el ataque si se volvio de la pausa
        if (!pauseState) StartCoroutine(CountdownSequence());
        else
        {
            referee.HideAnnouncement();
            timeDisplayer.StopTimer();
            StopAllCoroutines();
        }
    }

    #region Spectators-actions
    /// <summary>
    /// Emision de un apoyo en forma de challas para el jugador seleccionado
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="player2"></param>
    private void EmitHorn(int playerIndex = 0, bool player2 = false) {
        if (playerIndex == playerOneIndex || playerIndex == playerTwoIndex || spectatorsHornActive.Contains(playerIndex) || isGoingToResult) return;
        singleModeSession.StartCoroutine(AddSpectatorHorn(playerIndex));
        Instantiate(player2? confettiRight : confettiLeft).GetComponent<GeneralParticle>().Play();
    }

    /// <summary>
    /// Proceso para limitar el uso indiscriminado de horns por parte de los espectadores
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    private IEnumerator AddSpectatorHorn(int playerIndex = 0)
    {
        spectatorsHornActive.Add(playerIndex);
        yield return new WaitForSeconds(hornCooldown);
        spectatorsHornActive.Remove(playerIndex);
    }

    /// <summary>
    /// Lanzamiento de una coronta a uno de los jugadores
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="player2"></param>
    private void ThrowCorbcob(int playerIndex = 0, bool player2 = false) {
        if (playerIndex == playerOneIndex || playerIndex == playerTwoIndex || spectatorsCorncobActive.Contains(playerIndex) || isGoingToResult) return;
        singleModeSession.StartCoroutine(AddSpectatorCorncob(playerIndex));

        CharacterInGameController character = player2 ? playerTwoController : playerOneController;
        if (character == null) return;

        SpecialObjectProjectile corncob = Instantiate(projectilePrefabs[Random.Range(0, projectilePrefabs.Count)], arena.transform).GetComponent<SpecialObjectProjectile>();
        if (corncob) corncob.SetTarget(character.transform);
    }

    /// <summary>
    /// Proceso para limitar el uso indiscriminado de corontas por parte de los espectadores
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    private IEnumerator AddSpectatorCorncob(int playerIndex = 0)
    {
        spectatorsCorncobActive.Add(playerIndex);
        yield return new WaitForSeconds(corncobCooldown);
        spectatorsCorncobActive.Remove(playerIndex);
    }
    #endregion

    /// <summary>
    /// Despliegue de finta en el jugador seleccionado
    /// </summary>
    /// <param name="attack"></param>
    /// <param name="playerIndex"></param>
    private void ShowFeint(int attack = 0, int playerIndex = 0)
    {
        // Solo si el jugador es parte del duelo y no se estan mostrando resultados o transicionando
        if (configuringNextLevel || timeOut || !TransitionDoors._this.IsTotallyOpen() || singleModeSession.GetIsShowingResults() ||
            (playerIndex != playerOneIndex && playerIndex != playerTwoIndex)) return;

        CharacterInGameController target = playerIndex == playerOneIndex ? playerOneController : playerTwoController;
        target.ShowFeint(attack);
    }

    /// <summary>
    /// Desuscribirse de los metodos de ataque
    /// </summary>
    protected new void OnDestroy()
    {
        base.OnDestroy();
        if(!isGoingToResultScreen) GameController.SetGameplayActive(false);
        // Quitar localizacion
        LanguageController.onLanguageChangeDelegate -= Localize;
        // Quitar joystick a vibrar
        JoystickSupport.Instance.SetGamepadToVibrate(new List<int>());
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

        if (simulation)
        {
            if(!isGoingToResult) GoToMainScreen();
        }
        else
        {
            bool isPlaying = playerIndex == playerOneIndex || playerIndex == playerTwoIndex;

            switch (action)
            {
                case JoystickAction.Escape:
                    if (optionsMenu != null && optionsMenu.isActiveAndEnabled) optionsMenu.Toggle();
                    break;
                case JoystickAction.X:
                    // Por defecto: Piedra
                    if(isPlaying) SelectOption(0, playerIndex);
                    break;
                case JoystickAction.A:
                    // Por defecto: Papel
                    if(isPlaying) SelectOption(1, playerIndex);
                    break;
                case JoystickAction.B:
                    // Por defecto: Tijera
                    if(isPlaying) SelectOption(2, playerIndex);
                    break;
                case JoystickAction.ZR:
                case JoystickAction.ZL:
                case JoystickAction.LKeyboard:
                    // Super JanKenUP
                    if (isPlaying) SelectOption((int)Attacks.JanKenUp, playerIndex);
                    break;
                case JoystickAction.R:
                case JoystickAction.K:
                    // MagicWand
                    if (isPlaying) SelectOption((int)Attacks.MagicWand, playerIndex);
                    break;
                case JoystickAction.XHold:
                    if (isPlaying) ShowFeint(0, playerIndex);
                    break;
                case JoystickAction.AHold:
                    if (isPlaying) ShowFeint(1, playerIndex);
                    break;
                case JoystickAction.BHold:
                    if (isPlaying) ShowFeint(2, playerIndex);
                    break;
                case JoystickAction.Up:
                case JoystickAction.Down:
                    // Horn
                    if (!isPlaying) EmitHorn(playerIndex, action == JoystickAction.Down);
                    break;
                case JoystickAction.Left:
                case JoystickAction.Right:
                    // Tirar objetos
                    if (!isPlaying) ThrowCorbcob(playerIndex, action == JoystickAction.Right);
                    break;
            }
        }

        return canContinue;
    }
    #endregion
}