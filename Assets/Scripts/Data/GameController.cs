using JankenUp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController
{
    static string SAVENAME = "/jkup.save";
    static string TUTORIALNAME = "/jkuptut.save";
    static string deluxeID = "com.humita.jankenup.deluxe";
    public static IAPController iapController;
    private static bool isDeluxe = false;
    private static bool simulation = false;
    private static bool gameplayActive = false;

    // Packs comprados 
    private static List<JankenUp.Deluxe.Pack> playerPacks = new List<JankenUp.Deluxe.Pack>();

    // Identificadores de personajes que el jugador ha comprado
    private static List<string> charactersInPlayerAccount = new List<string>();

    // Identificadores de personajes de publico que el jugador ha comprado
    private static List<string> crowdsInPlayerAccount = new List<string>();

    // Guardar la data del jugador
    // TODO: Mejorar este método, hacer más seguro y mejor diseñado
    static public void Save(Save save)
    {
        // Guardar en disco
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + SAVENAME);
        bf.Serialize(file, save);
        file.Close();
    }

    // Guardar las monedas del jugador
    static public void Save(int coins)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.coins = coins;
        Save(save);
    }

    // Agregar monedas a la data del usuario
    static public void AddCoins(int coins)
    {
        // Cargar el archivo actual y agregar las monedas extras
        Save save = Load();
        save.coins += coins;
        Save(save);
    }

    // Guardar solo el nombre del jugador
    static public void Save(string name)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.name = name;
        Save(save);
    }

    // Guardar el indice del personaje seleccionado
    static public void SaveCharacterIndex(int characterIndex)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.characterIndex = characterIndex;
        Save(save);
    }

    /// <summary>
    /// Guarda el identificador del ultimo personaje usado
    /// </summary>
    /// <param name="characterIdentifier"></param>
    static public void SaveCharacterIdentifier(string characterIdentifier)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.characterIdentifier = characterIdentifier;
        Save(save);

        // Guardar tambien en el characterPool
        CharacterPool.Instance.SetCurrentCharacterIdentifier(characterIdentifier);
    }

    // Desbloqueo de los personajes 
    static public void SaveUnlockedCharacter(List<string> unlocked) {
        // Cargar el archivo actual
        Save save = Load();
        save.unlockedCharacters = unlocked;
        Save(save);
        if(CharacterPool.Instance) CharacterPool.Instance.AddSecretCharacter();
    }

    // Guardado del nivel de dificultad
    static public void SaveDifficultyLevel(int level)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.difficultyLevel = level;
        Save(save);
    }

    // Guardado del nivel de dificultad del modo survival (Frenzy)
    static public void SaveDifficultySurvival(int level)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.survivalLevel = level;
        Save(save);
    }

    // Guardado del record de nivel
    static public void SaveLevelRecord(int level)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.levelRecord = level;
        Save(save);
    }

    // Guardado del record de combos
    static public void SaveCombosRecord(int combos)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.comboRecord = combos;
        Save(save);
    }

    // Guardado del record de puntaje
    static public void SaveScoreRecord(int score)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.scoreRecord = score;
        Save(save);
    }

    // Guardado del record de timepo
    static public void SaveTimeRecord(float time)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.timeRecord = time;
        Save(save);
    }

    // Obtención de la data guardada del jugador
    static public Save Load()
    {
        if (File.Exists(Application.persistentDataPath + SAVENAME))
        {
            // Lectura de archivo
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + SAVENAME, FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();
            return save;
        }
        else
        {
            return new Save();
        }
    }

    // Revisar si el usuario tiene Deluxe o no
    public static bool IsDeluxe()
    {
        return isDeluxe;
    }

    /// <summary>
    /// Indicar que el jugador es deluxe y agregar el pack comprado
    /// </summary>
    /// <param name="pack"></param>
    public static void SetIsDeluxe()
    {
        isDeluxe = true;

        // Habilitar todos los personajes del juego (Util si es que se borro la data del juego)
        UnlockedCharacterController.UnlockDeluxe();

        // Marcar los overlays
        SetSingleModeDeluxeOverlayReady();

        // Logro de GooglePlay
        if (GooglePlayGamesController._this) GooglePlayGamesController._this.ReportAchievement(JankenUp.Achievements.deluxe, 100f);
    }

    // Crear el controlador de compras de la aplicacion
    public static void SetIAPController(IAPController controller)
    {
        iapController = controller;
    }

    // Si ya se mostro el deluxe overlay en el single mode, no continuar
    public static bool SingleModeDeluxeOverlayReady()
    {
        // Cargar el archivo actual
        Save save = Load();
        return save.deluxeOverlaySinglePlayer;
    }

    // Indicar que ya se mostro el deluxe sobre el single player
    public static void SetSingleModeDeluxeOverlayReady()
    {
        // Cargar el archivo actual
        Save save = Load();
        save.deluxeOverlaySinglePlayer = true;
        Save(save);
    }

    // Si ya se mostro el deluxe overlay en el JanKenShop!, no continuar
    public static bool ShopDeluxeOverlayReady()
    {
        // Cargar el archivo actual
        Save save = Load();
        return save.deluxeOverlayShop;
    }

    // Indicar que ya se mostro el deluxe sobre el JanKenShop!
    public static void SetShopDeluxeOverlayReady()
    {
        // Cargar el archivo actual
        Save save = Load();
        save.deluxeOverlayShop = true;
        Save(save);
    }

    // Si ya se mostro el deluxe overlay en el JanKenShop! Characters, no continuar
    public static bool ShopCharacterDeluxeOverlayReady()
    {
        // Cargar el archivo actual
        Save save = Load();
        return save.deluxeOverlayShopCharacter;
    }

    // Indicar que ya se mostro el deluxe sobre el JanKenShop! Characters
    public static void SetShopCharacterDeluxeOverlayReady()
    {
        // Cargar el archivo actual
        Save save = Load();
        save.deluxeOverlayShopCharacter = true;
        Save(save);
    }

    // Agregar a los superpoderes comprados por el usuario
    static public void ChangeSuperPower(SuperPowers superPower, int howMany) 
    {
        // Cargar el archivo actual y agregar el especial
        Save save = Load();
        switch (superPower)
        {
            case SuperPowers.TimeMaster:
                save.timeMaster = ( save.timeMaster + howMany < JankenUp.Shop.limitPerItem? (save.timeMaster + howMany) : JankenUp.Shop.limitPerItem  );
                break;
            case SuperPowers.MagicWand:
                save.magicWand = (save.magicWand + howMany < JankenUp.Shop.limitPerItem ? (save.magicWand + howMany) : JankenUp.Shop.limitPerItem);
                break;
            case SuperPowers.JanKenUp:
                save.superJanKenUp = (save.superJanKenUp + howMany < JankenUp.Shop.limitPerItem ? (save.superJanKenUp + howMany) : JankenUp.Shop.limitPerItem);
                break;
        }
        Save(save);
    }

    // Obtencion del valor guardado de los superpoderes
    static public int GetSuperPower(SuperPowers superPower)
    {
        // Cargar el archivo actual
        Save save = Load();
        int howMany = 0;
        switch (superPower)
        {
            case SuperPowers.TimeMaster:
                howMany = save.timeMaster;
                break;
            case SuperPowers.MagicWand:
                howMany = save.magicWand;
                break;
            case SuperPowers.JanKenUp:
                howMany = save.superJanKenUp;
                break;
        }
        return howMany;
    }

    // Si ya se mostro el prompt de review del juego
    public static bool ReviewPromptWasDisplayed()
    {
        // Cargar el archivo actual
        Save save = Load();
        return save.reviewPromptDisplayed;
    }

    // Indicar que ya se mostro el prompt de review del juego
    public static void SetReviewPromptWasDisplayed()
    {
        // Cargar el archivo actual
        Save save = Load();
        save.reviewPromptDisplayed = true;
        Save(save);
    }

    // Obtener las sesiones de juego
    public static int GetPlaySessions()
    {
        // Cargar el archivo actual
        Save save = Load();
        return save.playSessions;
    }

    // Aumentar en 1 las sesiones de juego
    public static void IncreasePlaySessions()
    {
        // Cargar el archivo actual
        Save save = Load();
        save.playSessions++;
        Save(save);
    }

    // Indicar que ya se mostro el mensaje de reestructuracion de modo publico
    public static void SetPublicModeRestructured()
    {
        // Cargar el archivo actual
        Save save = Load();
        save.competitiveModeRestructuredMessage = true;
        Save(save);
    }

    /// <summary>
    /// Obtener si es primera carga de juego
    /// </summary>
    /// <returns></returns>
    public static bool GetFirstLoad()
    {
        // Cargar el archivo actual
        Save save = Load();
        return save.firstLoad;
    }

    /// <summary>
    /// Guardar cambio de primera carga
    /// </summary>
    public static void UpdateFirstLoad()
    {
        // Cargar el archivo actual
        Save save = Load();
        save.firstLoad = false;
        Save(save);
    }

    /// <summary>
    /// Obtener el ultimo modo de juego
    /// </summary>
    /// <returns></returns>
    public static GameMode GetLastGameMode()
    {
        // Cargar el archivo actual
        Save save = Load();
        return save.lastGameMode;
    }

    /// <summary>
    /// Guardar el ultimo modo de juego seleccionado
    /// </summary>
    public static void SetLastGameMode(GameMode gameMode)
    {
        // Cargar el archivo actual
        Save save = Load();
        save.lastGameMode = gameMode;
        Save(save);
    }

    /* Tutoriales */

    // Tutoriales de single player
    static public void SaveTutorial(List<int> tutorial)
    {
        // Cargar el archivo actual
        TutoriaslCompleted tutorials = LoadTutorials();
        tutorials.tutorialSync = true;
        tutorials.tutorials = tutorial;

        // Guardar en disco
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + TUTORIALNAME);
        bf.Serialize(file, tutorials);
        file.Close();

    }

    // Obtención de la data guardada del jugador
    static public TutoriaslCompleted LoadTutorials()
    {
        if (File.Exists(Application.persistentDataPath + TUTORIALNAME))
        {
            // Lectura de archivo
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + TUTORIALNAME, FileMode.Open);
            TutoriaslCompleted saved = (TutoriaslCompleted)bf.Deserialize(file);
            file.Close();
            return saved;
        }
        else
        {
            return new TutoriaslCompleted();
        }
    }

    /// <summary>
    /// Agregar un pack a los packs comprados por el jugador
    /// </summary>
    /// <param name="pack">Pack a agregar</param>
    static public void AddPlayerPack(JankenUp.Deluxe.Pack pack)
    {
        playerPacks.Add(pack);

        // Agregar los elementos que contiene el pack
        charactersInPlayerAccount.AddRange(pack.characters);
        crowdsInPlayerAccount.AddRange(pack.crowd);
    }

    /// <summary>
    /// Obtencion de los packs comprados por el jugador
    /// </summary>
    /// <returns>Listado con todos los packs que posee el jugador</returns>
    static public List<JankenUp.Deluxe.Pack> GetPlayerPacks()
    {
        return playerPacks;
    }

    /// <summary>
    /// Obtencion de si el jugador tiene un pack
    /// </summary>
    /// <param name="packID"></param>
    /// <returns></returns>
    static public bool PlayerHasThisPack(string packID)
    {
        return playerPacks.Find(pack => pack.productID == packID) != null;
    }

    /// <summary>
    /// Revision de si un personaje se encuentra en el listado de personajes del jugador
    /// </summary>
    /// <param name="characterIdentifier"></param>
    /// <returns></returns>
    static public bool IsCharacterInPlayerAccount(string characterIdentifier)
    {
        return charactersInPlayerAccount.Contains(characterIdentifier);
    }

    /// <summary>
    /// Obtencion de los personajes de publico en la cuenta del jugador
    /// </summary>
    /// <returns></returns>
    static public List<string> GetCrowds()
    {
        return crowdsInPlayerAccount;
    }

    /// <summary>
    /// Cambia el estado del tutorial
    /// </summary>
    /// <param name="active"></param>
    static public void ToggleTutorial(bool active = false)
    {
        TutoriaslCompleted tutorials = new TutoriaslCompleted();
        if (!active)
        {
            for (int i = 0; i < 11; i++)
            {
                tutorials.tutorials.Add(i);
            }
        }
        GameController.SaveTutorial(tutorials.tutorials);
    }

    /// <summary>
    /// Establecer el valor de la simulacion
    /// </summary>
    /// <param name="value"></param>
    static public void SetSimulation(bool value = true)
    {
        simulation = value;
    }

    /// <summary>
    /// Obtener el estado de simulacion del juego
    /// </summary>
    /// <returns></returns>
    static public bool GetSimulation()
    {
        return simulation;
    }

    /// <summary>
    /// Establecer el valor de partida activa
    /// </summary>
    /// <param name="value"></param>
    static public void SetGameplayActive(bool value = true)
    {
        gameplayActive = value;
    }

    /// <summary>
    /// Obtener el estado de partida
    /// </summary>
    /// <returns></returns>
    static public bool GetGameplayActive()
    {
        return gameplayActive;
    }

    /// <summary>
    /// Simulacion de click sobre boton
    /// </summary>
    /// <param name="button"></param>
    public static void SimulateClick(Button button)
    {
        ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }

}
