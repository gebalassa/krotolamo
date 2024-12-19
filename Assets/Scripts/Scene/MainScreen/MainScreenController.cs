using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
using DigitalRuby.Tween;
using System;

public class MainScreenController : SceneController
{
    [Header("Addresable")]
    [SerializeField] string fontManagerAddresable;
    [SerializeField] List<string> addresableAssets;

    [Header("Animation")]
    [SerializeField] LogoMusicBox logo;
    [SerializeField] GameObject whiteScreen;
    [SerializeField] [Range(0f, 1f)] float whiteScreenTime = 0.4f;
    [SerializeField] Color initialColor = new Color(1, 1, 1, 0);
    [SerializeField] [Range(0, 10)] float initializationOpenDoorWaitTime = .8f;
    [SerializeField] [Range(0, 10)] float speedAnimationIllustrations = 1f;
    [SerializeField] MainScreenIllustration[] mainScreenIllustrations;

    [Header("Feedback")]
    [SerializeField] TextMeshProUGUI feedbackText;
    [SerializeField] [Range(0, 10)] float feedbackTime = 1f;

    [Header("FX")]
    [SerializeField] GameObject confettiParticle;
    [SerializeField] GeneralParticle confettiLeft;
    [SerializeField] GeneralParticle confettiRight;
    [SerializeField] Vector3 confettiPosition;

    [Header("SFX")]
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioClip drumRoll;

    [Header("Objects")]
    [SerializeField] GameObject languageOverlay;
    [SerializeField] HumitaLogoAnimation humitaLogoAnimation;
    [SerializeField] Button nextSceneButton;
    [SerializeField] GameObject singleModeSession;
    [SerializeField] GameObject menuObject;

    [Header("Others")]
    [SerializeField] bool showMenu = true;
    [SerializeField] bool langJoystick = false;
    [SerializeField] bool simulation = false;
    [SerializeField] [Range(0, 120)] int timeToSimulation = 60;
    [SerializeField] bool adjustTargetFrameRate = false;

    // Indicador de que la escena ya fue cargada una vez y solo se debe limitar a mostrar el inicio
    private static bool InitializationComplete = false;
    public static bool everythingIsLoaded = false;
    private static bool whiteScreenComplete = false;

    // Listado de handles para carga de assets
    List<AsyncOperationHandle<GameObject>> opHandles;

    // Componentes y utiles
    AudioSource audioSource;
    TutoriaslCompleted tutorialsCompleted;
    GameObject languageOverlayInstance;

    // Tween
    Tween<Color> tweenFeedbackAppear;
    Tween<Color> tweenFeedbackTick;

    // Flags para Unity en Android
    private bool isCheckGooglePlayGames = false;

    // Coroutine
    Coroutine simulationCoroutine;

    // Use this for initialization
    new void Start()
    {
        if (adjustTargetFrameRate)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        nextSceneButton.onClick.AddListener(GoToNextScene);

        Setup();

        // Suscribirse a los eventos del logo
        LogoMusicBox.onLogoClickDelegate += OnLogoClick;

        if (!InitializationComplete)
        {
            StartCoroutine(Initialization());
            openDoor = false;
            InitializationComplete = true;
            autoplayMusic = false;
        }
        else
        {
            humitaLogoAnimation.Remove(true);
            base.Start();
            AlredyInitialized();
        }


        GameController.SetGameplayActive(false);
    }

    new void Update()
    {
        base.Update();

        #if UNITY_ANDROID
        if (isReady && !isCheckGooglePlayGames)
        {
            isCheckGooglePlayGames = true;
            GooglePlayGamesController._this.FirstTimeSetup();
        }
        #endif
    }

    /// <summary>
    /// Carga de la configuracion general, tutoriales, etc
    /// </summary>
    private void Setup()
    {
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
    }

