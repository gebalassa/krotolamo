﻿using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Analytics;

public class SurvivalModeController : GamePlayScene
{
    [Header("UI")]
    [SerializeField] LevelDisplayer levelDisplayer;
    [SerializeField] AttackOptions attackOptions;
    [SerializeField] LivesDisplayer livesDisplayer;
    [SerializeField] CoinsCurrencyDisplayer coinsDisplayer;
    [SerializeField] AttackStack leftStack;
    [SerializeField] AttackStack rightStack;
    [SerializeField] ComboDisplayer comboDisplayer;
    [SerializeField] TimeDisplayer timeDisplayer;
    [SerializeField] GameObject pauseUI;
    [SerializeField] Text harderUIText;
    [SerializeField] GameObject pauseButton;
    [SerializeField] AlertCanvas alertCanvas;

    [Header("Labels")]
    [SerializeField] Text resumeLabel;
    [SerializeField] Text backLabel;

    [Header("Referee")]
    [SerializeField] Referee referee;
    int steps = 3;
    int currentCountdownSteps = -1;

    [Header("Actors")]
    [SerializeField] GameObject player;
    [SerializeField] GameObject cpu;

    // Posiciones iniciales de los personajes
    Vector3 playerInitialPosition;
    Vector3 cpuInitialPosition;

    [Header("SFX")]
    [SerializeField] AudioClip attackSelection;
    [SerializeField] AudioClip illuminatiSFX;
    [SerializeField] AudioClip spendCoinsSuccess;
    [SerializeField] AudioClip spendCoinsFail;
    [SerializeField] AudioClip destinyLevelSFX;
    [SerializeField] AudioClip strongPunchSFX;
    [SerializeField] AudioClip harderSFX;
    [SerializeField] AudioClip easierSFX;

    [Header("Tutorial")]
    [SerializeField] GameObject tutorial;
    Transform tutLevel;
    bool isTuto = false;
    bool playerNoPlayTuto = false;

    [Header("Times")]
    [SerializeField] [Range(0,1f)]float doorsClosingFor = 0.2f;
    [SerializeField] [Range(0, 1f)] float difficultyChangeTime = 1f;
    [SerializeField] [Range(0, 1f)] float superJanKenUpTime = 1f;

    [Header("SuperPower")]
    [SerializeField] GameObject janKenUpPrefab;

    // Componentes recurrentes
    CharacterInGameController playerController;
    CharacterInGameController cpuController;
    AudioSource audioSource;
    ArenaController arena;
    InGameSequence inGameSequence;

    [Header("Illuminati")]
    [SerializeField] int illuminatiCost = 6;

    [Header("Survival")]
    [SerializeField] Vector3 spawnPosition;
    [SerializeField] int spawnGravity = 12;
    [SerializeField] int spawnPostGravity = 1;
    [SerializeField] PhysicsMaterial2D spawnMaterial;
    [SerializeField] PhysicsMaterial2D spawnPostMaterial;
    [SerializeField] int timeAddOnWin = 3;
    [SerializeField] int timeSubtractOnDraw = 1;
    [SerializeField] int timeFinalWait = 1;
    [SerializeField] float timeToChangeGravity = .5f;
    [SerializeField] int survivalLives = 1;
    [SerializeField] float minMusicPitch = 1f; 
    [SerializeField] float maxMusicPitch = 1.1f;
    [SerializeField] float musicPitchStartAt = 10;
    [SerializeField] float changeMusicPitchTime = .5f;
    [SerializeField] float showAlertAtSeconds = 10;
    float deltaMusicPitch = 0;

    [Header("Others")]
    [SerializeField] SurvivalModeDifficulty difficulty;

    // Dificultad
    JankenUp.SinglePlayer.LevelType.Type levelType;
    float difficultySpeed = -1;
    float lastDifficultySpeed = -1;
    int difficultyTurns = 1;
    int currentTurn = 0;
    bool illuminati = false;
    bool showCPUAttack = true;
    Attacks[,] turnsAttacks;

    // Sesión singlePlayer
    SingleModeSession singleModeSession;

    // Ataques de los personajes
    Attacks playerAttack;
    Attacks cpuAttack;

    // Matriz de resultados
    int[,] results = new int[4,4] {
        { 0, -1, 1, -1 }, { 1, 0, -1, -1 }, { -1, 1, 0, -1 }, { 1, 1, 1, 0 }
    };

    // Indicación de primera configuración de nivel
    bool firstConfiguration = true;

    // Impedir entrar al opciones si ya se escogio ataque y aun no se define ganador 
    bool moveSelected = false;

    // Arbitro ya indico que esta lista para empezar la partida (Util para los triple shoot)
    bool refereeFirstShoutReady = false;

    // Configuracion del usuario
    int playerPosition = 0;

    // Corutina para cambio de ataque
    Coroutine changeAttackCoroutine;

    // Variables para modo survival
    bool timeOut = false;
    bool arenaConfigReady = false;

    // Para pausas
    bool pauseState = false;
    float prePauseTimeScale = 1;
    bool attackSequenceInCourse = false;
    bool preparedForPause = false;
    bool isGoingToResult = false;

    // Cargaremos de inmediato los tutoriales que ya ha superado el usuario y mantendremos el listado para ir actualizandolo
    List<int> tutorialsCompleted;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        // Guardar modo de juego como el ultimo
        GameController.SetLastGameMode(GameMode.Frenzy);
        GameController.SetGameplayActive(true);
        CharacterPool.Instance.SetPlayersAllowedToDuel(new List<int> {0});

        // Eventos de localizacion
        LanguageController.onLanguageChangeDelegate += Localize;

        // Impedir que la pantalla se vaya a negro
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Obtener tutoriales
        tutorialsCompleted = GameController.LoadTutorials().tutorials;

        // Componentes
        audioSource = GetComponent<AudioSource>();

        // Obtener la sessión de juego asociada.
        singleModeSession = FindObjectOfType<SingleModeSession>();
        singleModeSession.SetCurrentLives(survivalLives);
        singleModeSession.SetMaxLives(survivalLives);
        singleModeSession.AlwaysLoseLives(true);
        singleModeSession.SetIsShowingResults(false);
        singleModeSession.ActivateSurvivalMode();

        // Obtener el componente de dificultad
        difficulty = GetComponent<SurvivalModeDifficulty>();

        // Obtener el secuenciados
        inGameSequence = FindObjectOfType<InGameSequence>();

        // Configurar las vidas del jugador
        livesDisplayer.SetTotalLives(singleModeSession.GetLives(), singleModeSession.GetLives());
        livesDisplayer.SetPlayerName(singleModeSession.GetPlayer().GetComponent<CharacterConfiguration>().GetName());
        livesDisplayer.SetAvatar(singleModeSession.GetPlayer().GetComponent<CharacterConfiguration>().GetAvatar());

        // Configurar al jugador
        ConfigPlayer();

        // Configurar el nivel
        ConfigLevel();

        // Actualizar los textos
        UpdateCurrentFont();

        // Calcular diferencia de niveles para le pitch de la musica
        deltaMusicPitch = (maxMusicPitch - minMusicPitch) / musicPitchStartAt;

