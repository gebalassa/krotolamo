using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JankenUp;
using GooglePlayGames;
using System;
using Random = UnityEngine.Random;
using System.Linq;

public class SingleModeSession : JankenSession
{

    [Header("Stats")]
    [SerializeField] int level = 0;
    [SerializeField] int lives = 3;
    [SerializeField] int maxLives = 3;

    [Header("Earn lives")]
    [SerializeField] int eachLevels03 = 5;
    [SerializeField] int eachLevels46 = 4;
    [SerializeField] int eachLevels710 = 3;

    // Tiempos
    float timeElapsed = -1;
    float timeRemaining = -1;
    int floorTimeElapsed = 0;

    // Indicador de la dirección del ascensor
    bool isGoingUp = false;
    bool firstLevel = true;
    bool alwaysLoseLives = false;

    // Combos: El puntaje de combo actual se multiplica por el factor y ese puntaje se une al puntaje final
    int currentCombo = 0;
    int maxCombo = 0;
    int comboFactor = 50;
    List<int> combos = new List<int>();

    // Puntos extra por nivel de dificultad
    int difficultyExtraPointsDelta = 50;
    int currentExtraPoints = 0;

    // Contador para la cantidad de victorias
    int currentWinsStreak = 0;

    // Puntos extras por cumplir con la meta de streak y no recuperar vida
    int streakExtraPointsDelta = 50;

    // Estadisticas
    int levelRecord = -1;
    int comboRecord = -1;
    int scoreRecord = -1;
    float timeRecord = -1;

    // Valores extras en cuanto a superpoderes
    int playerTimeMaster = 0;
    int playerMagicWand = 0;
    int playerJanKenUP = 0;

    // Contador de veces seguidas que se lanzo SuperJanKenUP!
    int superPowerUpCounter = 0;

    // Modos de juego
    bool classicMode = false;
    bool survivalMode = false;
    bool duelMode = false;

    // Listado con todos los logros ya reportados en la sesion
    List<string> readyAchievements = new List<string>();

    // Se guarda el ultimo mob enfrentado en caso de ir al menu de opciones
    string lastCPUIdentifier = null;

    // Ronda actual
    int currentRound = 1;
    float currentDifficulytSpeed = -1;
    float lastDifficulytSpeed = -1;
    int difficulytLevel = -1;
    int lastDifficulytLevel = -1;

    // Ads
    bool adsWatched = false;

    // Duelo actual
    bool isShowingResults = false;

    // Para duelos
    List<int> playersLives = new List<int>();
    List<int> playersMagicWand = new List<int>();
    List<int> playersSuperJanken = new List<int>();
    List<int> playersVictories = new List<int>();
    List<int> playersDefeats = new List<int>();
    List<int> playersTies = new List<int>();
    List<int> playersCombos = new List<int>();
    List<int> playersRanking = new List<int>();
    List<int> playersStreak = new List<int>();
    List<List<Attacks>> playersAttackSequence = new List<List<Attacks>>();

    // Util para cuando se inicia desde simulacion
    bool noInitialSetup = false;

    // Revision si es deluxe para agregar superpoderes
    private void Start()
    {
        // Revision de si es deluxe
        CheckDeluxe();

        // Carga de los poderes comprados por el jugador
        playerTimeMaster = GameController.GetSuperPower(SuperPowers.TimeMaster);
        playerMagicWand = GameController.GetSuperPower(SuperPowers.MagicWand);
        playerJanKenUP = GameController.GetSuperPower(SuperPowers.JanKenUp);

        if (!noInitialSetup)
        {
            SetPlayersLives();
            SetPlayersPowers();
            SetPlayersStatistics();
        }
    }

    /// <summary>
    /// Cambiar el valor de la configuracion inicial
    /// </summary>
    /// <param name="value"></param>
    public void SetNoInitialSetup(bool value = true)
    {
        noInitialSetup = value;
    }

    /// <summary>
    /// Configuracion de modo duelo
    /// </summary>
    /// <param name="lives"></param>
    public void ConfigDuelMode(int lives = 1) {
        ActivateDuelMode(lives);
    }

    /// <summary>
    /// Seteo de vidas de jugadores
    /// </summary>
    private void SetPlayersLives(int customLives = -1)
    {
        playersLives.Clear();
        // Generar las vidas para todos los jugadores
        int max = GameController.GetSimulation()? CharacterPool.Instance.GetPlayersCount() - 1 : (CharacterPool.Instance.GetPlayersAllowedToDuel().Count > 0? CharacterPool.Instance.GetPlayersAllowedToDuel().Max() : 8);
        for (int i = 0; i <= max; i++)
        {
            int pLives = GameController.GetSimulation() || CharacterPool.Instance.GetPlayersAllowedToDuel().Contains(i) ? (customLives == -1 ? lives : customLives) : 0; 
            playersLives.Add(pLives);
        }
    }

    /// <summary>
    /// Agregar vidas a un personaje
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="count"></param>
    public void AddPlayerLives(int playerIndex = 0, int count = 1)
    {
        if (playersLives[playerIndex] <= 0) return;
        playersLives[playerIndex] += count;
    }