    /// <summary>
    /// Inicializacion de elementos recurrentes para el juego
    /// </summary>
    /// <returns></returns>
    private IEnumerator Initialization()
    {
        // Vamos con la challa
        StartCoroutine(ShowTime());

        // Esperar la localizacion
        yield return WaitForLocalization();

        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;

        // Esperar la carga de los recursos
        yield return LoadAssets();

        // Suscribirse al soporte de controles
        JoystickSupport.onJoystick += OnJoystick;

        // Inicializar la espera para simulacion
        if(simulation) simulationCoroutine = StartCoroutine(Simulation());
    }

    /// <summary>
    /// Carga de assets necesarios para el juego
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadAssets()
    {
        // Realizar la carga del FontManager
        yield return LoadAsset(fontManagerAddresable);
        // Realizar la carga de los demas elementos
        int index = 1;
        int total = addresableAssets.Count;
        foreach (string key in addresableAssets)
        {
            var data = new[] { new { Value = string.Format("({0}/{1})", index++, total) } };
            SetFeedbackText(JankenUp.Localization.tables.Options.Keys.loadingResources, JankenUp.Localization.tables.Options.tableName, data);
            yield return LoadAsset(key);
        }
        everythingIsLoaded = true;

        // No mostrar nada hasta que este completa la animacion inicial
        SetFeedbackText();
        while (!whiteScreenComplete) yield return null;

        // Activar menu
        if (menuObject && showMenu) menuObject.SetActive(true);

        // Realizar configuraciones que necesitan que los elementos esten cargados
        yield return PostLoadAssets();

        // Indicar que ya se puede avanzar
        SetFeedbackText(langJoystick ? JankenUp.Localization.tables.Options.Keys.startToPlay : JankenUp.Localization.tables.Options.Keys.tapToPlay, JankenUp.Localization.tables.Options.tableName);
        StartCoroutine(InitFeedbackTick());
    }

    /// <summary>
    /// Carga de un asset especifico
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private IEnumerator LoadAsset(string key)
    {
        AsyncOperationHandle<GameObject> opHandle = Addressables.LoadAssetAsync<GameObject>(key);
        yield return opHandle;
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
    /// Acciones a realizar una vez los assets han sido cargados
    /// </summary>
    /// <returns></returns>
    private IEnumerator PostLoadAssets()
    {
        yield return LoadCurrentCharacterMusic();
    }

    /// <summary>
    /// Carga de la musica del personaje actualmente seleccionado
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadCurrentCharacterMusic()
    {
        string identifier = GameController.Load().characterIdentifier;

        // Si esta desbloqueado, continuar. De lo contrario, seleccionar al primero de la lista// Obtener los personajes
        List<GameObject> charactersAvailables = CharacterPool.Instance.GetAvailables();
        GameObject selectedCharacter = charactersAvailables.Find(c => c.GetComponent<CharacterConfiguration>().GetIdentifier() == identifier);
        CharacterConfiguration characterConfiguration = selectedCharacter != null ? selectedCharacter.GetComponent<CharacterConfiguration>() : charactersAvailables[0].GetComponent<CharacterConfiguration>();

        // Precargar la musica del personaje inicial
        if (characterConfiguration) MasterAudioPlayer._player.PlayOrLoadThis(characterConfiguration.GetCharacterMusic(), false);

        yield return null;
    }

    /// <summary>
    /// Esperar hasta que la localizacion este cargada
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;

        // Obtener el codigo actual
        UnityEngine.Localization.Locale locale = LocalizationSettings.AvailableLocales.GetLocale(PlayersPreferencesController.GetCurrentLanguage());
        if (locale != null) LocalizationSettings.SelectedLocale = locale;

    }

    /// <summary>
    /// Se solicita realizar el cambio del texto mostrado como feedback para el jugador
    /// </summary>
    /// <param name="key"></param>
    /// <param name="table"></param>
    private void SetFeedbackText(string key = "", string table = "", object[] data = null) {
        if( key != "" && table != "")
        {
            if (data != null)
            {
                LocalizationHelper.FormatTranslate(feedbackText, table, key, data);
            }
            else
            {
                LocalizationHelper.Translate(feedbackText, table, key);
            }
        }
        else
        {
            feedbackText.text = "";
        }
    }

