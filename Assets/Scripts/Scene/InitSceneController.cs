using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Threading.Tasks;

public class InitSceneController : MonoBehaviour
{
    [Header("Addresable")]
    [SerializeField] string fontManagerAddresable;
    [SerializeField] List<string> addresableAssets;
    [SerializeField] Text addresablePercentage;

    [Header("Objects")]
    [SerializeField] GameObject languageOverlay;

    [Header("TutorialCharacters")]
    [SerializeField] GameObject[] tutorialCharacters;

    TutoriaslCompleted tutorialsCompleted;

    // Flag para indicar la primera llamada a Update
    bool languageIsSelected = false;

    // Indicador de que la escena ya fue cargada una vez y solo se debe limitar a mostrar el inicio
    private static bool InitializationComplete = false;
    public static bool everythingIsLoaded = false;

    // Utiles para porcentajes de carga
    private int addressablesReady = 0;
    private int addressableItemPercent = 0;

    void Awake()
    {
        // Para version PC
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Iniciar el controlador de compras
        GameController.SetIAPController(FindObjectOfType<IAPController>());

        /*
         * Nota: Se realizara el proceso de traspaso de tutorial de las preferencias de usuario al archivo de guardado. Deberia realizarse solo una vez por usuario que tiene las versiones anteriores
         * Al momento de hacer esta nota, los niveles tutoriales llegaban solo hasta nivel 10, por eso las variables del for
         */
        tutorialsCompleted = GameController.LoadTutorials();
        if (!tutorialsCompleted.tutorialSync)
        {
            for (int i = 0; i < 11; i++)
            {
                if (PlayerPrefs.GetInt(string.Format("TutorialLevel_{0}", i), 0) == 1) tutorialsCompleted.tutorials.Add(i);
            }
            GameController.SaveTutorial(tutorialsCompleted.tutorials);
        }

        // Ver el idioma del jugadors
        if (!PlayersPreferencesController.PlayerSetCurrentLanguage())
        {
            // Mostrar el selector de idiomas y una vez seleccionado, proseguir con el juego
            ForceLanguageSelection();
        }
        else
        {
            languageIsSelected = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (languageIsSelected)
        {
            languageIsSelected = false;
            StartCoroutine(Derive());
        }
    }

    // Derivar a la escena que corresponda
    private IEnumerator Derive()
    {

        // Esperar hasta que este cargado lo de las localizaciones
        yield return LocalizationSettings.InitializationOperation;

        // Esperar la carga de los recursos
        yield return LoadAssets();

        SceneLoaderManager sceneLoaderManager = FindObjectOfType<SceneLoaderManager>();

        // Si existe un deeplink, analizar
        //if (!String.IsNullOrEmpty(Application.absoluteURL)) DeepLinkManager.Instance.onDeepLinkActivated(Application.absoluteURL);
        string nextScene = !String.IsNullOrEmpty(Application.absoluteURL) ? DeepLinkManager.Instance.GetNextScene() : "";

        // Si existe una escena extra que debe ser explorada
        if (nextScene != "")
        {
            sceneLoaderManager.GoTo(nextScene);
        }
        // Si el jugador ya ha jugado el primer tutorial
        else if (tutorialsCompleted.tutorials.Contains(0))
        {
            // Destruir la sesion single mode creada
            SingleModeSession sms = FindObjectOfType<SingleModeSession>();
            if (sms) Destroy(sms.gameObject);

            // Ir al menu
            sceneLoaderManager.MainScreen();
        }
        else
        {
            // Seleccionar personaje al azar para el tutorial
            GameObject tutorialCharacter = tutorialCharacters[UnityEngine.Random.Range(0, tutorialCharacters.Length)];
            SingleModeSession sms = FindObjectOfType<SingleModeSession>();
            if (sms) sms.SetPlayer(tutorialCharacter);

            // Pasar directamente al juego
            sceneLoaderManager.SingleMode();
        }
    }

    // Mostrar la pantalla de seleccion de idiomas
    private void ForceLanguageSelection()
    {
        Instantiate(languageOverlay);
    }

    // Funcion llamada al seleccionar el language
    public void SetLanguageIsSelected()
    {
        languageIsSelected = true;
    }

    /// <summary>
    /// Carga de assets necesarios para el juego
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadAssets()
    {
        // Realizar la carga del FontManager
        //yield return LoadAsset(fontManagerAddresable);
        // Realizar la carga de los demas elementos
        int index = 1;
        int total = addresableAssets.Count;

        // Indicar el % por addresable
        addressableItemPercent = 100 / (addresableAssets.Count > 0 ? addresableAssets.Count : 1);

        foreach (string key in addresableAssets)
        {
            var data = new[] { new { Value = string.Format("({0}/{1})", index++, total) } };
            yield return LoadAsset(key);
            addressablesReady++;
        }
        everythingIsLoaded = true;
    }

    /// <summary>
    /// Carga de un asset especifico
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private IEnumerator LoadAsset(string key)
    {
        AsyncOperationHandle<GameObject> opHandle;
        opHandle = Addressables.LoadAssetAsync<GameObject>(key);

        // FIX
        yield return null;

        while (!opHandle.IsDone)
        {
            UpdatePercentageText(opHandle.PercentComplete);
            yield return null;
        }

        if (opHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var op = Addressables.InstantiateAsync(key);
            if (op.IsDone)
            {
                Addressables.Release(opHandle);
            }
        }
    }

    /// <summary>
    /// Actualizacion del porcentaje de carga
    /// </summary>
    /// <param name="percentage"></param>
    private void UpdatePercentageText(float percentage)
    {
        int percent = (int) (addressablesReady * addressableItemPercent);
        percent += (int) Math.Floor(percentage * addressableItemPercent);
        addresablePercentage.text = $"{percent}%";
    }
}