    /// <summary>
    /// Descuento de vidas en de un personaje
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="count"></param>
    public void SubtractLives(int playerIndex = 0, int count = 1)
    {
        if (playersLives[playerIndex] <= 0) return;
        playersLives[playerIndex] -= count;
        if (playersLives[playerIndex] == 0) AddPlayerToRanking(playerIndex);
    }

    /// <summary>
    /// Seteo de poderes para jugadores
    /// </summary>
    public void SetPlayersPowers(int customMagicWand = -1, int customSuperJanken = -1)
    {
        playersMagicWand.Clear();
        playersSuperJanken.Clear();
        // Generar los poderes por cada jugador
        int max = GameController.GetSimulation() ? CharacterPool.Instance.GetPlayersCount() - 1 : (CharacterPool.Instance.GetPlayersAllowedToDuel().Count > 0 ? CharacterPool.Instance.GetPlayersAllowedToDuel().Max() : 8);
        for (int i = 0; i <= max; i++)
        {
            int pMagicWands = GameController.GetSimulation() || CharacterPool.Instance.GetPlayersAllowedToDuel().Contains(i) ? (customMagicWand == -1 ? 1 : customMagicWand) : 0;
            int pSuperJanken = GameController.GetSimulation() || CharacterPool.Instance.GetPlayersAllowedToDuel().Contains(i) ? (customSuperJanken == -1 ? 1 : customSuperJanken) : 0;
            playersMagicWand.Add(pMagicWands);
            playersSuperJanken.Add(pSuperJanken);
        }
    }

    /// <summary>
    /// Obtencion de la cantidad de MagicWand del jugador requerido
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public int GetPlayerMagicWand(int playerIndex = 0)
    {
        return playersMagicWand[playerIndex];
    }

    /// <summary>
    /// Agrega un superpoder de varta magica
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayerMagicWand(int playerIndex = 0)
    {
        playersMagicWand[playerIndex]++;
    }