    /// <summary>
    /// FadeIn inicial del texto. Esto deberia estar en un elemento separado
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitFeedback()
    {
        if (!everythingIsLoaded || tweenFeedbackTick == null)
        {
            System.Action<ITween<Color>> updateColor = (t) =>
            {
                if (feedbackText) feedbackText.color = t.CurrentValue;
            };

            tweenFeedbackAppear = feedbackText.gameObject.Tween(string.Format("FeedbackTextAppear{0}", feedbackText.GetInstanceID()), JankenUp.JankenColors.clearWhite, Color.white, feedbackTime, TweenScaleFunctions.QuadraticEaseInOut, updateColor);
            yield return new WaitForSeconds(feedbackTime);
        }
    }

    /// <summary>
    /// FadeInOut del texto. Esto deberia estar en un elemento separado
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitFeedbackTick()
    {
        if (tweenFeedbackAppear != null) tweenFeedbackAppear.Stop(TweenStopBehavior.DoNotModify);

        while (true)
        {
            bool tickComplete = false;
            
            System.Action<ITween<Color>> updateTick = (t) =>
            {
                feedbackText.color = t.CurrentValue;
            };

            System.Action<ITween<Color>> updateTickComplete = (t) =>
            {
                tickComplete = true;
            };

            tweenFeedbackTick = feedbackText.gameObject.Tween(string.Format("FeedbackTextFadeIn{0}", feedbackText.GetInstanceID()), feedbackText.color, Color.white, feedbackTime, TweenScaleFunctions.QuadraticEaseInOut, updateTick)
                .ContinueWith(new ColorTween().Setup(Color.white, JankenUp.JankenColors.clearWhite, feedbackTime, TweenScaleFunctions.QuadraticEaseOut, updateTick, updateTickComplete));

            while (!tickComplete) yield return null;
        }
    }

    /// <summary>
    /// Corutina encargada de realizar la presentacion del juego
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowTime()
    {
        // Reproducir el video de Humita
        humitaLogoAnimation.Play();
        while (humitaLogoAnimation.IsPlaying()) yield return null;

        yield return LocalizationSettings.InitializationOperation;

        // Ver el idioma del jugadors
        if (!PlayersPreferencesController.PlayerSetCurrentLanguage())
        {
            // Mostrar el selector de idiomas y una vez seleccionado, proseguir con el juego
            ForceLanguageSelection();
        }

        // Cerrar puertas
        yield return TransitionDoors._this.Close();
        while (languageOverlayInstance) yield return null;
        UpdateCurrentFont();

        // Remover el splash
        humitaLogoAnimation.Remove();

        if (Shake._this) Shake._this.ShakeIt();

        // Reproducir el redoble de tambores
        DrumRoll();

        yield return new WaitForSeconds(initializationOpenDoorWaitTime);

        // Indicar que puertas deben ser abiertas
        openDoor = true;

        while (!isReady) yield return null;
        StartCoroutine(JanKenUpWelcome());
    }

    #region Audio

    /// <summary>
    /// Redoble de tambores
    /// </summary>
    public void DrumRoll()
    {
        mixer.SetFloat("MusicVolume", PercentToDB(PlayerPrefs.GetFloat(MasterAudioPlayer.MUSIC_MASTER_VOLUME, MasterAudioPlayer.musicDefaultVolume)));
        mixer.SetFloat("SFXVolume", PercentToDB(PlayerPrefs.GetFloat(MasterAudioPlayer.SFX_MASTER_VOLUME, MasterAudioPlayer.sfxDefaultVolume)));
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = drumRoll;
        audioSource.Play();
    }

    /// <summary>
    /// Detiene la condicion de que no se pueda reproducir otra musica
    /// </summary>
    public void StopDrumRoll()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// Obtencion del valor en decibeles del sonido en base a su porcentaje
    /// </summary>
    /// <param name="percent"></param>
    /// <returns></returns>
    public float PercentToDB(float percent)
    {
        return 20 * Mathf.Log10(percent);
    }

    #endregion

