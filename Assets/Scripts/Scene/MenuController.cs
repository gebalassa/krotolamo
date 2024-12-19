using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MenuController : SceneController
{
    // Primera vez que entra al menu debe esperar para abrir las puertas
    private static bool firstTime = true;
    private static bool firstTimeRunning = false;

    // Indicador para saber si se debe o no mostrar la animacion
    private static bool animationStart = false;

    [Header("Objects")]
    [SerializeField] GameObject buttonsContainer;
    [SerializeField] LogoMusicBox logo;
    [SerializeField] GameObject whiteScreen;
    [SerializeField] Image creditsButton;

    [Header("Animation")]
    [SerializeField] [Range(0f, 1f)] float whiteScreenTime = 0.4f;
    [SerializeField] float distance = 100f;
    [SerializeField] Color initialColor = new Color(1, 1, 1, 0);
    [SerializeField] [Range(0f, 1f)] float firstTimeMenuWaitTime = .8f;

    [Header("FX")]
    [SerializeField] GameObject confettiParticle;
    [SerializeField] Vector3 confettiPosition;

    [Header("SFX")]
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioClip drumRoll;

    // Componentes y utiles
    AudioSource audioSource;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        // Ver el idioma del jugadors
        CompleteStart();
    }

    // Completar el inicio del juego
    private void CompleteStart()
    {
        // Si la animacion ya fue ejecutada una vez, mostrar todos los componentes altiro
        if (animationStart)
        {
            Show();
        }

        // Cambiar el idioma segun preferencia de jugador
        StartCoroutine(WaitForLocalization());
    }

    // Esperar hasta que la localizacion este cargada
    private IEnumerator WaitForLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;

        // Obtener el codigo actual
        UnityEngine.Localization.Locale locale = LocalizationSettings.AvailableLocales.GetLocale(PlayersPreferencesController.GetCurrentLanguage());
        if (locale != null) LocalizationSettings.SelectedLocale = locale;

    }


    // Update is called once per frame
    new void Update()
    {

        if (firstTime)
        {
            if (!firstTimeRunning)
            {
                firstTimeRunning = true;
                autoplayMusic = false;
                StartCoroutine(FirstTimeMenu());
            }
        }
        else
        {
            base.Update();
        }

        if (isReady && !animationStart)
        {
            animationStart = true;
            StartCoroutine(JanKenUpWelcome());
        }

    }

    // Corutina encargada esperar para abrir las puertas la primera vez
    private IEnumerator FirstTimeMenu()
    {
        Shake shake = FindObjectOfType<Shake>();
        if (shake) shake.ShakeIt();

        // Reproducir el redoble de tambores
        DrumRoll();

        yield return new WaitForSeconds(firstTimeMenuWaitTime);

        firstTime = false;
    }

    // Corutina encargada de mostrar todos los elementos del menu
    private IEnumerator JanKenUpWelcome()
    {
        // Animar el logo
        yield return StartCoroutine(logo.StartAnimation());

        // Animar la aparicion del whiteScreen
        whiteScreen.SetActive(true);
        bool whiteScreenComplete = false;
        Image whiteScreenImage = whiteScreen.GetComponent<Image>();
        System.Action<ITween<Color>> updatewhiteScreen = (t) =>
        {
            whiteScreenImage.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> completeHalfUpdatewhiteScreen = (t) =>
        {
            logo.ShowLogoExtras();
        };

        System.Action<ITween<Color>> completeUpdatewhiteScreen = (t) =>
        {
            whiteScreenComplete = true;
        };

        whiteScreen.gameObject.Tween(string.Format("FadeIn{0}", whiteScreen.GetInstanceID()), initialColor, Color.white, whiteScreenTime, TweenScaleFunctions.QuadraticEaseInOut, updatewhiteScreen, completeHalfUpdatewhiteScreen)
            .ContinueWith(new ColorTween().Setup(Color.white, initialColor, whiteScreenTime, TweenScaleFunctions.QuadraticEaseOut, updatewhiteScreen, completeUpdatewhiteScreen));

        yield return new WaitForSeconds(whiteScreenTime);

        // Challas
        //if (GameController.IsDeluxe()){
            GeneralParticle confetti = Instantiate(confettiParticle).GetComponent<GeneralParticle>();
            PartyHorn confettiSound = confetti.GetComponent<PartyHorn>();
            confettiSound.SetSoundOn(false);
            confetti.transform.position = confettiPosition;
            confetti.Play();
        //}

        // Comenzar a reproducir la musica
        MasterAudioPlayer masterAudioPlayer = FindObjectOfType<MasterAudioPlayer>();
        StopDrumRoll();
        masterAudioPlayer.MenuMusic();

        // Animar cada boton por separado
        foreach (Transform child in buttonsContainer.transform)
        {
            GameObject target = child.gameObject;
            Image targetImage = target.GetComponent<Image>();
            targetImage.color = Color.white;
        }

        // Mostrar boton creditos
        creditsButton.color = Color.white;

        while (!whiteScreenComplete) yield return null;
        whiteScreen.SetActive(false);
    }

    // Mostrar todos los elementos sin animacion
    private void Show()
    {
        logo.Show();
        whiteScreen.SetActive(false);
        // Todos los botones
        foreach (Transform child in buttonsContainer.transform)
        {
            child.gameObject.GetComponent<Image>().color = Color.white;
        }

        creditsButton.color = Color.white;
    }

    // Reproducir sonido de boton
    public void UIButtonSFX()
    {
        MasterSFXPlayer._player.UISFX();
    }

    // Mostrar los diferentes tableros de liderazgos
    public void ShowLeaderboards()
    {
        MasterSFXPlayer._player.UISFX();
        if (GooglePlayGamesController._this) GooglePlayGamesController._this.ShowLeaderboards();
    }

    // Mostrar los logros del juego
    public void ShowAchievements()
    {
        MasterSFXPlayer._player.UISFX();
        if (GooglePlayGamesController._this) GooglePlayGamesController._this.ShowAchievements();
    }

    // Funcion llamada al comprar deluxe
    public void DeluxeReady()
    {
        logo.Show();
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

}