        // Configurar los tiempos del timer si es que ya se habian seteado en la sesion
        if (singleModeSession.GetTimeRemaining() != -1) timeDisplayer.SetRemainingTime(singleModeSession.GetTimeRemaining());
        if (singleModeSession.GetTimeElapsed() != -1) timeDisplayer.SetElapsedTime(singleModeSession.GetTimeElapsed());

        // Referee no puede desaparecer
        referee.SetImmuneToDisappear(true);

        // Configurar botones
        AttackOptions.onAttackSelectDelegate += SelectOption;
        AttackOptions.onMagicWandDelegate += SelectOption;
        AttackOptions.onSuperDelegate += PerformJanKenUp;

        // Iniciar partida
        StartCoroutine(StartMatch());

    }

    // Revision si se debe tocar la musica especial asignada al personaje. De lo contrario, ejecutar codigo normal
    protected override void PlayMusic()
    {
        if (!singleModeSession) singleModeSession = FindObjectOfType<SingleModeSession>();
        CharacterConfiguration playerConf = singleModeSession.GetPlayer().GetComponent<CharacterConfiguration>();
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

        // Botón retroceder en androide
        if (Application.platform == RuntimePlatform.Android)
        {
            // Check if Back was pressed this frame
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!pauseState && TransitionDoors._this.IsTotallyOpen()) optionsMenu.Toggle();
            }
        }

        if (!attackOptions.IsSuperAttackExecuting() && !timeOut && singleModeSession.GetLives() > 0) CheckForTime();

        // Agregar el tiempo trasncurrido a la sesion
        if (timeDisplayer.IsTimerRunning())
        {
            singleModeSession.SetTimeElapsed(timeDisplayer.GetTimeElapsed());
            singleModeSession.SetTimeRemaining(timeDisplayer.GetTimeRemaining());
        }
    }

    private void CheckForTime()
    {
        float currentTime = timeDisplayer.GetTimeRemaining();
        if (currentTime <= musicPitchStartAt && currentTime > 0)
            MasterAudioPlayer._player.ChangePitchAudio(Mathf.Clamp( (musicPitchStartAt - currentTime) * deltaMusicPitch + minMusicPitch, minMusicPitch, maxMusicPitch));
        else MasterAudioPlayer._player.ChangePitchAudio(minMusicPitch);
    }

    // Configurar ubicacion del personaje
    private void ConfigPlayer()
    {
        // Conseguir la preferencia del usuario. 0 izquierda 1 seria derecha
        playerPosition = PlayerPrefs.GetInt(PlayersPreferencesController.CONTROLPOSITIONKEY, PlayersPreferencesController.CONTROLPOSITIONKEYDEFAULT);
        if (playerPosition == 1)
        {
            GameObject temp = player;
            player = cpu;
            cpu = temp;

            // Cambiar el vector de aparicion de rival
            spawnPosition.x = -spawnPosition.x;

        }
    }

    // Reconfigurar la escena de juego
    private void ConfigLevel()
    {
        // Actualizar el UI
        UpdateUI();

        // Configurar nivel de dificultad
        ConfigDifficulty();

        // Si no es la primera configuracion
        if (!firstConfiguration) ConfigNewCPU();

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

        // Resetear al arbitro
        referee.ChangeState(InGameStates.Stand);

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

    }

    // Cerrar las puertas y reconfigurar el juego
    private IEnumerator ReStartMatch()
    {
        TransitionDoors doors = FindObjectOfType<TransitionDoors>();

        // Configurar nivel
        ConfigLevel();

        // Resetear la posibilidad de usar super ataques
        attackOptions.Reset();

        // Si la dificultad (Entendida en terminos de cambio de velocidad) es más rápida o lenta que la anterior, indicar. 
        if(lastDifficultySpeed != difficultySpeed)
        {
            // Si hay menos tiempo, más dificil
            if(difficultySpeed <= lastDifficultySpeed || lastDifficultySpeed == -1)
            {
                StartCoroutine(ShowDifficultyChange());
            }
        }

        yield return StartCoroutine(doors.Open());

        // Iniciar partida
        StartCoroutine(StartMatch());
    }

    // Mostrar cambio en la dificultad
    private IEnumerator ShowDifficultyChange()
    {
        levelDisplayer.Show();
        yield return new WaitForSeconds(difficultyChangeTime);
        levelDisplayer.Hide();
    }

    // Inicio del juego
    private IEnumerator StartMatch() {

        // Revisar si es nivel tutorial y sii el jugador no lo ha visto
        isTuto = Tutorial.IsSurvivalModeTutorial(singleModeSession.GetLevel());
        playerNoPlayTuto = !tutorialsCompleted.Contains(singleModeSession.GetLevel());

        // Si no esta aún abiertas las puertas, esperar
        while (!isReady) yield return null;

        // Mostrar las opciones de ataque
        ToggleAttackUI(true);

        // Revisar si el referee debe anunciar algo segun el tipo de juego
        if (!refereeFirstShoutReady)
        {
            switch (levelType)
            {
                case JankenUp.SinglePlayer.LevelType.Type.Triple:
                    timeDisplayer.Toggle(false);
                    yield return referee.Announcement(JankenUp.SinglePlayer.LevelType.Triple.announcement,2);
                    timeDisplayer.Toggle(true);
                    yield return timeDisplayer.GetTimeToToggle();
                    break;
                case JankenUp.SinglePlayer.LevelType.Type.Destiny:
                    timeDisplayer.Toggle(false);
                    yield return referee.Announcement(JankenUp.SinglePlayer.LevelType.Destiny.announcement,2);
                    timeDisplayer.Toggle(true);
                    yield return timeDisplayer.GetTimeToToggle();
                    break;
            }
            refereeFirstShoutReady = true;
        }

        // Si se encuentra en un nivel con tutorial, mostrar antes del countdown
        if (singleModeSession && isTuto && playerNoPlayTuto)
        {
            // Activar el ataque del enemigo mientras no sea destiny
            cpuController.ShowPreAttack(levelType != JankenUp.SinglePlayer.LevelType.Type.Destiny);

            // El ataque del enemigo en los tutoriales siempre es piedra
            cpuAttack = Attacks.Rock;

            // Si es el nivel tutorial de illuminati, no mostrar las opciones de ataque
            if (difficulty.IlluminatiTutorialLevel(singleModeSession.GetLevel()) && IsIlluminati()) ToggleAttackUI(false);

            // Mostrar el tutorial
            tutorial.gameObject.SetActive(true);

            // Activar el nivel
            tutLevel = tutorial.transform.Find("Level_" + singleModeSession.GetLevel());
            CheckTutorialAttackCoroutine();

            // Detener el tiempo
            timeDisplayer.StopTimer();

        }
        else
        {
            // Iniciar countdown
            StartCoroutine(CountdownSequence());
        }

        // Si se necesita pausar el juego
        if (preparedForPause)
        {
            preparedForPause = false;
            optionsMenu.Toggle(true);
        }

    }

    /// <summary>
    /// Revisar si el tutorial necesita que el ataque de la CPU cambie
    /// </summary>
    private void CheckTutorialAttackCoroutine(bool show = true)
    {
        tutLevel = tutorial.transform.Find("Level_" + singleModeSession.GetLevel());
        if (tutLevel)
        {
            Tutorial tutorialScript = tutLevel.GetComponent<Tutorial>();
            tutLevel.gameObject.SetActive(true);
            if (show) tutorialScript.Show();
            if (tutorialScript.SurvivalLevelsWithChangeAttack(singleModeSession.GetLevel())) changeAttackCoroutine = StartCoroutine(ChangeCPUAttack());
        }
    }

    // Actualización de la UI de vidas, monedas
    private void UpdateUI()
    {
        if (singleModeSession)
        {
            livesDisplayer.SetCurrentLives(singleModeSession.GetLives());
            if (coinsDisplayer) coinsDisplayer.OnCoinsCurrencyUpdate();

            // Revisar por vidas
            singleModeSession.CheckForLives();

            // Revisar si se puede o no mostrar los superataques (Esto es que se tenga el nivel tutorial superado)
            bool playerKnowsSuperPowers = tutorialsCompleted.Contains(Tutorial.GetSuperPowerLevel());
            bool isSuperPowerTutorialLevel = Tutorial.GetSuperPowerLevel() == singleModeSession.GetLevel();
            if (playerKnowsSuperPowers || isSuperPowerTutorialLevel)
            {
                attackOptions.ReadyToUseSuperPowers();

                // Si se entro por la condicion de tutorial de superpoder, ocultar ataques
                if (!playerKnowsSuperPowers)
                {
                    attackOptions.ToggleNormalAttacks(false);

                    // Si el jugador no cuenta con una MagicWand, agregar
                    if(singleModeSession.GetMagicWand() == 0)
                    {
                        singleModeSession.AddSuperPower(SuperPowers.MagicWand, 1);
                    }

                }
                else attackOptions.ToggleNormalAttacks(true);

            }

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
                playerInitialPosition = player.transform.position;
                cpuInitialPosition = cpu.transform.position;

                GameObject newPlayer = Instantiate(
                    singleModeSession.GetPlayer(),
                    player.transform.position,
                    Quaternion.identity
                );

                // Destruir el jugador actual y asignar nueva referencia
                newPlayer.transform.parent = player.transform.parent;
                Destroy(player);
                player = newPlayer;
                playerController = player.GetComponent<CharacterInGameController>();
                playerController.Flip(playerPosition == 1 ? - 1 : 1);
            }

            // Posicion correcta del jugador
            player.transform.position = playerInitialPosition;

            // Realizar lo mismo con la CPU
            GameObject randomCPU = Instantiate(
                RandomCPU(singleModeSession.GetPlayer()),
                cpu.transform.position,
                Quaternion.identity
                );
            randomCPU.transform.parent = cpu.transform.parent;
            Destroy(cpu);
            cpu = randomCPU;
            cpuController = cpu.GetComponent<CharacterInGameController>();
            cpuController.Flip(playerPosition == 0 ? -1 : 1);

            // Posicion correcta del adversario
            cpu.transform.position = cpuInitialPosition;
        }

        // Añadir control
        playerController.SetControl(true);
        cpuController.SetControl(!showCPUAttack);

        // Si el arbitro esta desbloqueado, utilizar el backup
        referee.ToggleBackup(CharacterPool.Instance.Get("referee").GetComponent<CharacterConfiguration>().IsUnlocked());

        // Volver a la normalidad al jugador en caso de que haya sido mandado a volar (Lo mismo con el referee)
        referee.Reappear();
        playerController.Reappear();

        // Si es un nivel illuminati, indicar a CPU
        if (illuminati) cpuController.Illuminati();

        // Indicar al secuenciador quien es el nuevo objeto de la izquierda
        if (inGameSequence) inGameSequence.SetActors(player,cpu);

        // Revisar los NPC a ocultar
        List<string> npcIdentifiers = new List<string> { playerController.GetIdentifier(), cpuController.GetIdentifier() };
        arena.HideNPCInMatch(npcIdentifiers);
    }

    // Aparicion de un nuevo mob para pelear
    private GameObject SpawnCPU()
    {
        if (timeOut) return null;

        // Remover el colider2d del actual CPU
        Destroy(cpu.GetComponent<Collider2D>());

        // Realizar cambio de CPU
        GameObject randomCPU = Instantiate(
            RandomCPU(singleModeSession.GetPlayer()),
            spawnPosition,
            Quaternion.identity
            );
        randomCPU.transform.parent = cpu.transform.parent;
        CharacterInGameController characterInGamecontroller = randomCPU.GetComponent<CharacterInGameController>();
        characterInGamecontroller.Flip(playerPosition == 0 ? -1 : 1);

        // Posicion correcta del adversario
        randomCPU.transform.position = spawnPosition;

        // Alterar el rigidbody para una caida rapida
        Rigidbody2D cpuRigidbody2D = randomCPU.GetComponent<Rigidbody2D>();
        cpuRigidbody2D.sharedMaterial = spawnMaterial;
        cpuRigidbody2D.gravityScale = spawnGravity;

        // Indicar al controlador que debe hacer el cambio de material al tocar plataforma
        characterInGamecontroller.AlterRigidBodyOnContact(spawnPostMaterial, spawnPostGravity);

        // Revisar los NPC a ocultar
        List<string> npcIdentifiers = new List<string> { characterInGamecontroller.GetIdentifier() };
        arena.HideNPCInMatch(npcIdentifiers);

        return randomCPU;

    }

    // Terminar la aparicion del nuevo CPU, permitiendo que sea controlado por el juego
    private void FinishSpawnCPU(GameObject newCPU)
    {
        if (!newCPU) return;

        // Destruir y asignar la nueva CPU
        Destroy(cpu);
        cpu = newCPU;

        // Añadir control
        cpuController = cpu.GetComponent<CharacterInGameController>();

        // Revisar si el nuevo CPU necesita soporte
        ActorsNeedSupportNpc();

    }

    // Configurar CPU
    private void ConfigNewCPU()
    {
        // Si es un nivel illuminati, indicar a CPU
        if (illuminati) cpuController.Illuminati();

        // Indicar al secuenciador quien es el nuevo objeto de la izquierda
        if (inGameSequence) inGameSequence.SetActors(player, cpu);

        // No mostrar de inmediato el preattack
        cpuController.ShowPreAttack(false);
    }

    // Configuración de la dificultad del nivel.
    private void ConfigDifficulty()
    {
        // Si exise sesión, realizar configuración.
        if (!singleModeSession) singleModeSession = FindObjectOfType<SingleModeSession>();
        if (!difficulty) difficulty = GetComponent<SurvivalModeDifficulty>();

        if (singleModeSession && difficulty)
        {
            int level = singleModeSession.GetLevel();

            difficulty.CalcDifficulty();

            // Obtener la velocidad de cambio actual
            lastDifficultySpeed = difficultySpeed;
            difficultySpeed = difficulty.GetSpeed();

            // Obtener los pasos
            steps = difficulty.GetCountdown();

            // Obtener el tipo de nivel y configurar en base a eso los parametros de turno y ataques
            levelType = difficulty.GetType(level);
            switch (levelType) {
                case JankenUp.SinglePlayer.LevelType.Type.Normal:
                    difficultyTurns = JankenUp.SinglePlayer.LevelType.Normal.turns;
                    showCPUAttack = JankenUp.SinglePlayer.LevelType.Normal.showCPUAttack;
                    break;
                case JankenUp.SinglePlayer.LevelType.Type.Triple:
                    difficultyTurns = JankenUp.SinglePlayer.LevelType.Triple.turns;
                    showCPUAttack = JankenUp.SinglePlayer.LevelType.Triple.showCPUAttack;
                    break;
                case JankenUp.SinglePlayer.LevelType.Type.Destiny:
                    difficultyTurns = JankenUp.SinglePlayer.LevelType.Destiny.turns;
                    showCPUAttack = JankenUp.SinglePlayer.LevelType.Destiny.showCPUAttack;
                    steps = JankenUp.SinglePlayer.LevelType.Destiny.time;
                    break;
            }
            currentCountdownSteps = -1;

            // Generar el mantenedor de ataques
            turnsAttacks = new Attacks[difficultyTurns, 2];

            // Obtener si es un nivel illuminati o no
            illuminati = difficulty.Illuminati(level);

            // Desordenar las opciones de ataque
            if (difficulty.ShuffleAttacks()) {
                attackOptions.Shuffle();
            }
        }

    }

    /// <summary>
    /// Selección de ataque
    /// </summary>
    /// <param name="attack"></param>
    public void SelectOption(int attack)
    {

        if (timeOut || !TransitionDoors._this.IsTotallyOpen() || singleModeSession.GetIsShowingResults()) return;

        // Si no quedan turnos
        if (currentTurn >= difficultyTurns) return;

        // Esconder diplayer de dificultad 
        levelDisplayer.Hide();

        // Revisar que tipo de movimiento es
        Attacks playerPreAttack = IntToAttack(attack);

        // Si el ataque es un MagicWand o MegaPunch, revisar si es posible utilizarlo
        bool isPossible = true;

        if (playerPreAttack == Attacks.MagicWand)
        {
            isPossible = attackOptions.MagicWand();
            if (isPossible) singleModeSession.IncreaseSuperJanKenUPCounter();
        }

        if (!isPossible) return;

        // Indicar que se ha seleccionado ataque
        moveSelected = true;
        //pauseButton.SetActive(false);

        // Reproducir sonido de selección
        audioSource.PlayOneShot(attackSelection);

        // Si esta el anuncio del arbrito, desactivar
        referee.HideAnnouncement();

        // Guardar el ataque del usuario
        playerAttack = playerPreAttack;

        // Indicar el ataque del player y de la CPU en sus stacks
        StackAttacks(attack);
        currentTurn++;

        // ¿Quedan turnos?
        if ( currentTurn >= difficultyTurns)
        {
            // Detener la coroutine de conteo e iniciar la coroutine ataque
            currentCountdownSteps = -1;
            StopAllCoroutines();

            // Mostrar tiempo si esta oculto
            timeDisplayer.Toggle(true);

            // Cancelar el superpoder TimeMaster si esta en ejecucion
            attackOptions.StopTimeMaster();

            // Si existe tutorial, desactivar
            if (tutorial.activeSelf && tutLevel)
            {
                tutLevel.GetComponent<Tutorial>().Hide();
            }

            // Iniciar la siguiente rutina
            StartCoroutine(AttackSequence());
        }
        else
        {
            // Si no existe tutorial, continuar
            if (tutorial.activeSelf)
            {
                // Cambiar ataque de CPU
                cpuAttack = IntToAttack(-1, cpuAttack);
                cpuController.ChangeCurrentAttack(cpuAttack);
            }
            else
            {
                // Detener la coroutine de conteo e iniciar la coroutine ataque
                currentCountdownSteps = -1;
                StopAllCoroutines();

                // Iniciar nuevamente el contador
                StartCoroutine(CountdownSequence());
            }
        }

        // Indicar el ataque selecionado para las estadisticas
        string attackName = "";
        switch (playerAttack)
        {
            case Attacks.Rock:
                attackName = "rock";
                break;
            case Attacks.Paper:
                attackName = "paper";
                break;
            case Attacks.Scissors:
                attackName = "scissors";
                break;
            case Attacks.MagicWand:
                attackName = "magicwand";
                break;
        }

        Analytics.CustomEvent("OfflineAttack", new Dictionary<string, object>
        {
            { "attack", attackName }
        });

    }

    // Resetear el contador
    public void ResetCountdown()
    {
        // Detener la coroutine de conteo e iniciar la coroutine ataque
        StopAllCoroutines();

        // Iniciar nuevamente el contador
        StartCoroutine(CountdownSequence());
    }

    // Secuencia que indica cuanto tiempo queda para seleccionar el ataque
    public IEnumerator CountdownSequence() {

        while (overlayObjects.Count > 0) yield return null;

        // Iniciar el parpadeo
        if (levelType == JankenUp.SinglePlayer.LevelType.Type.Triple
            || levelType == JankenUp.SinglePlayer.LevelType.Type.Destiny
            || timeDisplayer.GetTimeRemaining() <= showAlertAtSeconds) alertCanvas.Blink();

        // Mostrar el UI innecesario
        ToggleAttackUI(true);

        // Ver si mostrar ataque enemigo
        cpuController.SetControl(!showCPUAttack);

        // Iniciar el contador de tiempo
        timeDisplayer.StartTimer();

        // Si la velocidad es distinta a -1 y no es una jugada tipo destino, iniciar la corutina de cambio de ataque.
        if (difficultySpeed != -1 && levelType != JankenUp.SinglePlayer.LevelType.Type.Destiny)
        {
            if(changeAttackCoroutine != null) StopCoroutine(changeAttackCoroutine);
            changeAttackCoroutine = StartCoroutine(ChangeCPUAttack());
        }
        else
        {
            cpuAttack = IntToAttack(-1);
            cpuController.ChangeCurrentAttack(cpuAttack);
        }

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
        if (!timeOut) SelectOption(-1);
        else StartCoroutine(GoToResults());

    }

    // Convertir un int a un ataque enumerado
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
        }

        return transformed;

    }

    // Convertir un int a un ataque enumerado además de verificar que no sea igual al pivote
    private Attacks IntToAttack(int attack, Attacks pivot)
    {
        Attacks transformed;
        do
        {
            transformed = IntToAttack(attack);
        } while (transformed == pivot);

        return transformed;
    }

    // Secuencia donde se muestra los ataques y se indica el ganador
    public IEnumerator AttackSequence() {

        // Detener la alerta en el Canvas
        alertCanvas.Stop();

        // Indicar que se esta viendo los ataques
        attackSequenceInCourse = true;
        singleModeSession.SetIsShowingResults(true);

        // Detener el contador de tiempo si queda la nada de tiempo
        float timeRemaining = timeDisplayer.GetTimeRemaining();
        if (0 < timeRemaining && timeRemaining <= 3) timeDisplayer.StopTimer();

        // Ver quién ganó
        InGameStates result = CalcWholeResult();

        // Quitar el UI innecesario
        ToggleAttackUI(false);

        // Quitar el dialogo del referee
        referee.ShowDialog(false);

        // Si es un tutorial, saber si se supero
        int currentLevel = singleModeSession.GetLevel();
        bool tutorialComplete = isTuto && tutorialsCompleted.Contains(currentLevel);

        // Mantener al tanto si usuario uso o no un superpoder
        bool useSuperPower = false;

        // Si fue un empate
        if (singleModeSession)
        {
            if (result.Equals(InGameStates.Draw))
            {
                // Indicar a la sessión del resultado
                singleModeSession.Draw();
            }
            else
            {

                // Indicar a la sessión del resultado
                if (result.Equals(InGameStates.Win))
                {
                    singleModeSession.Win();
                    if (IsShowingTuto())
                    {
                        tutorialComplete = true;
                        tutorialsCompleted.Add(currentLevel);

                        // El tutorial de nivel 0 para Survival es el mismo que para Classic nivel 6 (Aca no existe nivel 6, asi que forzamos que este completo altiro)
                        if (currentLevel == 0) tutorialsCompleted.Add(6);

                        GameController.SaveTutorial(tutorialsCompleted);

                    }
                }
                else
                {
                    // Solo si no es un tutorial y si lo es, que no haya sido superado
                    if (!IsShowingTuto())
                    {
                        singleModeSession.Lose();
                    }

                }

            }
        }

        // Por cada turno, mostrar el ataque correspondiente
        for ( int i = 0; i < difficultyTurns; i++)
        {
            // Obtener el resultado parcial
            InGameStates parcialResult = CalcResult(turnsAttacks[i, 0], turnsAttacks[i, 1]);

            // Determinar uso de superpoder
            useSuperPower = useSuperPower || (turnsAttacks[i, 0] == Attacks.MagicWand);

            // Indicar a los actores su ataque
            playerController.ChangeCurrentAttack(turnsAttacks[i,0]);
            cpuController.ChangeCurrentAttack(turnsAttacks[i,1]);

            // Si es un ataque tipo destino o es el ultimo tiro de una jugada triple, se debe eliminar a uno de los participantes
            bool strong = levelType == JankenUp.SinglePlayer.LevelType.Type.Destiny
                || (levelType == JankenUp.SinglePlayer.LevelType.Type.Triple && i == difficultyTurns - 1 && result != InGameStates.Draw && !IsShowingTuto())
                || (levelType == JankenUp.SinglePlayer.LevelType.Type.Normal && result != InGameStates.Draw && !IsShowingTuto())
                || (IsShowingTuto() && i == difficultyTurns - 1 && result == InGameStates.Win); 

            if (inGameSequence) yield return StartCoroutine(inGameSequence.ParcialGameCourutine(2));

            if (strong) PlayStrongHit(levelType == JankenUp.SinglePlayer.LevelType.Type.Triple || levelType == JankenUp.SinglePlayer.LevelType.Type.Destiny);

            // Mostrar el ataque del rival
            AttackStack playerStack = playerPosition == 0 ? leftStack : rightStack;
            AttackStack cpuStack = playerPosition == 0 ? rightStack : leftStack;
            if (cpuStack) cpuStack.ChangeAttack(i, turnsAttacks[i, 1]);

            // Calcular el step correspondiente para la animacion de la camara
            int cameraStep = difficultyTurns > 1 ? i + 1 : 0;

            // Ver que resultado enviar: Si es una jugada por turno, solo al final se envia el resultado global
            InGameStates resultToSend = difficultyTurns == 1 || (i == difficultyTurns - 1) ? result : parcialResult;

            // Si es el ultimo turno y se gano, hacer aparecer al nuevo CPU
            GameObject newCpu = null;
            if (result.Equals(InGameStates.Win) && i == difficultyTurns - 1) newCpu = SpawnCPU();

            if (inGameSequence) yield return StartCoroutine(inGameSequence.FinishParcialGameRoutine(strong, resultToSend, cameraStep));

            if (result.Equals(InGameStates.Win) && i == difficultyTurns - 1) FinishSpawnCPU(newCpu);

            // Actualizar UI ligada
            if (singleModeSession)
            {
                comboDisplayer.SetValue(singleModeSession.GetCurrentCombo(), singleModeSession.IsNewComboRecord());
            }

            // Ver resultado de la jugada y activar en player o cpu
            switch (parcialResult)
            {
                case InGameStates.Win:
                    ActivateStackAttack(i, playerStack);
                    break;
                case InGameStates.Lose:
                    ActivateStackAttack(i, cpuStack);
                    break;
            }

        }

        if(result == InGameStates.Lose) timeDisplayer.StopTimer();

        // Si no se ha completado el tutorial
        if (isTuto && !tutorialComplete)
        {
            // Resetear el stack y el turno actual
            ResetStackAttacks();

            if (inGameSequence) yield return StartCoroutine(inGameSequence.ResultCourutine(InGameStates.Draw, InGameStates.Draw));

            // Mostrar tutorial otra vez
            StartCoroutine(StartMatch());
        }
        else
        {

            // Si fue un empate, iniciar nuevamente el conteo
            if (result.Equals(InGameStates.Draw))
            {
                // Quitar tiempo
                timeDisplayer.SubtractTime(timeSubtractOnDraw * (levelType == JankenUp.SinglePlayer.LevelType.Type.Normal ? 1 : 2));

                // Resetear el stack y el turno actual
                ResetStackAttacks();

                // Resetear los pj a su estado stand
                referee.ChangeState(InGameStates.Stand);
                playerController.ChangeState(InGameStates.Stand);
                cpuController.ChangeState(InGameStates.Stand);

                // Iniciar countdown si queda tiempo
                if (singleModeSession.GetLives() <= 0 || timeOut) StartCoroutine(GoToResults());
                else ResetCountdown();

            }
            else
            {
                // Resetear el contador de superataque
                if (!useSuperPower) singleModeSession.ResetSuperJanKenUPCounter();

                // Agregar tiempo si se se gano
                if (result.Equals(InGameStates.Win)) timeDisplayer.AddTime(timeAddOnWin * ( levelType == JankenUp.SinglePlayer.LevelType.Type.Normal? 1 : 3 ));

                if ( singleModeSession.GetLives() <= 0 || timeOut ) StartCoroutine(GoToResults());
                else StartCoroutine(ReStartMatch());

            }
            // Actualización de UI
            UpdateUI();
        }

        // Indicar que no se ha seleccionado ataque
        moveSelected = false;
        attackSequenceInCourse = false;
        //pauseButton.SetActive(true);
        singleModeSession.SetIsShowingResults(false);

    }

    // Calculo del resultado de la jugada
    public InGameStates CalcResult(Attacks attackOne, Attacks attackTwo)
    {
        // Utilizar la matriz de resultados y ver el valor. Se relaciona a Attacks
        int resultInt = results[(int)attackOne, (int)attackTwo];
        InGameStates result;
        switch (resultInt)
        {
            case 1:
                if (singleModeSession) singleModeSession.IncreaseCombo();
                result = InGameStates.Win;
                break;
            case -1:
                if (singleModeSession) singleModeSession.FinishCombo();
                result = InGameStates.Lose;
                break;
            case 0:
            default:
                if (singleModeSession) singleModeSession.FinishCombo();
                result = InGameStates.Draw;
                break;
        }

        if (singleModeSession)
        {
            // Agregar a la secuencia de ataques
            singleModeSession.AddAttackSequence(attackOne);
            singleModeSession.AddParcialResultsSequence(resultInt);
        }

        return result;
    }

    // Calculo del resultado total según los turnos
    public InGameStates CalcWholeResult() {

        // Resultados generales
        int playerCount = 0;

        // Por cada turno, mostrar el ataque correspondiente
        for (int i = 0; i < difficultyTurns; i++)
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

    // Selección aleatoria del enemigo
    public GameObject RandomCPU(GameObject player)
    {
        // Obtener el identificador del player y del actual CPU
        string playerIdentifier = player.GetComponent<CharacterConfiguration>().GetIdentifier();
        string cpuIdentifier = cpuController != null ? cpuController.GetIdentifier() : null;

        GameObject cpu;

        // Revisar si existe un CPU guardado con anterioridad para el nivel
        string lastCPUIdentifier = singleModeSession.GetLastCPUIdentifier();
        if (lastCPUIdentifier != null)
        {
            cpu = CharacterPool.Instance.Get(lastCPUIdentifier);
        }
        else
        {
            // Obtener un indice al azar de entre todos los PJ disponibles
            string newCPUIdentifier = null;
            do
            {
                cpu = CharacterPool.Instance.Surprise();
                newCPUIdentifier = cpu.GetComponent<CharacterConfiguration>().GetIdentifier();
            } while (newCPUIdentifier == cpuIdentifier || newCPUIdentifier == playerIdentifier);
        }

        return cpu;
    }

    // Cambio de la jugada del adversario
    public IEnumerator ChangeCPUAttack()
    {
        while (overlayObjects.Count > 0) yield return null;

        // Se ejecuta hasta que se detenga la coroutine
        while (true)
        {
            // Determinar el ataque del enemigo, se debe tener en cuenta que no sea el mismo que el anterior
            cpuAttack = IntToAttack(-1, cpuAttack);
            cpuController.ChangeCurrentAttack(cpuAttack);
            yield return StartCoroutine(cpuController.FillChangeAttack(difficultySpeed));
        }
    }

    // Apilar los ataques de los jugadores. Si el valor de attack viene como -1, significa que el ataque del jugador fue al azar.
    private void StackAttacks(int playerSelectedAttack)
    {
        // Guardar los ataques
        turnsAttacks[currentTurn, 0] = playerAttack;

        // Si se esta en una jugada destino, asignar directamente el ataque del enemigo
        if (levelType == JankenUp.SinglePlayer.LevelType.Type.Destiny)
        {
            // Determinar si jugador gana o no el combate
            bool playerWins = playerSelectedAttack != -1;

            // Si el jugador no selecciono un ataque, perdera automaticamente por no confiar en su corazon
            if (playerWins)
            {
                // Calcular el % de ganar en base a las ultimas jugadas
                List<int> results = singleModeSession.GetResultsSequence();
                int matchesToReview = results.Count >= JankenUp.SinglePlayer.LevelType.Destiny.matchesToReview ?
                    JankenUp.SinglePlayer.LevelType.Destiny.matchesToReview : results.Count;

                // Obtener solo las victorias
                int victories = 0;

                int[] lastResults = new int[matchesToReview];
                results.CopyTo(results.Count - matchesToReview, lastResults, 0, matchesToReview);
                foreach (int r in lastResults)
                {
                    if (r == 1) victories++;
                }

                // Obtener el porcentaje final para el calculo de la jugada
                int winPercentaje = JankenUp.SinglePlayer.LevelType.Destiny.basePercentaje + victories;

                // Si el porcentaje 100 o mas (Si, mejor asegurarse) o se esta en modo tutorial, ganar automaticamente
                if (winPercentaje < 100 && !IsShowingTuto())
                {
                    List<int> deathNumbers = new List<int>();
                    int deathNumbersCount = 100 - winPercentaje;

                    Random.InitState(System.DateTime.Now.Millisecond);

                    while (deathNumbers.Count != deathNumbersCount)
                    {
                        int newDeathNumber = Random.Range(0, 100);
                        if(!deathNumbers.Contains(newDeathNumber)) deathNumbers.Add(newDeathNumber);

                    }

                    // Seleccionar el numero que tendra el destino de la partida en sus bytes
                    Random.InitState(System.DateTime.Now.Millisecond + 1);
                    int theDeathNumber = Random.Range(0, 100);

                    // Calcular si el jugador gano o perdio
                    playerWins = !deathNumbers.Contains(theDeathNumber);

                }

            }

            // Determinar la jugada del CPU
            switch (playerAttack)
            {
                case Attacks.Rock:
                    cpuAttack = playerWins ? Attacks.Scissors : Attacks.Paper;
                    break;
                case Attacks.Paper:
                    cpuAttack = playerWins ? Attacks.Rock : Attacks.Scissors;
                    break;
                case Attacks.Scissors:
                    cpuAttack = playerWins ? Attacks.Paper : Attacks.Rock;
                    break;
            }

        }
        else
        {
            // Si el ataque es de tipo magicWand y se ha superado el limite de ataques, asignar al enemigo un magicWand
            if (playerAttack == Attacks.MagicWand && singleModeSession.GetSuperPowerUPCounter() >= JankenUp.Limits.superPowerUpLimit) cpuAttack = Attacks.MagicWand;
        }

        // Asignar la jugada de la CPU
        turnsAttacks[currentTurn, 1] = cpuAttack;

        // Indicar en la 'UI' de stack
        AttackStack playerStack = playerPosition == 0 ? leftStack : rightStack;
        if(playerStack) playerStack.ChangeAttack(currentTurn, playerAttack);
    }

    // Elimina los elementos del stack de atauqe
    private void ResetStackAttacks() {
        currentTurn = 0;
        if(leftStack) leftStack.Reset();
        if(rightStack) rightStack.Reset();
    }

    // Activar indice X en el stack Y
    private void ActivateStackAttack( int index, AttackStack stack)
    {
        if(stack) stack.ActiveAttack(index);
    }

    /* Método temporal para ocultar el tutorial*/
    public void AfterTutorial()
    {
        // Iniciar nuevamente el contador
        StartCoroutine(CountdownSequence());
    }

    /* Revisión de si es posible o no quitar el illuminati del enemigo */
    public bool DestroyIlluminati()
    {
        if (inGameSequence.IsSuperJankenUpActive()) return false;

        if (singleModeSession)
        {

            // Revisar si es posible gastar monedas
            if (singleModeSession.SpendCoins(illuminatiCost))
            {
                // Reproducir los sonidos asociados
                MasterSFXPlayer._player.PlayOneShot(illuminatiSFX);
                audioSource.PlayOneShot(spendCoinsSuccess);

                UpdateUI();

                // Si no hay tutorial, resetear el contador
                if (!IsShowingTuto()) ResetCountdown();
                else if (difficulty.IlluminatiTutorialLevel(singleModeSession.GetLevel())) ToggleAttackUI(true);

                illuminati = false;

                return true;
            }
            else
            {
                audioSource.PlayOneShot(spendCoinsFail);
                return false;
            }
        }

        return true;

    }

    // Saber si actualmente hay una jugada illuminati en curso
    public bool IsIlluminati() {
        return illuminati;
    }

    // Saber si el nivel actual opera como un tutorial y el jugador no lo ha pasado
    public bool IsShowingTuto()
    {
        return isTuto && playerNoPlayTuto;
    }

    // Obtener la cantidad de turnos actuales
    public int CountTurns() {
        return difficultyTurns;
    }

    // Obtener el identificador del rival actual
    public string GetCPUIdentifier()
    {
        return cpu.GetComponent<CharacterConfiguration>().GetIdentifier();
    }

    /// <summary>
    /// Consulta la posiblidad de abrir menu
    /// </summary>
    /// <returns></returns>
    public override bool CanToggleMenu()
    {
        // Si se esta ejecutando un super ataque no permitir la pausa o ya se selecciono ataque
        return !attackOptions.IsSuperAttackExecuting() && !moveSelected && !isGoingToResult && !attackSequenceInCourse && !TransitionDoors._this.IsInTransition();
    }

    /// <summary>
    /// Recepcion del estado del menu
    /// </summary>
    /// <param name="state"></param>
    protected override void OnToggleMenu(OptionsMenu.OptionsMenuStates state) {

        bool newPauseState = state == OptionsMenu.OptionsMenuStates.Open;
        // Nota: Lo demas es reciclaje de codigo que funciona, podria ser optimizado pero funciona. Funciona.

        // Invertir pausa para mantener estado. Guardar ademas la escala de tiempo actual para recuperar al volver de pausa
        bool isTheSamePauseState = newPauseState == pauseState;
        pauseState = newPauseState;
        if (pauseState && !isTheSamePauseState) prePauseTimeScale = Time.timeScale;

        //Time.timeScale = pauseState ? 0 : prePauseTimeScale;

        // Si el juego esta pausado, mostrar el UI de pausa. Además, no mostrar ataque enemigo
        cpuController.ShowPreAttack(!pauseState && currentTurn < difficultyTurns && refereeFirstShoutReady && overlayObjects.Count == 0);

        // Cambiar el ataque si se volvio de la pausa
        if (!pauseState)
        {
            if (!tutorial.activeSelf) StartCoroutine(CountdownSequence());
            if (difficultySpeed != -1 && levelType != JankenUp.SinglePlayer.LevelType.Type.Destiny && !attackSequenceInCourse)
            {
                if (changeAttackCoroutine != null) StopCoroutine(changeAttackCoroutine);
                if (tutorial.activeSelf) CheckTutorialAttackCoroutine(false);
                else changeAttackCoroutine = StartCoroutine(ChangeCPUAttack());
            }
        }
        else
        {
            levelDisplayer.Hide();
            referee.HideAnnouncement();
            timeDisplayer.StopTimer();
            StopAllCoroutines();
        }
    }

    // Volver a la pantalla de seleccion
    public void Back() {
        timeDisplayer.StopTimer();
        Time.timeScale = 1;
        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        if (slm) slm.Back();
    }

    // Mostrar/Ocultar las opciones de ataque
    public void ToggleAttackUI(bool show)
    {
        attackOptions.gameObject.SetActive(show);
    }

    /* Los siguientes metodos cumplen la funcion de actualizar los distintos labels que aparecen la pantalla*/
    public void UpdateResumeString(string text)
    {
        resumeLabel.text = text;
    }

    public void UpdateBackString(string text)
    {
        backLabel.text = text;
    }


    // Actualiza todos los elementos ligados a un translate
    protected override void Localize()
    {
        //LocalizationHelper.Translate(resumeLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.resume);
        //LocalizationHelper.Translate(backLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.back);
        LocalizationHelper.Translate(harderUIText, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.harder);
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        Font plainFont = FontManager._mainManager.GetPlainFont();
        //resumeLabel.font = plainFont;
        //backLabel.font = plainFont;

        levelDisplayer.UpdateCurrentFont();
    }

    // Mostrar la obtencion de un superpoder en el jugador
    public void ShowGetSuperPower(SuperPowers superPower)
    {
        MasterSFXPlayer._player.WinSuperPower();
        StartCoroutine(playerController.ShowGetSuperPower(superPower));
        UpdateAttackUI();
    }

    // Super JanKenUp
    public void PerformJanKenUp()
    {
        currentCountdownSteps = -1;
        StopAllCoroutines();
        StartCoroutine(CoroutineJanKenUp());
    }

    private IEnumerator CoroutineJanKenUp()
    {
        // Detener la alerta en el Canvas
        alertCanvas.Stop();

        // Ocultar toda la UI
        HideUI();

        // Detener el tiempo
        timeDisplayer.StopTimer();

        // Sumar al contador de superataque. Si ya son 3 seguidos, CPU tirara su super tambien
        singleModeSession.IncreaseSuperJanKenUPCounter();
        singleModeSession.SetIsShowingResults(true);

        // Ocultar UI innecesaria
        ToggleAttackUI(false);
        referee.HideAnnouncement();
        levelDisplayer.Hide();
        //pauseButton.SetActive(false);

        // Quitar la barra de carga y el preAtaque del enemigo
        cpuController.ShowPreAttack(false);

        // Disminuir el volumen de la musica
        MasterAudioPlayer._player.FadeAudioSourceSuper(false);

        // Si el jugador ha llegado al limite de veces seguidas del SuperJanKenUP!, se le para la mano
        bool cpuHasSuperJanKenUP = singleModeSession.GetSuperPowerUPCounter() >= JankenUp.Limits.superPowerUpLimit;

        // Indicar la ejecucion del super al InGameSequence
        yield return StartCoroutine(inGameSequence.ExecuteSuperJanKenUP(playerController, cpuController, true, cpuHasSuperJanKenUP, false, false));

        // Ocultar toda la UI
        ShowUI();

        if (singleModeSession && !cpuHasSuperJanKenUP)
        {
            singleModeSession.IncreaseCombo();
            singleModeSession.AddParcialResultsSequence(1);
            comboDisplayer.SetValue(singleModeSession.GetCurrentCombo(), singleModeSession.IsNewComboRecord());
        }

        // Aumentar el volumen de la musica
        MasterAudioPlayer._player.FadeAudioSourceSuper(true);

        // Agregar triunfo inmediato
        if (!cpuHasSuperJanKenUP)
        {
            // Spawnear al CPU
            FinishSpawnCPU(SpawnCPU());

            if (singleModeSession)
            {
                singleModeSession.AddResultsSequence(1);
                singleModeSession.Win();
            }

            UpdateUI();

            // Agregar el tiempo que gano el usuario
            timeDisplayer.AddTime(timeAddOnWin * (levelType == JankenUp.SinglePlayer.LevelType.Type.Normal ? 1 : 3));

            StartCoroutine(ReStartMatch());

        }
        else {

            // Quitar tiempo
            timeDisplayer.SubtractTime(timeSubtractOnDraw * (levelType == JankenUp.SinglePlayer.LevelType.Type.Normal ? 1 : 2));

            // Resetear el stack y el turno actual
            ResetStackAttacks();

            // Iniciar countdown
            StartCoroutine(CountdownSequence());
        }

        // Indicar que ya se ejecuto super ataque
        attackOptions.SuperAttackExecuted();

        // Retomar el tiempo
        timeDisplayer.StartTimer();
        singleModeSession.SetIsShowingResults(false);

    }

    // Reproducir sonido de super golpe
    public void PlayStrongHit(bool crowd)
    {
        if(crowd) MasterSFXPlayer._player.PlayOneShot(destinyLevelSFX);
        audioSource.PlayOneShot(strongPunchSFX);
    }

    // Funcion llamada despues de comprar el paquete deluxe
    public void AfterPurchaseDeluxe()
    {
        // Resetear el stack y el turno actual
        ResetStackAttacks();

        // Actualizar UI
        UpdateUI();

        // Actualizar ataques especiales
        attackOptions.UpdateUI();

        // Continuar con el contador
        timeDisplayer.StartTimer();

        // Iniciar countdown
        StartCoroutine(CountdownSequence());
    }

    // Obtener el tipo de nivel actual
    public JankenUp.SinglePlayer.LevelType.Type GetLevelType()
    {
        return levelType;
    }

    // Finalizacion del tiempo de juego
    public override void TimeOut()
    {
        // Indicar que ya no puede recibir ataques
        timeOut = true;

        // Quitar las opciones de pantalla
        ToggleAttackUI(false);
    }

    // Ir a los resultados de la partida
    private IEnumerator GoToResults()
    {
        // Detener la alerta en el Canvas
        alertCanvas.Stop();

        isGoingToResult = true;

        // Tween para la aumentar y disminuir el tiempo
        System.Action<ITween<float>> updateTimeAndPitch = (t) =>
        {
            MasterAudioPlayer._player.ChangePitchAudio(t.CurrentValue);
        };

        // Ejecutar la desaceleracion de la musica
        gameObject.Tween(string.Format("Deceleration{0}", GetInstanceID()), MasterAudioPlayer._player.GetPitchAudio(), minMusicPitch, changeMusicPitchTime, TweenScaleFunctions.QuadraticEaseOut, updateTimeAndPitch);

        // Guardar la dificultad actual menos 1
        difficulty.SaveFinalDifficulty();

        // Cambiar el estado de los players y referee
        if (singleModeSession.GetLives() <= 0)
        {
            referee.ChangeState(playerPosition == 1 ? InGameStates.Win : InGameStates.Lose);
            cpuController.ShowPreAttack(false);
            // Espera final
            if (inGameSequence) yield return StartCoroutine(inGameSequence.FinalWait());
            cpuController.ChangeState(InGameStates.Win);
        }
        else
        {
            referee.ChangeState(InGameStates.Draw);
            cpuController.ShowPreAttack(false);
            // Espera final
            if (inGameSequence) yield return StartCoroutine(inGameSequence.FinalWait());
            playerController.ChangeState(InGameStates.Stand);
            cpuController.ChangeState(InGameStates.Stand);
        }

        // Sumatoria final del tiempo acumulado al tiempo que se mantuvo el jugador en la plataforma (Solo si se ha superado los 10 segundos de aguante)
        if (timeDisplayer.GetTimeElapsed() >= 10) timeDisplayer.AddRemainingToElapsed(timeFinalWait);
        yield return new WaitForSeconds(timeFinalWait);

        // Guardar los tiempos finales en la sesion
        singleModeSession.SetTimeElapsed(timeDisplayer.GetTimeElapsed(), false);
        singleModeSession.SetTimeRemaining(timeDisplayer.GetTimeRemaining());

        SceneLoaderManager slm = FindObjectOfType<SceneLoaderManager>();
        if (slm)
        {
            if (UnlockedCharacterController.AreThereNewCharacters()) slm.UnlockedCharacter(SceneLoaderManager.SceneNames.SurvivalModeResults);
            else slm.SurvivalModeResults();
        }
    }

    // Al pausar el juego, deberia mostrarse la pantalla de pausa
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !TransitionDoors._this.IsInTransition())
        {
            preparedForPause = false;
            optionsMenu.Toggle(true);
        }
        else
        {
            preparedForPause = true;
        }
    }

    /*
     * Comprobacion de si uno de los actores actuales necesita la presencia de un crowd NPC en la arena
     * @return {void}
     * **/
    private void ActorsNeedSupportNpc()
    {
        JankenUp.NPC.Identifiers playerSupportNpc = playerController.GetSupportNPCIdentifier();
        JankenUp.NPC.Identifiers cpuSupportNpc = cpuController.GetSupportNPCIdentifier();

        if (playerSupportNpc != JankenUp.NPC.Identifiers.None) arena.CheckForSupportNPC(playerSupportNpc, playerController);
        if (cpuSupportNpc != JankenUp.NPC.Identifiers.None) arena.CheckForSupportNPC(cpuSupportNpc, cpuController);
    }

    /// <summary>
    /// Actualizacion del UI de ataque
    /// </summary>
    public override void UpdateAttackUI()
    {
        base.UpdateAttackUI();

        // Actualizar ataques especiales
        attackOptions.UpdateUI();
    }


    /// <summary>
    /// Desuscribirse de los metodos de ataque
    /// </summary>
    protected new void OnDestroy()
    {
        base.OnDestroy();
        GameController.SetGameplayActive(false);
        // Quitar localizacion
        LanguageController.onLanguageChangeDelegate -= Localize;
        // Configurar botones
        AttackOptions.onAttackSelectDelegate -= SelectOption;
        AttackOptions.onMagicWandDelegate -= SelectOption;
        AttackOptions.onSuperDelegate -= PerformJanKenUp;
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
            case JoystickAction.Escape:
                if (optionsMenu != null && optionsMenu.isActiveAndEnabled) optionsMenu.Toggle();
                break;
            case JoystickAction.Y:
                // Illuminati
                if (illuminati) cpuController.OnMouseDown();
                break;
            case JoystickAction.X:
                // Por defecto: Piedra
                attackOptions.AttackByPosition(0);
                break;
            case JoystickAction.A:
                // Por defecto: Papel
                attackOptions.AttackByPosition(1);
                break;
            case JoystickAction.B:
                // Por defecto: Tijera
                attackOptions.AttackByPosition(2);
                break;
            case JoystickAction.L:
            case JoystickAction.J:
                // TimeMaster
                attackOptions.TimeMaster();
                break;
            case JoystickAction.ZR:
            case JoystickAction.ZL:
            case JoystickAction.LKeyboard:
                // Super JanKenUP
                attackOptions.JanKenUp();
                break;
            case JoystickAction.R:
            case JoystickAction.K:
                // MagicWand
                attackOptions.MagicWandClick();
                break;
        }
        return canContinue;
    }
    #endregion

}
