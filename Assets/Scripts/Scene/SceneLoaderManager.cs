using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderManager : MonoBehaviour
{
    public static SceneLoaderManager _instance;
    public static SceneLoaderManager Instance { get { return _instance; } }

    TransitionDoors transitionDoors;
    static bool transitionInProgress = false;
    static string currentScene = "";
    static string previousScene = "";

    // Singleton
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }
    }   

    public enum SceneNamesEnum{
        MainScreen,
        SingleModeSelection,
        GameModeSelection,
        SingleMode,
        SingleModeRoundComplete,
        SingleModeResults,
        SurvivalMode,
        SurvivalModeResults,
        UnlockedCharacter,
        UnlockedDeluxeCharacter,
        Options,
        Credits,
        JanKenShop,
        DuelMode,
        DuelModeResults,
        DuelModeSelection
    }

    // Nombre de las scenes
    public static class SceneNames
    {
        public const string MainScreen = "MainScreen";
        public const string SingleModeSelection = "SingleModeSelection";
        public const string GameModeSelection = "GameModeSelection";
        public const string SingleMode = "SingleMode";
        public const string SingleModeRoundComplete = "SingleModeRoundComplete";
        public const string SingleModeResults = "SingleModeResults";
        public const string SurvivalMode = "SurvivalMode";
        public const string SurvivalModeResults = "SurvivalModeResults";
        public const string UnlockedCharacter = "UnlockedCharacter";
        public const string UnlockedDeluxeCharacter = "UnlockedDeluxeCharacter";
        public const string Options = "Options";
        public const string Credits = "Credits";
        public const string JanKenShop = "JanKenShop";
        public const string DuelMode = "DuelMode";
        public const string DuelModeResults = "DuelModeResults";
        public const string DuelModeSelection = "DuelModeSelection";
    } 

    // Vuelve a la última escena (Si es que hay). Si no, cierra el juego
    public void Back() {

        switch (previousScene)
        {
            case SceneNames.MainScreen:
                MainScreen();
                break;
            case SceneNames.SingleModeSelection:
                SingleModeSelection();
                break;
            case SceneNames.GameModeSelection:
                GameModeSelection();
                break;
            case SceneNames.SingleMode:
                SingleMode();
                break;
            case SceneNames.SingleModeRoundComplete:
                SingleModeRoundComplete();
                break;
            case SceneNames.SingleModeResults:
                SingleModeResults();
                break;
            case SceneNames.SurvivalMode:
                SurvivalMode();
                break;
            case SceneNames.SurvivalModeResults:
                SurvivalModeResults();
                break;
            case SceneNames.DuelModeSelection:
                DuelModeSelection();
                break;
            default:
                //Application.Quit();
                break;
        }

    }

    // Ir a una escena por el nombre
    public void GoTo(string scene)
    {
        switch (scene)
        {
            case SceneNames.MainScreen:
                MainScreen();
                break;
            case SceneNames.SingleModeSelection:
                SingleModeSelection();
                break;
            case SceneNames.GameModeSelection:
                GameModeSelection();
                break;
            case SceneNames.SingleMode:
                SingleMode();
                break;
            case SceneNames.SingleModeRoundComplete:
                SingleModeRoundComplete();
                break;
            case SceneNames.SingleModeResults:
                SingleModeResults();
                break;
            case SceneNames.SurvivalMode:
                SurvivalMode();
                break;
            case SceneNames.SurvivalModeResults:
                SurvivalModeResults();
                break;
            case SceneNames.DuelMode:
                DuelMode();
                break;
            case SceneNames.DuelModeSelection:
                DuelModeSelection();
                break;
            case SceneNames.Options:
                Options(currentScene);
                break;
        }
    }

    // Menu se encarga de llevar a la pantalla de menú
    public void MainScreen() {

        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene("");
            StartCoroutine(LoadAsync(SceneNames.MainScreen));
        }
    }

    // SingleModeSelection se encarga de llevar a la pantalla de selección
    // de personaje en single player
    public void SingleModeSelection()
    {
        if (transitionInProgress) return;

        // Si existen desbloqueos tipo deluxe, mostrar antes de ir a la seleccion
        if (UnlockedDeluxeController.AreThereNewCharacters())
        {
            UnlockedDeluxeCharacter(SceneNames.SingleModeSelection);
        }
        else
        {
            transitionDoors = FindObjectOfType<TransitionDoors>();
            if (transitionDoors)
            {
                transitionInProgress = true;

                // Si existe una sesión activa, destruir
                JankenSession session = FindObjectOfType<JankenSession>();
                if (session) Destroy(session.gameObject);

                SavePreviousScene(SceneNames.GameModeSelection);
                StartCoroutine(LoadAsync(SceneNames.SingleModeSelection));
            }
        }
    }

    // SingleModeSelectionType se encarga de llevar a la pantalla de selección de tipo single player
    public void GameModeSelection()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;

            // Si existe una sesión activa, resetear
            SingleModeSession session = FindObjectOfType<SingleModeSession>();
            if (session) session.Reset();

            SavePreviousScene(SceneNames.MainScreen);
            StartCoroutine(LoadAsync(SceneNames.GameModeSelection));
        }
    }

    // SingleMode se encarga de llevar a la pantalla de juego en single player
    public void SingleMode()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(SceneNames.SingleModeSelection);
            StartCoroutine(LoadAsync(SceneNames.SingleMode));
        }
    }

    // SingleModeRoundComplete se encarga de llevar a la pantalla de resultados de ronda en modo clasico
    public void SingleModeRoundComplete()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(SceneNames.SingleMode);
            StartCoroutine(LoadAsync(SceneNames.SingleModeRoundComplete));
        }
    }

    // SingleMode se encarga de llevar a la pantalla de resultados en single player
    public void SingleModeResults()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(SceneNames.SingleModeSelection);
            StartCoroutine(LoadAsync(SceneNames.SingleModeResults));
        }
    }

    // SurvivalMode se encarga de llevar a la pantalla de juego en modo supervivencia
    public void SurvivalMode()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(SceneNames.SingleModeSelection);
            StartCoroutine(LoadAsync(SceneNames.SurvivalMode));
        }
    }

    // SurvivalMode se encarga de llevar a la pantalla de resultados en modo supervivencia
    public void SurvivalModeResults()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(SceneNames.SingleModeSelection);
            StartCoroutine(LoadAsync(SceneNames.SurvivalModeResults));
        }
    }

    // Pantalla de desbloqueo de personajes
    public void UnlockedCharacter(string nextScene) {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(nextScene);
            StartCoroutine(LoadAsync(SceneNames.UnlockedCharacter));
        }
    }

    // Pantalla de desbloqueo de personajes tipo deluxe
    public void UnlockedDeluxeCharacter(string nextScene)
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(nextScene);
            StartCoroutine(LoadAsync(SceneNames.UnlockedDeluxeCharacter));
        }
    }

    // Pantalla de configuraciones
    public void Options(string previousScene)
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(previousScene);
            StartCoroutine(LoadAsync(SceneNames.Options));
        }
    }

    // Método especial para menú
    public void OptionsFromMenu() {
        Options(SceneNames.MainScreen);
    }

    // Mostrar los creditos del juego
    public void Credits()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(currentScene);
            StartCoroutine(LoadAsync(SceneNames.Credits));
        }
    }

    // Tienda del juego
    public void JanKenShop(string previousScene)
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(previousScene);
            StartCoroutine(LoadAsync(SceneNames.JanKenShop));
        }
    }

    /// <summary>
    /// Duelos
    /// </summary>
    public void DuelMode()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(SceneNames.DuelModeSelection);
            StartCoroutine(LoadAsync(SceneNames.DuelMode));
        }
    }

    /// <summary>
    /// Resultado duelos
    /// </summary>
    public void DuelModeResults()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(SceneNames.DuelModeSelection);
            StartCoroutine(LoadAsync(SceneNames.DuelModeResults));
        }
    }

    /// <summary>
    /// Seleccion de personajes para duelos
    /// </summary>
    public void DuelModeSelection()
    {
        if (transitionInProgress) return;

        transitionDoors = FindObjectOfType<TransitionDoors>();
        if (transitionDoors)
        {
            transitionInProgress = true;
            SavePreviousScene(SceneNames.GameModeSelection);
            StartCoroutine(LoadAsync(SceneNames.DuelModeSelection));
        }
    }

    /// <summary>
    /// Guardado del nombre de la scene actual
    /// </summary>
    /// <param name="name"></param>
    void SaveCurrentScene(string name)
    {
        currentScene = name;
    }

    /// <summary>
    /// Obtencion de la escena actual
    /// </summary>
    /// <returns></returns>
    public string GetCurrentScene()
    {
        return currentScene;
    }

    // Guardado del nombre de la scene anterior
    public void SavePreviousScene(string name)
    {
        previousScene = name;
    }

    // Carga asyncronica de escena
    private IEnumerator LoadAsync(string scene)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
        asyncLoad.allowSceneActivation = false;

        yield return StartCoroutine(transitionDoors.Close());

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Guardar la escena actual
        SaveCurrentScene(scene);

        asyncLoad.allowSceneActivation = true;

    }

    // Indicar que ya se puede volver a transicionar
    public static void TransitionReady() {
        transitionInProgress = false;
    }

    /// <summary>
    /// Realiza una llamada de transicion cuando se termina la transicion actual
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private IEnumerator CallWhenIsReady(string scene)
    {
        while (transitionInProgress) yield return null;
        GoTo(scene);
    }

}
