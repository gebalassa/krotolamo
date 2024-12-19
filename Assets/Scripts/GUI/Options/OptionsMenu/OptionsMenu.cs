using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour, IPointerClickHandler
{
    [Header("Elements")]
    [SerializeField] Button trigger;
    [SerializeField] Image triggerIcon;
    [SerializeField] CanvasGroup canvasGroup;

    [Header("Options")]
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform optionsContainer;
    [SerializeField] Button leaderboardOptions;
    [SerializeField] Button achievementsOptions;
    [SerializeField] Button settingsOptions;
    [SerializeField] Button discordOptions;
    [SerializeField] Button creditsOptions;
    [SerializeField] TutorialOptionsMenu tutorialOptions;
    [SerializeField] Button gameplayExitOptions;
    [SerializeField] Button buttonLayoutOptions;
    [SerializeField] Button keyboardLayoutOptions;

    [Header("Prefab")]
    [SerializeField] GameObject overlaySettings;
    [SerializeField] GameObject gameplayExitPrefab;
    [SerializeField] GameObject buttonLayoutPrefab;
    [SerializeField] GameObject keyboardLayoutPrefab;

    [Header("Animation")]
    [SerializeField][Range(0, 2)] float canvasFadeTime = 0.25f;
    [SerializeField] [Range(0, 2)] float optionsMoveTime = .1f;
    [SerializeField] [Range(0, 100)] float optionsMovementXDelta = 40;

    [Header("Trigger")]
    [SerializeField] Sprite triggerOpenStateIcon;
    [SerializeField] Sprite triggerCloseStateIcon;

    [Header("Others")]
    [SerializeField] bool showLeaderboard = true;
    [SerializeField] bool showAchievements = true;
    [SerializeField] bool reactToSteamOverlay = false;

    /// <summary>
    /// Eventos de cambio en el estado del estado del menu
    /// </summary>
    public delegate void OnToggle(OptionsMenuStates state);
    public static event OnToggle onToggle;

    // Estados
    public enum OptionsMenuStates {
        Close,
        Transition,
        Open
    };
    OptionsMenuStates currentState;

    // Opciones habilitadas
    enum OptionsKeys
    {
        Leaderboard,
        Achievements,
        Settings,
        Discord,
        Credits,
        Tutorial,
        GameplayExit,
        ButtonLayout,
        KeyboardLayout
    }

    // Posiciones iniciales de las opciones
    List<Vector3> optionsInitialPositions = new List<Vector3>();

    // Utiles
    List<JoystickUIElement> activeOptions = new List<JoystickUIElement>();
    int focusIndex = 0;

    // Scena actual
    SceneController currentScene;

    // Utiles
    Coroutine reloadActiveOptionsCoroutine;
    bool isLoadingOptions = false;

    // Use this for initialization
    void Start()
    {
        // Para mostrar opciones
        trigger.onClick.AddListener(Toggle);

        leaderboardOptions.onClick.AddListener( delegate { OnClick(OptionsKeys.Leaderboard); });
        achievementsOptions.onClick.AddListener( delegate { OnClick(OptionsKeys.Achievements); });
        settingsOptions.onClick.AddListener( delegate { OnClick(OptionsKeys.Settings); });
        discordOptions.onClick.AddListener( delegate { OnClick(OptionsKeys.Discord); });
        creditsOptions.onClick.AddListener( delegate { OnClick(OptionsKeys.Credits); });
        //if (tutorialOptions && tutorialOptions.isActiveAndEnabled) (tutorialOptions.GetComponent<Button>()).onClick.AddListener(delegate { OnClick(OptionsKeys.Tutorial); });
        if (gameplayExitOptions && gameplayExitOptions.isActiveAndEnabled) gameplayExitOptions.onClick.AddListener(delegate { OnClick(OptionsKeys.GameplayExit); });
        if (buttonLayoutOptions) buttonLayoutOptions.onClick.AddListener(delegate { OnClick(OptionsKeys.ButtonLayout); });
        if (keyboardLayoutOptions) keyboardLayoutOptions.onClick.AddListener(delegate { OnClick(OptionsKeys.KeyboardLayout); });
        if (JoystickSupport.Instance.SupportActivated())
        {
            buttonLayoutOptions.gameObject.SetActive(true);
            keyboardLayoutOptions.gameObject.SetActive(true);
        }

        // Desactivar opciones no requeridas
        if (!showLeaderboard) leaderboardOptions.gameObject.SetActive(false);
        if (!showAchievements) achievementsOptions.gameObject.SetActive(false);

        OnJoystickSupportChange(JoystickSupport.Instance.SupportActivated());

        currentScene = FindObjectOfType<SceneController>();
        JoystickSupport.onSupportStatusChange += OnJoystickSupportChange;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        if (optionsContainer.transform.localPosition.y == Mathf.Infinity) optionsContainer.transform.localPosition = new Vector2(0,0);
    }

    /// <summary>
    /// Revisar si esta abierto el menu
    /// </summary>
    /// <returns></returns>
    public bool IsOpen()
    {
        return currentState == OptionsMenuStates.Open;
    }

    /// <summary>
    /// Revisar si esta en estado de transicion
    /// </summary>
    /// <returns></returns>
    public bool IsTransitioning()
    {
        return currentState == OptionsMenuStates.Transition;
    }

    /// <summary>
    /// Realiza el cambio de estado entre el estado de apertura o cierre de las opciones
    /// </summary>
    public void Toggle()
    {
        if (currentScene && !currentScene.CanToggleMenu()) return;
        if (currentState == OptionsMenuStates.Transition) return;
        Toggle(currentState != OptionsMenuStates.Open);
    }

    /// <summary>
    /// Apertura de menu a partir del estado solicitado
    /// </summary>
    /// <param name="open"></param>
    public void Toggle(bool open = true)
    {
        if (currentScene && !currentScene.CanToggleMenu()) return;
        if (currentState == OptionsMenuStates.Transition) return;

        if (open) {
            if (currentState == OptionsMenuStates.Close) Open();
        }
        else
        {
            if (currentState == OptionsMenuStates.Open) Close();
        }
    }

    /// <summary>
    /// Apertura de las opciones del menu
    /// </summary>
    void Open() {
        if (currentState == OptionsMenuStates.Transition) return;
        MasterSFXPlayer._player.UISFX();
        if (onToggle != null) onToggle(OptionsMenuStates.Open);

        currentState = OptionsMenuStates.Transition;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        triggerIcon.sprite = triggerOpenStateIcon;

        System.Action<ITween<float>> fadeFn = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
            if (triggerIcon) triggerIcon.transform.eulerAngles = new Vector3(0, 0, t.CurrentValue * 360);
        };

        System.Action<ITween<float>> fadeFnComplete = (t) =>
        {
            currentState = OptionsMenuStates.Open;
        };

        gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), 0, 1,
         canvasFadeTime, TweenScaleFunctions.QuadraticEaseInOut, fadeFn, fadeFnComplete);

        if (JoystickSupport.Instance.SupportActivated()) FocusOption();

        StartCoroutine(MoveOptions());
    }

    /// <summary>
    /// Cierre de las opciones del menu
    /// </summary>
    void Close()
    {
        if (currentState == OptionsMenuStates.Transition) return;
        MasterSFXPlayer._player.UISFX();

        currentState = OptionsMenuStates.Transition;
        triggerIcon.sprite = triggerCloseStateIcon;

        System.Action<ITween<float>> fadeFn = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
            if (triggerIcon) triggerIcon.transform.eulerAngles = new Vector3(0,0,t.CurrentValue * 360);
        };

        System.Action<ITween<float>> fadeFnComplete = (t) =>
        {
            if (canvasGroup) canvasGroup.interactable = false;
            if (canvasGroup) canvasGroup.blocksRaycasts = false;
            if (canvasGroup) currentState = OptionsMenuStates.Close;
            if (onToggle != null) onToggle(OptionsMenuStates.Close);
        };

        gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), 1, 0,
         canvasFadeTime, TweenScaleFunctions.QuadraticEaseInOut, fadeFn, fadeFnComplete);

        StartCoroutine(MoveOptions(true));
    }

    /// <summary>
    /// Realizar animacion de movimiento de las opciones
    /// </summary>
    /// <param name="close"></param>
    IEnumerator MoveOptions(bool close = false)
    {
        if(optionsInitialPositions.Count == 0)
        {
            // Agregar todas las posiciones iniciales
            foreach (Transform t in optionsContainer)
            {
                optionsInitialPositions.Add(t.localPosition);
            }
        }

        foreach (Transform child in optionsContainer)
        {
            //child.gameObject.SetActive(false);
            //child.gameObject.SetActive(true);
            System.Action<ITween<Vector3>> moveFn = (t) =>
            {
                if (child) child.localPosition = t.CurrentValue;
            };
            
            Vector3 initialPosition = optionsInitialPositions[child.GetSiblingIndex()];
            Vector2 startPosition = new Vector2( close? initialPosition.x :  (initialPosition.x + optionsMovementXDelta), child.transform.localPosition.y);
            Vector2 endPosition = new Vector2(close? (initialPosition.x + optionsMovementXDelta) : initialPosition.x, child.transform.localPosition.y);

            child.gameObject.Tween(string.Format("Move{0}", child.gameObject.GetInstanceID()), startPosition, endPosition,
             optionsMoveTime, TweenScaleFunctions.QuadraticEaseInOut, moveFn);

            yield return new WaitForSeconds(.025f);
        }
    }

    /// <summary>
    /// Realizar accion asociada a una opcion
    /// </summary>
    /// <param name="key"></param>
    void OnClick(OptionsKeys key) {
        if (currentState == OptionsMenuStates.Transition) return;

        switch (key)
        {
            case OptionsKeys.Leaderboard:
                ShowRankingOverlay();
                break;
            case OptionsKeys.Achievements:
                ShowAchievements();
                break;
            case OptionsKeys.Settings:
                ShowSettings();
                break;
            case OptionsKeys.Discord:
                Application.OpenURL(DiscordInvitation.discordLink);
                break;
            case OptionsKeys.Credits:
                ShowCredits();
                break;
            case OptionsKeys.Tutorial:
                tutorialOptions.ToogleTutorial();
                break;
            case OptionsKeys.GameplayExit:
                ShowGameplayExit();
                break;
            case OptionsKeys.ButtonLayout:
                ShowButtonLayout();
                break;
            case OptionsKeys.KeyboardLayout:
                ShowKeyboardLayout();
                break;
        }
        
        if (key != OptionsKeys.Tutorial) Close();
        else MasterSFXPlayer._player.UISFX();
    }

    /// <summary>
    /// Click para cierre en panel
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Close();
    }

    /// <summary>
    /// Focalizacion en la opcion del menu
    /// </summary>
    private void FocusOption()
    {
        if (focusIndex < 0 || focusIndex >= activeOptions.Count) focusIndex = 0;
        for (int i = 0; i < activeOptions.Count; i++)
        {
            if (i == focusIndex)
            {
                ScrollTo(activeOptions[i].transform.localPosition);
                activeOptions[i].SetInteractable(true);
            }
            else activeOptions[i].SetInteractable(false);

        }
    }

    /// <summary>
    /// Desenfocar la opcion actual y volver a habilitar todas las opciones
    /// </summary>
    private void UnfocusOption() {
        foreach(JoystickUIElement element in activeOptions)
        {
            element.SetInteractable(true);
        }
    }

    /// <summary>
    /// Abrir la opcion actualmente enfocada
    /// </summary>
    private void OpenOptionByFocusIndex()
    {
        if (focusIndex < 0 || focusIndex >= activeOptions.Count) focusIndex = 0;
        activeOptions[focusIndex].GetButton().onClick.Invoke();
    }

    /// <summary>
    /// Se centra el scroll en el elemento indicado
    /// </summary>
    /// <param name="itemPosition"></param>
    private void ScrollTo(Vector2 itemPosition)
    {
        float verticalNormalizedPosition = (float)System.Math.Round(Mathf.Abs(itemPosition.y / ((RectTransform)optionsContainer).rect.height), 1);
        System.Action<ITween<float>> updateScroll = (t) =>
        {
            if (scrollRect) scrollRect.verticalNormalizedPosition = t.CurrentValue;
        };

        float toNormalizedPosition = 1 - verticalNormalizedPosition;
        scrollRect.gameObject.Tween(string.Format("Scroll{0}", scrollRect.GetInstanceID()), scrollRect.verticalNormalizedPosition, toNormalizedPosition, .1f, TweenScaleFunctions.QuadraticEaseOut, updateScroll);
    }

    /// <summary>
    /// Indicacion de cambio en el soporte para joystick
    /// </summary>
    /// <param name="value"></param>
    private void OnJoystickSupportChange(bool value)
    {
        if (reloadActiveOptionsCoroutine != null) StopCoroutine(reloadActiveOptionsCoroutine);
        reloadActiveOptionsCoroutine = StartCoroutine(ReloadActiveOptions(value));
    }

    /// <summary>
    /// Recarga de las opciones activas del menu
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private IEnumerator ReloadActiveOptions(bool value)
    {
        while (isLoadingOptions) yield return null;
        if (buttonLayoutOptions) buttonLayoutOptions.gameObject.SetActive(value);
        if (keyboardLayoutOptions) keyboardLayoutOptions.gameObject.SetActive(value);

        // Recargar las opciones disponibles
        activeOptions.Clear();
        isLoadingOptions = true;
        foreach (Transform t in optionsContainer)
        {
            if (t.gameObject.activeSelf) activeOptions.Add(t.GetComponent<JoystickUIElement>());
        }
        isLoadingOptions = false;

        if (value)
        {
            focusIndex = 0;
            FocusOption();
        }
        else UnfocusOption();
    }

    /// <summary>
    /// Activar o desactivar la opcion de tutorial
    /// </summary>
    public void ActiveTutorial(bool value = true) {
        tutorialOptions.gameObject.SetActive(value);
        (tutorialOptions.GetComponent<Button>()).onClick.RemoveAllListeners();
        if (value){
            (tutorialOptions.GetComponent<Button>()).onClick.AddListener(delegate { OnClick(OptionsKeys.Tutorial); });
        }
        OnJoystickSupportChange(JoystickSupport.Instance.SupportActivated());
    }

    /// <summary>
    /// Indicar si esta cargando las opciones del menu
    /// </summary>
    public bool IsLoadingOptions(){
        return isLoadingOptions;
    }

    private void OnDestroy()
    {
        if (reloadActiveOptionsCoroutine != null) StopCoroutine(reloadActiveOptionsCoroutine);
        JoystickSupport.onSupportStatusChange -= OnJoystickSupportChange;
    }

    #region Leaderboard
    /// <summary>
    /// Muestra el ranking online de los jugadores
    /// </summary>
    private void ShowRankingOverlay()
    {
        #if UNITY_ANDROID
        MasterSFXPlayer._player.UISFX();
        if (GooglePlayGamesController._this) GooglePlayGamesController._this.ShowLeaderboards();
        #endif
    }
    #endregion

    #region Settings
    /// <summary>
    /// Muestra las configuraciones del juego
    /// </summary>
    private void ShowSettings()
    {
        // Revisar si ya existe el ranking
        if (FindObjectOfType<SettingsOverlay>()) return;

        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();
        sceneController.SwitchUI(OpenSettings());
    }

    /// <summary>
    /// Apertura de las configuraciones
    /// </summary>
    /// <returns></returns>
    public IEnumerator OpenSettings()
    {
        Instantiate(overlaySettings);
        yield return null;
    }
    #endregion

    #region Credits
    /// <summary>
    /// Muestra los creditos del juego
    /// </summary>
    private void ShowCredits()
    {
        SceneLoaderManager._instance.Credits();
    }
    #endregion

    #region Achievements
    /// <summary>
    /// Muestra los logros del jugador (Modo single player, conectado con Google Play / Apple Store)
    /// </summary>
    private void ShowAchievements()
    {
        #if UNITY_ANDROID
        MasterSFXPlayer._player.UISFX();
        if (GooglePlayGamesController._this) GooglePlayGamesController._this.ShowAchievements();
        #endif
    }
    #endregion

    #region GameplayExit
    /// <summary>
    /// Muestra pantalla de confirmacion de salida de partida
    /// </summary>
    private void ShowGameplayExit()
    {
        // Revisar si ya existe pantalla de cierre
        if (FindObjectOfType<GameplayExitOverlay>()) return;

        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();
        sceneController.SwitchUI(OpenGameplayExit());
    }

    /// <summary>
    /// Apertura de la pantalla de confirmacion de cierre
    /// </summary>
    /// <returns></returns>
    public IEnumerator OpenGameplayExit()
    {
        Instantiate(gameplayExitPrefab);
        yield return null;
    }
    #endregion

    #region ButtonLayout
    /// <summary>
    /// Muestra pantalla de controles de joystick
    /// </summary>
    private void ShowButtonLayout()
    {
        // Revisar si ya existe pantalla de joystick
        if (FindObjectOfType<JoystickGameplayDescription>()) return;

        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();
        sceneController.SwitchUI(OpenButtonLayout());
    }

    /// <summary>
    /// Apertura de la pantalla de controles de joystick
    /// </summary>
    /// <returns></returns>
    public IEnumerator OpenButtonLayout()
    {
        Instantiate(buttonLayoutPrefab);
        yield return null;
    }
    #endregion

    #region KeyboardLayout
    /// <summary>
    /// Muestra pantalla de controles de teclado
    /// </summary>
    private void ShowKeyboardLayout()
    {
        // Revisar si ya existe pantalla de joystick
        if (FindObjectOfType<KeyboardGameplayDescription>()) return;

        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();
        sceneController.SwitchUI(OpenKeyboardLayout());
    }

    /// <summary>
    /// Apertura de la pantalla de controles de teclado
    /// </summary>
    /// <returns></returns>
    public IEnumerator OpenKeyboardLayout()
    {
        Instantiate(keyboardLayoutPrefab);
        yield return null;
    }
    #endregion

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    public virtual bool OnJoystick(JoystickAction action, int playerIndex)
    {
        switch (action)
        {
            case JoystickAction.Start:
                Toggle();
                break;
        }
        if (IsOpen()) {
            switch (action)
            {
                case JoystickAction.Up:
                    if (focusIndex > 0)
                    {
                        focusIndex--;
                        FocusOption();
                    };
                    break;
                case JoystickAction.Down:
                    if (focusIndex < activeOptions.Count - 1)
                    {
                        focusIndex++;
                        FocusOption();
                    }
                    break;
                case JoystickAction.R:
                case JoystickAction.X:
                case JoystickAction.Y:
                case JoystickAction.A:
                case JoystickAction.B:
                    // Apertura de opcion en base a focusIndex
                    OpenOptionByFocusIndex();
                    break;
                case JoystickAction.L:
                case JoystickAction.Escape:
                    Close();
                    break;
            }
        }

        return !IsOpen() && !IsTransitioning();
    }
    #endregion
}