    /// <summary>
    /// Corutina para animacion inicio
    /// </summary>
    /// <returns></returns>
    private IEnumerator JanKenUpWelcome()
    {
        // Animar el logo
        yield return StartCoroutine(logo.StartAnimation());

        // Animar la aparicion del whiteScreen
        whiteScreen.SetActive(true);
        Image whiteScreenImage = whiteScreen.GetComponent<Image>();
        System.Action<ITween<Color>> updatewhiteScreen = (t) =>
        {
            whiteScreenImage.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> completeHalfUpdatewhiteScreen = (t) =>
        {
            logo.ShowLogoExtras();
            AnimateIllustrations();
        };

        System.Action<ITween<Color>> completeUpdatewhiteScreen = (t) =>
        {
            whiteScreenComplete = true;
        };

        whiteScreen.gameObject.Tween(string.Format("FadeIn{0}", whiteScreen.GetInstanceID()), initialColor, Color.white, whiteScreenTime, TweenScaleFunctions.QuadraticEaseInOut, updatewhiteScreen, completeHalfUpdatewhiteScreen)
            .ContinueWith(new ColorTween().Setup(Color.white, initialColor, whiteScreenTime, TweenScaleFunctions.QuadraticEaseOut, updatewhiteScreen, completeUpdatewhiteScreen));

        yield return new WaitForSeconds(whiteScreenTime);

        // Challas
        //GeneralParticle confetti = Instantiate(confettiParticle).GetComponent<GeneralParticle>();
        //PartyHorn confettiSound = confetti.GetComponent<PartyHorn>();
        //confettiSound.SetSoundOn(true);
        //confetti.transform.position = confettiPosition;
        //confetti.Play();
        confettiLeft.gameObject.SetActive(true);
        confettiRight.gameObject.SetActive(true);
        confettiLeft.Play();
        confettiRight.Play();

        while (!whiteScreenComplete) yield return null;
        whiteScreen.SetActive(false);

        // Comenzar a reproducir la musica
        StopDrumRoll();
        while (!MasterAudioPlayer._player) yield return null;
        MasterAudioPlayer._player.MenuMusic();

        // Mostrar el feedback
        StartCoroutine(InitFeedback());
    }

    /// <summary>
    /// Metodo que indica a las ilustraciones que deben animarse
    /// </summary>
    /// <param name="noTransition"></param>
    private void AnimateIllustrations(bool noTransition = false) {
        foreach(MainScreenIllustration illustration in mainScreenIllustrations){
            illustration.Show(noTransition);
        }
    }

    /// <summary>
    /// Inicia la derivacion del jugador a la siguiente escena
    /// </summary>
    private void GoToNextScene()
    {
        if (!everythingIsLoaded) return;
        GameController.UpdateFirstLoad();

        // Si existe un deeplink, analizar
        if (!String.IsNullOrEmpty(Application.absoluteURL)) DeepLinkManager.Instance.onDeepLinkActivated(Application.absoluteURL);
        string nextScene = !String.IsNullOrEmpty(Application.absoluteURL) ? DeepLinkManager.Instance.GetNextScene() : "";

        // Si existe una escena extra que debe ser explorada
        if (nextScene != "")
        {
            SceneLoaderManager.Instance.GoTo(nextScene);
        }
        // Si el jugador ya ha jugado el primer tutorial
        else if (tutorialsCompleted.tutorials.Contains(0))
        {
            // Destruir la sesion single mode creada
            SingleModeSession sms = FindObjectOfType<SingleModeSession>();
            if (sms) Destroy(sms.gameObject);

            // Ir a seleccion de modo de juego
            SceneLoaderManager.Instance.GameModeSelection();
        }
        else
        {
            SingleModeSession sms = FindObjectOfType<SingleModeSession>();
            if (!sms)
            {
                sms = Instantiate(singleModeSession).GetComponent<SingleModeSession>();
                sms.SetNoInitialSetup();
            }
            GameObject tutorialCharacter = CharacterPool.Instance.Surprise();
            sms.SetPlayer(tutorialCharacter);
            // Pasar directamente al juego
            int randomScreen = UnityEngine.Random.Range(0, 2);
            if(randomScreen == 0) SceneLoaderManager.Instance.SingleMode();
            else SceneLoaderManager.Instance.SurvivalMode();
        }
    }

    /// <summary>
    /// Mostrar todos los elementos sin animacion
    /// </summary>
    private void AlredyInitialized()
    {
        logo.Show();
        whiteScreen.SetActive(false);
        AnimateIllustrations(true);
        // Activar menu
        if (menuObject && showMenu) menuObject.SetActive(true);

        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;

        // Indicar que ya se puede avanzar
        UpdateCurrentFont();
        SetFeedbackText(langJoystick? JankenUp.Localization.tables.Options.Keys.startToPlay : JankenUp.Localization.tables.Options.Keys.tapToPlay, JankenUp.Localization.tables.Options.tableName);
        StartCoroutine(InitFeedbackTick());

        // Inicializar la espera para simulacion
        if(simulation) simulationCoroutine = StartCoroutine(Simulation());
    }

    /// <summary>
    /// Llamado desde el evento de presionar una nota del logo
    /// </summary>
    private void OnLogoClick()
    {
        SetFeedbackText(langJoystick? JankenUp.Localization.tables.Options.Keys.startToPlayPlease : JankenUp.Localization.tables.Options.Keys.tapToPlayPlease, JankenUp.Localization.tables.Options.tableName);
    }

    /// <summary>
    /// Mostrar la pantalla de seleccion de idiomas
    /// </summary>
    private void ForceLanguageSelection()
    {
        languageOverlayInstance = Instantiate(languageOverlay);
    }

    /// <summary>
    /// Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    public void UpdateCurrentFont()
    {
        if (FontManager._mainManager && feedbackText)
        {
            TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
            feedbackText.font = mainFont;

            // Cambiar el estilo
            FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
            feedbackText.fontStyle = style;
        }
        
    }

    /// <summary>
    /// Llamado frente a cambios en el idioma seleccionado por el jugador. 
    /// </summary>
    protected override void Localize()
    {
        if (!everythingIsLoaded) return;
        UpdateCurrentFont();
        SetFeedbackText(langJoystick ? JankenUp.Localization.tables.Options.Keys.startToPlay : JankenUp.Localization.tables.Options.Keys.tapToPlay, JankenUp.Localization.tables.Options.tableName);
    }

    /// <summary>
    /// Espera de X tiempo para ir luego a la simulacion del modo duelo
    /// </summary>
    /// <returns></returns>
    private IEnumerator Simulation()
    {
        yield return new WaitForSeconds(timeToSimulation);
        GameController.SetSimulation();
        SceneLoaderManager.Instance.DuelMode();
    }

    /// <summary>
    /// Detener animaciones
    /// </summary>
    protected new void OnDestroy()
    {
        base.OnDestroy();
        if (tweenFeedbackAppear != null) tweenFeedbackAppear.Stop( TweenStopBehavior.DoNotModify);
        if (tweenFeedbackTick != null) tweenFeedbackTick.Stop( TweenStopBehavior.DoNotModify);
        if (simulationCoroutine != null) StopCoroutine(simulationCoroutine);

        // Desuscribirse a los eventos del logo
        LogoMusicBox.onLogoClickDelegate -= OnLogoClick;

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
        // Resetear la corutina de simulacion
        if (simulation && simulationCoroutine != null)
        {
            StopCoroutine(simulationCoroutine);
            simulationCoroutine = StartCoroutine(Simulation());
        }

        if (!base.OnJoystick(action, playerIndex)) return false;

        switch (action)
        {
            case JoystickAction.Start:
                GoToNextScene();
                break;
            case JoystickAction.X:
                logo.PlayKro();
                break;
            case JoystickAction.A:
                logo.PlayTo();
                break;
            case JoystickAction.B:
                logo.PlayLa();
                break;
            case JoystickAction.Y:
                logo.PlayMo();
                break;
            case JoystickAction.L:
            case JoystickAction.R:
                logo.PlayJapanese();
                break;
            default:
                OnLogoClick();
                break;
        }

        return true;
    }
    #endregion
}