    /// <summary>
    /// Descuento de un poder MagicWand
    /// </summary>
    /// <param name="playerIndex"></param>
    public bool SubstractMagicWand(int playerIndex = 0)
    {
        if (playersMagicWand[playerIndex] > 0)
        {
            playersMagicWand[playerIndex]--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Obtencion de la cantidad de SuperJanken del jugador requerido
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public int GetPlayerSuperJanken(int playerIndex = 0)
    {
        return playersSuperJanken[playerIndex];
    }

    /// <summary>
    /// Agrega un superpoder SuperJanken al jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayerSuperJanken(int playerIndex = 0)
    {
        playersSuperJanken[playerIndex]++;
    }

    /// <summary>
    /// Descuento de un poder SuperJanken
    /// </summary>
    /// <param name="playerIndex"></param>
    public bool SubstractSuperJanken(int playerIndex = 0)
    {
        if (playersSuperJanken[playerIndex] > 0)
        {
            playersSuperJanken[playerIndex]--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Seteo de las estadisticas de los jugadores
    /// </summary>
    private void SetPlayersStatistics()
    {
        playersVictories.Clear();
        playersDefeats.Clear();
        playersTies.Clear();
        playersCombos.Clear();
        playersStreak.Clear();
        playersAttackSequence.Clear();
        playersRanking.Clear();
        // Generar las victorias para todos los jugadores
        int max = GameController.GetSimulation() ? CharacterPool.Instance.GetPlayersCount() - 1 : (CharacterPool.Instance.GetPlayersAllowedToDuel().Count > 0 ? CharacterPool.Instance.GetPlayersAllowedToDuel().Max() : 8);
        for (int i = 0; i <= max; i++)
        {
            playersVictories.Add(0);
            playersDefeats.Add(0);
            playersTies.Add(0);
            playersCombos.Add(0);
            playersStreak.Add(0);
            playersAttackSequence.Add(new List<Attacks>());
        }
    }

    /// <summary>
    /// Agregar una victoria al jugador indicado por index
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayerVictory(int playerIndex = 0)
    {
        playersVictories[playerIndex]++;
    }

    /// <summary>
    /// Obtencion de la cantidad de victorias de un jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    public int GetPlayerVictory(int playerIndex = 0)
    {
        return playersVictories[playerIndex];
    }

    /// <summary>
    /// Agregar una derrota al jugador indicado por index
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayerDefeat(int playerIndex = 0)
    {
        playersDefeats[playerIndex]++;
    }

    /// <summary>
    /// Obtencion de la cantidad de derrotas de un jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    public int GetPlayerDefeats(int playerIndex = 0)
    {
        return playersDefeats[playerIndex];
    }

    /// <summary>
    /// Agregar un empate de los jugadores
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayersTie(int playerIndex = 0)
    {
        playersTies[playerIndex]++;
    }

    /// <summary>
    /// Obtencion de los empates de los jugadores
    /// </summary>
    /// <returns></returns>
    public int GetPlayerTies(int playerIndex = 0)
    {
        return playersTies[playerIndex];
    }

    /// <summary>
    /// Agregar un combo de los jugadores
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayersCombos(int combo = 0, int playerIndex = 0)
    {
        if (playersCombos[playerIndex] < combo) playersCombos[playerIndex] = combo;
    }

    /// <summary>
    /// Obtencion de los combos de los jugadores
    /// </summary>
    /// <returns></returns>
    public int GetPlayerCombos(int playerIndex = 0)
    {
        return playersCombos[playerIndex];
    }

    /// <summary>
    /// Agregar un streak de los jugadores
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayersStreak(int playerIndex = 0)
    {
        playersStreak[playerIndex]++;
    }

    /// <summary>
    /// Obtencion de los streak de los jugadores
    /// </summary>
    /// <returns></returns>
    public int GetPlayerStreak(int playerIndex = 0)
    {
        return playersStreak[playerIndex];
    }

    /// <summary>
    /// Reseteo del streak de un jugador
    /// </summary>
    /// <param name="playerIndex"></param>
    public void ResetPlayerStreak(int playerIndex = 0) {
        playersStreak[playerIndex] = 0;
    }

    /// <summary>
    /// Agregar un ataque a la secuencia de un jugador
    /// </summary>
    /// <param name="attack"></param>
    /// <param name="playerIndex"></param>
    public void AddPlayersAttack(Attacks attack, int playerIndex = 0)
    {
        playersAttackSequence[playerIndex].Add(attack);
    }

    /// <summary>
    /// Obtencion de los ataques de un jugador durante la partida
    /// </summary>
    /// <returns></returns>
    public List<Attacks> GetPlayerAttackSequence(int playerIndex = 0)
    {
        return playersAttackSequence[playerIndex];
    }

    // Obtención del nivel actual
    public int GetLevel()
    {
        return level;
    }

    // Subir de nivel
    private void UpLevel() {

        // Agregar al streak
        currentWinsStreak++;

        // Agregar los puntos extras por dificultad
        SingleModeDifficulty smd = FindObjectOfType<SingleModeDifficulty>();
        SurvivalModeDifficulty survivalmd = FindObjectOfType<SurvivalModeDifficulty>();
        if ( (smd || survivalmd) && level >= 0) currentExtraPoints += (smd? smd.GetDifficultyLevel() : survivalmd.GetDifficultyLevel()) * difficultyExtraPointsDelta;

        // Si se esta en piso superior a nivel 0 y en smd, siempre se quitara vida
        if (smd)
        {
            if (level >= 1) AlwaysLoseLives(true);
            else AlwaysLoseLives(false);
        }

        level++;
        // Piso 4 no existe, asi que saltarlo si dio ese resultado
        if (level == 4 ) level++;
        isGoingUp = true;
    }

    // Bajar de nivel
    private void DownLevel() {

        // Se acabo el streak de ganadas
        currentWinsStreak = 0;

        // Si estaba subiendo, debe restarse una vida
        if (isGoingUp || alwaysLoseLives)
        {
            isGoingUp = false;

            SubtractLives();
            if( GetLives() <= 0)
            {
                SetPlayersLives(0);
            }
        }

        level--;
        // Piso 4 no existe, asi que saltarlo si dio ese resultado
        if (level == 4) level--;
    }

    // Incremento de conteo de jugadas ganadas
    public new void Win() {
        base.Win();
        UpLevel();
        AddCoins();
        CheckCompleteRules();
    }

    // Incremento de conteo de jugadas perdidas
    public new void Lose() {
        base.Lose();
        DownLevel();
        SubtractCoins();
        CheckCompleteRules();
    }

    // Empates
    public new void Draw()
    {
        base.Draw();
        // Si se esta en modo survival, quitar monedas al empatar
        SurvivalModeDifficulty survivalmd = FindObjectOfType<SurvivalModeDifficulty>();
        if (survivalmd) SubtractCoins();
    }

    /// <summary>
    /// Obtención de las vidas de un jugador
    /// </summary>
    /// <returns></returns>
    public int GetLives(int playerIndex = 0) {
        if (playersLives.Count <= playerIndex) return 0;
        return playersLives[playerIndex];
    }

    /// <summary>
    /// Revision de si al menos X jugadores aun estan vivos
    /// </summary>
    /// <returns></returns>
    public bool AtLeastXPlayersAlive(int x = 2) {
        int p = 0;
        foreach(int lives in playersLives)
        {
            if (lives > 0) p++;
        }
        return p >= x;
    }

    // Obtener si se esta subiendo o no
    public bool IsGoingUp() { return isGoingUp; }

    // Revisar si es un nuevo record de nivel
    public bool IsNewLevelRecord()
    {
        if (levelRecord == -1) levelRecord = GameController.Load().levelRecord;
        bool isNew = levelRecord < level;

        if (isNew)
        {
            levelRecord = level;
            GameController.SaveLevelRecord(level);
        }

        return isNew;
    }

    // Revisar si es un nuevo record de combos
    public bool IsNewComboRecord()
    {
        if (comboRecord == -1) comboRecord = GameController.Load().comboRecord;
        bool isNew = comboRecord < currentCombo;

        if (isNew)
        {
            comboRecord = currentCombo;
            GameController.SaveCombosRecord(comboRecord);
        }

        return isNew;
    }

    // Revisar si es un nuevo record de puntaje
    public bool IsNewScoreRecord()
    {
        if (scoreRecord == -1) scoreRecord = GameController.Load().scoreRecord;
        bool isNew = scoreRecord < GetScore();

        if (isNew)
        {
            scoreRecord = GetScore();
            GameController.SaveScoreRecord(scoreRecord);
        }

        return isNew;
    }

    // Obtener si es el primer nivel y cambiar
    public bool IsFirstLevel()
    {
        if (!firstLevel) return false;
        firstLevel = false;
        return true;
    }

    // Resetear los valores de la sesión
    public new void Reset()
    {
        base.Reset();
        level = 0;
        lives = 3;
        maxLives = 3;
        SetPlayersLives();
        currentCombo = 0;
        maxCombo = 0;
        combos.Clear();
        timeMaster = 0;
        magicWand = 0;
        janKenUp = 0;
        alwaysLoseLives = false;
        timeElapsed = -1;
        timeRemaining = -1;
        floorTimeElapsed = 0;
        currentExtraPoints = 0;
        superPowerUpCounter = 0;
        currentWinsStreak = 0;
        lastCPUIdentifier = null;
        isGoingUp = false;
        currentRound = 1;
        currentDifficulytSpeed = -1;
        lastDifficulytSpeed = -1;
        difficulytLevel = -1;
        lastDifficulytLevel = -1;
        adsWatched = false;
        CheckDeluxe();

        // Duelo
        playersRanking.Clear();
    }

    // Agregar monedas
    public new void AddCoins()
    {
        SingleModeDifficulty smd = FindObjectOfType<SingleModeDifficulty>();
        SurvivalModeDifficulty survivalmd = FindObjectOfType<SurvivalModeDifficulty>();
        if (smd || survivalmd)
        {
            int plus = (smd? smd.GetCoinsPerLevel(level) : survivalmd.GetCoinsPerLevel(level)) * (GameController.IsDeluxe()? 2 : 1);

            // Si es un nivel triple o de destiny, dar triple monedas
            SinglePlayer.LevelType.Type levelType = SinglePlayer.LevelType.Type.Normal;
            if (classicMode) {
                SingleModeController smc = FindObjectOfType<SingleModeController>();
                if (smc) levelType = smc.GetLevelType();
            }
            else
            {
                SurvivalModeController surmc = FindObjectOfType<SurvivalModeController>();
                if (surmc) levelType = surmc.GetLevelType();
            }

            if (levelType == SinglePlayer.LevelType.Type.Destiny || levelType == SinglePlayer.LevelType.Type.Triple) plus *= 3;

            coins += plus;
        }
    }

    public void AddCoins(int coins)
    {
        this.coins += coins;
    }

    // Sustraer monedas
    public new void SubtractCoins()
    {
        SingleModeDifficulty smd = FindObjectOfType<SingleModeDifficulty>();
        SurvivalModeDifficulty survivalmd = FindObjectOfType<SurvivalModeDifficulty>();
        if (smd) coins -= smd.GetCoinsPerLevel(level);
        else if (survivalmd) coins -= survivalmd.GetCoinsPerDraw();
        if (coins < 0) coins = 0;
    
    }

    // Aumentar el combo actual y registrar el max combo si ha sido superado
    public void IncreaseCombo() {
        currentCombo++;
        if (currentCombo > maxCombo) maxCombo = currentCombo;
    }

    // Obtención del combo actual
    public int GetCurrentCombo() {
        return currentCombo;
    }

    // Obtención del combo máximo de la sesión
    public int GetMaxCombo()
    {
        return maxCombo;
    }

    // Reset del combo actual, aplicando la sumatoria
    public void FinishCombo()
    {
        combos.Add(comboFactor * currentCombo);
        currentCombo = 0;
    }

    /* Calculo de puntaje según formula
     * La formula usada será (victorias - derrotas - 0.5 * empates) * piso alcanzado + sumatoria de combos;
     */
    public int GetScore()
    {
        int combosSum = 0;
        combos.ForEach(c => combosSum += c);

        // Se sumará el combo actual también
        return (int)((victories - defeats - 0.5f * ties) * level) + combosSum + (comboFactor * currentCombo) + currentExtraPoints;
    }

    // Se indica que un nuevo personaje ha sido desbloqueado
    public void CheckCompleteRules()
    {
        // Obtener el controlador de single player
        SingleModeController smc = FindObjectOfType<SingleModeController>();

        // Listado con todos los logros desbloqueados
        List<string> achievements = new List<string>();

        // Ver los logros por tipo
        if (IsInClassicMode())
        {

            // School time: Sobrepasar el nivel 5
            if (level == 6)
            {
                if (!UnlockedCharacterController.IsUnlocked(Characters.GIRLTWO)) UnlockedCharacterController.NewUnlock(Characters.GIRLTWO);

                if (!readyAchievements.Contains(Characters.GIRLTWO))
                {
                    achievements.Add(Characters.achievements.GetAchievementId(Characters.GIRLTWO));
                    readyAchievements.Add(Characters.achievements.GetAchievementId(Characters.GIRLTWO));
                }
            }

            // Illuminati: Ganar una jugada iluminati sin saber que va a tirar el adversario
            if (smc && smc.IsIlluminati() && !smc.IsShowingTuto() && resultsSequence[resultsSequence.Count - 1] == 1)
            {
                if (!UnlockedCharacterController.IsUnlocked(Characters.YOUNGMAN)) UnlockedCharacterController.NewUnlock(Characters.YOUNGMAN);

                if (!readyAchievements.Contains(Characters.YOUNGMAN))
                {
                    achievements.Add(Characters.achievements.GetAchievementId(Characters.YOUNGMAN));
                    readyAchievements.Add(Characters.achievements.GetAchievementId(Characters.YOUNGMAN));
                }
            }

            // Rock&Rock: Ganar dos partidas seguidas tirando piedra
            if (resultsSequence.Count > 2 && resultsSequence[resultsSequence.Count - 1] == 1 && resultsSequence[resultsSequence.Count - 2] == 1
                && attackSequence.Count > 2 && attackSequence[attackSequence.Count - 1] == Attacks.Rock && attackSequence[attackSequence.Count - 2] == Attacks.Rock)
            {
                if (!UnlockedCharacterController.IsUnlocked(Characters.YOUNGWOMAN)) UnlockedCharacterController.NewUnlock(Characters.YOUNGWOMAN);

                if (!readyAchievements.Contains(Characters.YOUNGWOMAN))
                {
                    achievements.Add(Characters.achievements.GetAchievementId(Characters.YOUNGWOMAN));
                    readyAchievements.Add(Characters.achievements.GetAchievementId(Characters.YOUNGWOMAN));
                }
            }

            // BigBoy: Llegar a nivel 30
            if (level == 30)
            {
                if (!UnlockedCharacterController.IsUnlocked(Characters.ADULTMAN)) UnlockedCharacterController.NewUnlock(Characters.ADULTMAN);

                if (!readyAchievements.Contains(Characters.ADULTMAN))
                {
                    achievements.Add(Characters.achievements.GetAchievementId(Characters.ADULTMAN));
                    readyAchievements.Add(Characters.achievements.GetAchievementId(Characters.ADULTMAN));
                }
            }

            // I'm proud: Llegar a nivel 20 sin haber perdido ni empatado ninguna tirada
            if (level == 20 && !parcialResultsSequence.Contains(0) && !parcialResultsSequence.Contains(-1))
            {
                if (!UnlockedCharacterController.IsUnlocked(Characters.ADULTWOMAN)) UnlockedCharacterController.NewUnlock(Characters.ADULTWOMAN);

                if (!readyAchievements.Contains(Characters.ADULTWOMAN))
                {
                    achievements.Add(Characters.achievements.GetAchievementId(Characters.ADULTWOMAN));
                    readyAchievements.Add(Characters.achievements.GetAchievementId(Characters.ADULTWOMAN));
                }

            }
            // Wisdom: Ganar la partida triple de nivel 40 sin perder ninguna tirada
            if (level == 41
                && smc
                && smc.GetLevelType() == SinglePlayer.LevelType.Type.Triple
                && parcialResultsSequence.Count >= 3
                && parcialResultsSequence[parcialResultsSequence.Count - 1] == 1
                && parcialResultsSequence[parcialResultsSequence.Count - 2] == 1
                && parcialResultsSequence[parcialResultsSequence.Count - 3] == 1
                )
            {
                if (!UnlockedCharacterController.IsUnlocked(Characters.OLDMAN)) UnlockedCharacterController.NewUnlock(Characters.OLDMAN);
                if (!readyAchievements.Contains(Characters.OLDMAN))
                {
                    achievements.Add(Characters.achievements.GetAchievementId(Characters.OLDMAN));
                    readyAchievements.Add(Characters.achievements.GetAchievementId(Characters.OLDMAN));
                }
            }

            // Polite:  Dejarse perder en un partida triple si te toca con la abuelita.
            if (smc && smc.CountTurns() == 3 && resultsSequence[resultsSequence.Count - 1] == -1 && smc.GetCPUIdentifier() == Characters.OLDWOMAN)
            {
                if (!UnlockedCharacterController.IsUnlocked(Characters.OLDWOMAN)) UnlockedCharacterController.NewUnlock(Characters.OLDWOMAN);
                if (!readyAchievements.Contains(Characters.OLDWOMAN)){
                    achievements.Add(Characters.achievements.GetAchievementId(Characters.OLDWOMAN));
                    readyAchievements.Add(Characters.achievements.GetAchievementId(Characters.OLDWOMAN));
                }
                    
            }

            // Logros por nivel
            string levelAchievement = Achievements.GetAchivementByLevel(level);
            if (levelAchievement != null && !readyAchievements.Contains(levelAchievement))
            {
                achievements.Add(levelAchievement);
                readyAchievements.Add(levelAchievement);
            }
        }

        // Logros por combos
        string combosAchievement = Achievements.GetAchivementByCombos(currentCombo);
        if (combosAchievement != null) achievements.Add(combosAchievement);

        // Solicitar todos los achievements de GooglePlay
        /*if (GooglePlayGamesController._this) { 

            foreach (string achievement in achievements) {
                GooglePlayGamesController._this.ReportAchievement(achievement, 100.0f);
            }

        }*/

    }

    // Revision de los logros en modo survival
    public void CheckSurvivalRules()
    {

        // Listado con todos los logros desbloqueados
        List<string> achievements = new List<string>();

        if (IsInSurvivalMode())
        {
            string[] timeAchievements = Achievements.GetAchivementSurvivalMode(Convert.ToInt32(GetTimeElapsed()));

            if(timeAchievements != null)
            {
                foreach (string timeAchievement in timeAchievements)
                {
                    if (timeAchievement != null && !timeAchievement.StartsWith('x') && !readyAchievements.Contains(timeAchievement))
                    {
                        achievements.Add(timeAchievement);
                        readyAchievements.Add(timeAchievement);
                    }

                    // Si logro estar 30 segundos, desbloquea al cabro chico tocopilla
                    if (timeAchievement == Characters.achievements.BOYMASK && !UnlockedCharacterController.IsUnlocked(Characters.BOYMASK)) UnlockedCharacterController.NewUnlock(Characters.BOYMASK);

                    // Si el logro coincide con el logro de duo (1 minuto), desbloquear PJ
                    if (timeAchievement == Characters.achievements.DUO && !UnlockedCharacterController.IsUnlocked(Characters.DUO)) UnlockedCharacterController.NewUnlock(Characters.DUO);
                    
                    if (timeAchievement == Characters.achievements.GASTONMIAUFFIN && !UnlockedCharacterController.IsUnlocked(Characters.GASTONMIAUFFIN)) UnlockedCharacterController.NewUnlock(Characters.GASTONMIAUFFIN);
                }
            }
            
        }

        // Logros por combos
        string combosAchievement = Achievements.GetAchivementByCombos(currentCombo);
        if (combosAchievement != null) achievements.Add(combosAchievement);

        // Solicitar todos los achievements de GooglePlay
        /*if (GooglePlayGamesController._this)
        {

            foreach (string achievement in achievements)
            {
                GooglePlayGamesController._this.ReportAchievement(achievement, 100.0f);
            }

        }*/

    }

    // Revision por si hay posibilidades de ganar una vida
    public void CheckForLives()
    {

        SingleModeDifficulty smd = FindObjectOfType<SingleModeDifficulty>();
        SurvivalModeDifficulty survivalmd = FindObjectOfType<SurvivalModeDifficulty>();
        if (smd || survivalmd)
        {
            int level = smd? smd.GetDifficultyLevel() : survivalmd.GetDifficultyLevel();
            int neccesary = eachLevels03;

            if( 3 < level && level < 7)
            {
                neccesary = eachLevels46;
            }
            else if(level >= 7)
            {
                neccesary = eachLevels710;
            }

            // Comprobar condicion para ganar vida
            bool earnLife = currentWinsStreak >= neccesary;

            // Indicar la carga actual
            LivesDisplayer livesDisplayer = FindObjectOfType<LivesDisplayer>();
            if (livesDisplayer) livesDisplayer.SetTotalStreak(neccesary, currentWinsStreak);

            // Ver si se gano una vida
            if (earnLife) {

                int preLives = GetLives();
                lives = Mathf.Clamp(GetLives() + 1, 0, maxLives);
                SetPlayersLives(lives);
                currentWinsStreak = 0;

                // Si no se aumento vida, se agrega puntaje
                currentExtraPoints += streakExtraPointsDelta * level;

                // Si no incremento vida, agregar un superpoder al azar
                if(preLives == lives) {

                    // Obtener el superpoder que se gano
                    int superPower = Random.Range(0, Enum.GetNames(typeof(SuperPowers)).Length);
                    StartCoroutine(AnimateSuperPowerEarned(superPower));
                }
                else
                {
                    // Animar el incremento de vida
                    StartCoroutine(AnimateLifeEarned());
                }

            }
        }
    }

    // Animacion cuando se gana una vida
    private IEnumerator AnimateLifeEarned()
    {
        LivesDisplayer livesDisplayer = FindObjectOfType<LivesDisplayer>();
        if (livesDisplayer) yield return StartCoroutine(livesDisplayer.LifeUp(lives));
    }

    // Animacion cuando se consigue un superpoder
    public IEnumerator AnimateSuperPowerEarned(int superPowerInt, bool animateLivesDisplayer = true)
    {
        SuperPowers superPower = GetSuperPower(superPowerInt);

        // Buscar el controlador de juego y avisar del superpoder ganado
        SingleModeController smc = FindObjectOfType<SingleModeController>();
        SurvivalModeController survivalmc = FindObjectOfType<SurvivalModeController>();
        if(smc) smc.ShowGetSuperPower(superPower);
        if(survivalmc) survivalmc.ShowGetSuperPower(superPower);

        if (animateLivesDisplayer)
        {
            LivesDisplayer livesDisplayer = FindObjectOfType<LivesDisplayer>();
            if (livesDisplayer) yield return StartCoroutine(livesDisplayer.FullStrike());
        }
    }
    
    // Obtencion de un superpoder
    public SuperPowers GetSuperPower(int superPowerInt = -1)
    {
        SuperPowers superPower = SuperPowers.MagicWand;

        // Azar
        if (superPowerInt == -1) superPowerInt = Random.Range(0, 3);

        // Si el jugador aun no ha completado el nivel tutorial de poderes, solo recibira varita magica
        bool playerKnowsSuperPowers = GameController.LoadTutorials().tutorials.Contains(Tutorial.GetSuperPowerLevel());
        if (!playerKnowsSuperPowers) superPowerInt = 1;

        switch (superPowerInt)
        {
            case 0:
                timeMaster++;
                superPower = SuperPowers.TimeMaster;
                break;
            case 1:
                magicWand++;
                superPower = SuperPowers.MagicWand;
                break;
            case 2:
                janKenUp++;
                superPower = SuperPowers.JanKenUp;
                break;
        }

        return superPower;
    }

    // Agregar un superpoder directamente
    public void AddSuperPower(SuperPowers superPower, int howMuch)
    {
        switch (superPower)
        {
            case SuperPowers.TimeMaster:
                timeMaster += howMuch;
                break;
            case SuperPowers.MagicWand:
                magicWand += howMuch;
                break;
            case SuperPowers.JanKenUp:
                janKenUp += howMuch;
                break;
        }
    }

    // Revision de estado de deluxe
    public void CheckDeluxe()
    {
        if (GameController.IsDeluxe())
        {
            // Agregar superpoderes disponibles
            int points = 3;
            while(points > 0)
            {
                GetSuperPower(-1);
                points--;
            }

            // Recargar las monedas
            coins = GameController.Load().coins;
        }
    }

    // Setear las vidas actuales
    public void SetCurrentLives(int lives)
    {
        this.lives = lives;
        SetPlayersLives(lives);
    }

    // Setear las vidas maximas
    public void SetMaxLives(int maxLives)
    {
        this.maxLives = maxLives;
    }

    // Permite indicar si se pierde vida siempre que pierda o solo si va ganando
    public void AlwaysLoseLives(bool always)
    {
        alwaysLoseLives = always;
    }

    // Obtención del tiempo transcurrido
    public float GetTimeElapsed()
    {
        return timeElapsed;
    }

    // Setear el tiempo transcurrido
    public void SetTimeElapsed(float timeElapsed)
    {
        SetTimeElapsed(timeElapsed, true);
    }

    // Setear el tiempo transcurrido
    public void SetTimeElapsed(float timeElapsed, bool checkAchievements)
    {
        this.timeElapsed = timeElapsed;
        int currentTimeElapsed = Mathf.FloorToInt(timeElapsed);
        if(floorTimeElapsed != currentTimeElapsed && checkAchievements)
        {
            floorTimeElapsed = currentTimeElapsed;
            CheckSurvivalRules();
        }
    }

    // Obtención del tiempo restante
    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    // Setear el tiempo restante
    public void SetTimeRemaining(float timeRemaining)
    {
        this.timeRemaining = timeRemaining;
    }

    // Revisar si es un nuevo record de tiempo
    public bool IsNewTimeRecord()
    {
        if (timeRecord == -1) timeRecord = GameController.Load().timeRecord;
        bool isNew = timeRecord < this.timeElapsed;

        if (isNew)
        {
            timeRecord = this.timeElapsed;
            GameController.SaveTimeRecord(timeRecord);
        }

        return isNew;
    }

    // Agregar al contador de superpoderes comprados por el jugador
    public void AddPurchaseSuperPower(SuperPowers superPower, int howMany)
    {
        switch (superPower)
        {
            case SuperPowers.TimeMaster:
                playerTimeMaster += howMany;
                break;
            case SuperPowers.MagicWand:
                playerMagicWand += howMany;
                break;
            case SuperPowers.JanKenUp:
                playerJanKenUP += howMany;
                break;
        }
    }

    // Obtener la cantidad de poderes TimeMaster disponible
    new public int GetTimeMaster()
    {
        return timeMaster + playerTimeMaster;
    }

    // Revisar la posibilidad de gastar un super poder TimeMaster
    new public bool SpendTimeMaster()
    {
        // Primero ver si es posible gastar los que compro el usuario
        if(playerTimeMaster > 0)
        {
            playerTimeMaster--;
            GameController.ChangeSuperPower(SuperPowers.TimeMaster, -1);
            return true;
        }

        return base.SpendTimeMaster();
    }

    // Obtener la cantidad de poderes MagicWand disponible
    new public int GetMagicWand()
    {
        return magicWand + playerMagicWand;
    }

    // Revisar la posibilidad de gastar un super poder MagicWand
    new public bool SpendMagicWand()
    {
        // Primero ver si es posible gastar los que compro el usuario
        if (playerMagicWand > 0)
        {
            playerMagicWand--;
            GameController.ChangeSuperPower(SuperPowers.MagicWand, -1);
            return true;
        }

        return base.SpendMagicWand();
    }

    // Obtener la cantidad de poderes MegaPunch disponible
    new public int GetJanKenUp()
    {
        return janKenUp + playerJanKenUP;
    }

    // Revisar la posibilidad de gastar un super poder MegaPunch (Cambiar a algo mas bacan, en chileno, Charchazo)
    new public bool SpendJanKenUp()
    {
        // Primero ver si es posible gastar los que compro el usuario
        if (playerJanKenUP > 0)
        {
            playerJanKenUP--;
            GameController.ChangeSuperPower(SuperPowers.JanKenUp, -1);
            return true;
        }

        return base.SpendJanKenUp();
    }

    // Agregar al contador del super JanKenUP!
    public void IncreaseSuperJanKenUPCounter()
    {
        superPowerUpCounter++;
    }

    // Obtener al contador del super JanKenUP!
    public int GetSuperPowerUPCounter()
    {
        return superPowerUpCounter;
    }

    // Resetear el contador del super JanKenUP!
    public void ResetSuperJanKenUPCounter()
    {
        superPowerUpCounter = 0;
    }

    // Activar modo clasico
    public void ActivateClassicMode(int lives = 3)
    {
        classicMode = true;
        survivalMode = false;
        duelMode = false;
        SetPlayersLives(lives);
        SetMaxLives(lives);
        SetPlayersPowers();
        SetPlayersStatistics();
    }

    public bool IsInClassicMode()
    {
        return classicMode;
    }

    // Activar modo survival
    public void ActivateSurvivalMode(int lives = 1)
    {
        classicMode = false;
        survivalMode = true;
        duelMode = false;
        SetPlayersLives(lives);
        SetMaxLives(lives);
        SetPlayersPowers();
        SetPlayersStatistics();
    }

    public bool IsInSurvivalMode()
    {
        return survivalMode;
    }

    /// <summary>
    /// Activar modo duelo
    /// </summary>
    public void ActivateDuelMode(int lives = 1)
    {
        classicMode = false;
        survivalMode = false;
        duelMode = true;
        SetPlayersLives(lives);
        SetMaxLives(lives);
        SetPlayersPowers();
        SetPlayersStatistics();
    }

    /// <summary>
    /// Indicacion de si esta o no en modo duelo
    /// </summary>
    /// <returns></returns>
    public bool IsInDuelMode()
    {
        return duelMode;
    }

    // Guardar el identificador del ultimo CPU utilizado
    public void SetLastCPUIdentifier(string identifier)
    {
        lastCPUIdentifier = identifier;
    }

    // Obtener el identificador del ultimo CPU utilizado
    public string GetLastCPUIdentifier()
    {
        string cpu = lastCPUIdentifier;
        lastCPUIdentifier = null;
        return cpu;
    }

    // Incrementar la ronda actual
    public void IncreaseCurrentRound()
    {
        currentRound++;
    }

    // Obtener la ronda actual
    public int GetCurrentRound()
    {
        return currentRound;
    }

    // Indicar cual es la velocidad de dificultad actual
    public void SetCurrentDifficultySpeed(float speed)
    {
        currentDifficulytSpeed = speed;
    }

    // Obtener la velocidad de dificultad actual
    public float GetCurrentDifficultySpeed()
    {
        return currentDifficulytSpeed;
    }

    // Indicar cual es la velocidad de dificultad pasada
    public void SetLastDifficultySpeed(float speed)
    {
        lastDifficulytSpeed = speed;
    }

    // Obtener la velocidad de dificultad pasada
    public float GetLastDifficultySpeed()
    {
        return lastDifficulytSpeed;
    }

    // Indicar cual es el nivel de dificultad actual
    public void SetDifficultyLevel(int level)
    {
        difficulytLevel = level;
    }

    // Obtener el nivel de dificultad actual
    public int GetDifficultyLevel()
    {
        return difficulytLevel;
    }

    // Indicar cual es el nivel de dificultad pasada
    public void SetLasDifficultyLevel(int level)
    {
        lastDifficulytLevel = level;
    }

    // Obtener el nivel de dificultad pasada
    public int GetLastDifficultyLevel()
    {
        return lastDifficulytLevel;
    }

    // Indicar que el ads fue visto. Tambien sirve para resetear
    public void SetAdsWasWatched(bool complete)
    {
        adsWatched = complete;
    }

    public bool IsAdsWasWatched()
    {
        return adsWatched;
    }

    // Obtener los achievementes listos
    public List<string> GetReadyAchievements()
    {
        return readyAchievements;
    }

    /// <summary>
    /// Indica si se esta mostrando o no un resultado de un duelo 
    /// </summary>
    /// <returns></returns>
    public bool GetIsShowingResults()
    {
        return isShowingResults;
    }

    /// <summary>
    /// Indicar si se esta o no mostrando los resultados de un duelo
    /// </summary>
    /// <param name="showing"></param>
    public void SetIsShowingResults(bool showing)
    {
        isShowingResults = showing;
    }

    /// <summary>
    /// Agrega un jugador al ranking
    /// </summary>
    public void AddPlayerToRanking(int playerIndex) {
        if (!playersRanking.Contains(playerIndex)) playersRanking.Add(playerIndex);
    }

    /// <summary>
    /// Obtencion del ranking de los jugadores en orden correcto
    /// </summary>
    /// <returns></returns>
    public List<int> GetPlayersRanking()
    {
        List<int> list = new List<int>(playersRanking);
        list.Reverse();
        return list;
    }
}