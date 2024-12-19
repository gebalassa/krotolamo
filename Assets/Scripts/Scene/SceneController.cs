using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] protected Music currentMusic;
    [SerializeField] protected BackgroundEnvironment sceneEnvironment = BackgroundEnvironment.Default;
    [SerializeField] protected CanvasGroup[] mainCanvas;
    [SerializeField] protected float initialMove = 40;
    [SerializeField] [Range(0.1f, 1f)] protected float timeToMove = .5f;

    protected bool openDoor = true;
    protected bool isReady = false;
    protected bool isTransitioningToAnOverlay = false;
    protected bool autoplayMusic = true;

    // Background
    MovementBackground movingBackground;

    // Stack de objetos overlay
    protected List<GameObject> overlayObjects = new List<GameObject>();
    List<Transform> containerTransform = new List<Transform>();

    // Utiles
    protected OptionsMenu optionsMenu;
    protected BackCanvas backCanvas;
    bool menuChecked = false;
    bool backCanvasChecked = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Indicar que la pantalla no puede apagarse
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Localize();

        // Ante cualquier pifia en las escenas que cambien la escala de tiempo, se arreglara aca
        FixTimeScale();

        // Cambiar el ambiente de la escena
        ChangeBackgroundEnvironment(sceneEnvironment);

        // Obtener instancia de menu/backcanvas si existe
        CheckIfMenuExist();
        CheckIfBackCanvasExist();

        // Suscribirse al soporte de controles
        JoystickSupport.onJoystick += OnJoystick;

        // Suscribirse al cambio del estado de menu
        OptionsMenu.onToggle += OnToggleMenu;
    }

    /// <summary>
    /// Revision de existencia de menu
    /// </summary>
    void CheckIfMenuExist()
    {
        menuChecked = true;
        optionsMenu = FindObjectOfType<OptionsMenu>();
    }

    /// <summary>
    /// Revision de existencia de backcanvas
    /// </summary>
    void CheckIfBackCanvasExist()
    {
        backCanvasChecked = true;
        backCanvas = FindObjectOfType<BackCanvas>();
    }

    // Update is called once per frame
    protected void Update()
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
                GoBack();
            }
        }

    }

    /// <summary>
    /// Indicar los ajustes para dar por cargada la pantalla inicial
    /// </summary>
    /// <returns></returns>
    protected IEnumerator SetReady()
    {
        // Cambiar la música que se esta reproduciendo
        if (autoplayMusic) PlayMusic();
        
        if (TransitionDoors._this)
        {
            yield return StartCoroutine(TransitionDoors._this.Open());
            isReady = true;
        }
    }

    // Metodo llamado al presionar el boton volver en Android
    protected void AndroidBackAction()
    {
        SceneLoaderManager.Instance.Back();
    }

    protected virtual void PlayMusic() {
        if (MasterAudioPlayer._player) MasterAudioPlayer._player.PlayOrLoadThis(currentMusic);
    }

    // Actualiza todos los elementos ligados a un translate
    protected virtual void Localize() {}

    // Volver a la normalidad la escala de tiempo
    private void FixTimeScale()
    {
        Time.timeScale = 1;
        MasterAudioPlayer._player.ChangePitchAudio(1);
    }

    // Reproducir sonido de boton
    public virtual void UIButtonSFX()
    {
        MasterSFXPlayer._player.UISFX();
    }

    /// <summary>
    /// Inicio de secuencia de apertura de puertas para la realizacion de la creacion o destruccion de un objeto
    /// </summary>
    /// <param name="environment"></param>
    /// <param name="gameObjectToInitialize"></param>
    /// <param name="gameObjectToDestroy"></param>
    public virtual void SwitchUI(IEnumerator coroutine = null, bool closeDoors = false)
    {
        if (isTransitioningToAnOverlay) return;
        StartCoroutine(SwitchUICoroutine(coroutine, closeDoors));
    }
    
    /// <summary>
    /// Corutina que cierra puertas, realiza una accion de creacion o destruccion y establece que elementos de UI deben ser mostrados
    /// En caso de traer una corutina, se debe ocultar los elementos de la UI principal. De lo contrario, deben aparecer
    /// </summary>
    /// <param name="environment"></param>
    /// <param name="gameObjectToInitialize"></param>
    /// <param name="gameObjectToDestroy"></param>
    /// <returns></returns>
    IEnumerator SwitchUICoroutine(IEnumerator coroutine = null, bool closeDoors = false, bool uiSFX = true)
    {
        // Indicar que se esta transicionando hacia un overlay
        isTransitioningToAnOverlay = true;

        // Sonido UI: Se dejo aca por que en cada transicion deberia sonar, asi se asegura fallos cuando se olvide colocar en un script.
        if (uiSFX) MasterSFXPlayer._player.UISFX();

        // Revisar que puertas estan abiertas y luego proceder a cerrar
        if(closeDoors) yield return TransitionDoors._this.Open();
        if (closeDoors) yield return TransitionDoors._this.Close();

        // Si existe un overlay actualmente, ocultar
        if(overlayObjects.Count > 0)
        {
            GameObject overlayObject = overlayObjects[overlayObjects.Count - 1];
            if (overlayObject) overlayObject.SetActive(false);
        }
        else
        {
            // Ocultar  el UI principal
            HideMainContainer();
        }

        // Realizar la corutina indicada
        if (coroutine != null) yield return StartCoroutine(coroutine);

        if(closeDoors)  yield return TransitionDoors._this.Open();

        isTransitioningToAnOverlay = false;
    }

    /// <summary>
    /// Cerrado del ultimo overlay abierto
    /// </summary>
    public virtual void CloseOverlay()
    {
        if (isTransitioningToAnOverlay) return;
        StartCoroutine(CloseOverlayCoroutine());
    }

    /// <summary>
    /// Rutina para cerrar efectivamente ultimo overlay
    /// </summary>
    /// <returns></returns>
    IEnumerator CloseOverlayCoroutine(bool force = false)
    {
        // Indicar que se esta transicionando hacia un overlay
        isTransitioningToAnOverlay = true;

        bool closeDoors = false;

        if (overlayObjects.Count > 0)
        {
            GameObject overlayObject = overlayObjects[overlayObjects.Count - 1];

            // Si no es un cierre forzado, confirmar que el jugador pueda cerrarlo
            bool canClose = true;
            if (!force)
            {
                canClose = overlayObject.GetComponent<OverlayObject>().UserCanClose();
            }

            if (canClose)
            {
                overlayObjects.Remove(overlayObject);

                BackgroundEnvironment currentEnvironment = overlayObject.GetComponent<OverlayObject>().GetBackgroundEnvironment();
                BackgroundEnvironment environment = sceneEnvironment;
                if (overlayObjects.Count > 0)
                {
                    GameObject preOverlayObject = overlayObjects[overlayObjects.Count - 1];
                    preOverlayObject.SetActive(true);
                    environment = preOverlayObject.GetComponent<OverlayObject>().GetBackgroundEnvironment();
                }

                closeDoors = currentEnvironment != environment;

                // Revisar que puertas estan abiertas y luego proceder a cerrar
                if (closeDoors) yield return TransitionDoors._this.Open();
                if (closeDoors) yield return TransitionDoors._this.Close();

                // Destruir el overlay
                overlayObject.GetComponent<OverlayObject>().Close();

                // Mostrar el UI principal
                if (overlayObjects.Count == 0) StartCoroutine(ShowMainContainer(!closeDoors));

                // Cambiar el ambiente
                ChangeBackgroundEnvironment(environment);
            }

        }

        if (closeDoors) yield return TransitionDoors._this.Open();

        isTransitioningToAnOverlay = false;
    }

    /// <summary>
    /// Agregar al listado un objeto de clase OverlayObject
    /// </summary>
    /// <param name="overlayObject"></param>
    public virtual void AddOverlayObject(GameObject overlayObject)
    {
        overlayObjects.Add(overlayObject);
    }

    /// <summary>
    /// Obtener el fondo que se mueve relacionado a la escena
    /// </summary>
    protected void GetMovingBackground()
    {
        movingBackground = FindObjectOfType<MovementBackground>();
    }

    /// <summary>
    /// Cambiar el ambiente del fondo actual
    /// </summary>
    /// <param name="backgroundEnvironment"></param>
    public void ChangeBackgroundEnvironment(BackgroundEnvironment backgroundEnvironment)
    {
        // Cambiar el ambiente
        GetMovingBackground();
        if (movingBackground) movingBackground.ChangeMaterial(backgroundEnvironment);
    }

    /// <summary>
    /// Obtencion de ambiente que esta usando el movingBackground
    /// </summary>
    /// <returns></returns>
    public BackgroundEnvironment GetCurrentBackgroundEnvironment()
    {
        GetMovingBackground();
        if (movingBackground) return movingBackground.GetCurrentMaterial();
        else return sceneEnvironment;
    }

    /// <summary>
    /// Retrocede de escena o quita un overlay de ser necesario
    /// </summary>
    public virtual void GoBack()
    {
        MasterSFXPlayer._player.UISFX();

        // Ver si hay algun objeto overlay activo
        if (overlayObjects.Count > 0)
        {
            CloseOverlay();
        }
        else
        {
            AndroidBackAction();
        }
    }

    /// <summary>
    /// Corutina para mostrar la aparicion de las UI principales
    /// </summary>
    /// <param name="alpha">Indica si se debe ejecutar el cambio de alpha</param>
    /// <param name="movement">Indica si se debe ejecutar el cambio de posicion</param>
    /// <returns></returns>
    private IEnumerator ShowMainContainer(bool animate = true)
    {
        // Activar la UI
        foreach (CanvasGroup canvas in mainCanvas)
        {
            if (canvas)
            {
                canvas.gameObject.SetActive(true);
                if (containerTransform.Count == 0)
                {
                    foreach (Transform t in canvas.transform) containerTransform.Add(t);
                }
            }
        }

        // Aplicar animacion de ser necesario
        if (animate)
        {
            // Mover el contenedor
            System.Action<ITween<float>> updatePosition = (t) =>
            {
                foreach (Transform transformTarget in containerTransform)
                {
                    if (transformTarget) transformTarget.localPosition = new Vector3(t.CurrentValue, transformTarget.localPosition.y, transformTarget.localPosition.z);
                }
            };

            // Cmabiar el alpha
            System.Action<ITween<float>> updateAlpha = (t) =>
            {
                foreach (CanvasGroup canvas in mainCanvas)
                {
                    if (canvas) canvas.alpha = t.CurrentValue;
                }
            };

            // Hacer fade in y mostrar elemento
            gameObject.Tween(string.Format("FadeIn{0}", GetInstanceID()), 0, 1, timeToMove, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);
            gameObject.Tween(string.Format("Move{0}", GetInstanceID()), containerTransform[0].localPosition.x + initialMove, containerTransform[0].localPosition.x, timeToMove, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

        }
        else
        {
            foreach (CanvasGroup canvas in mainCanvas)
            {
                if (canvas)
                {
                    canvas.gameObject.SetActive(true);
                }
            }
        }

        yield return null;
    }

    /// <summary>
    /// Ocultar los contenedores de GUI principales
    /// </summary>
    /// <returns></returns>
    void HideMainContainer()
    {
        if (mainCanvas.Length > 0)
        {
            foreach (CanvasGroup canvas in mainCanvas)
            {
                if (canvas) canvas.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Desuscripcion de eventos
    /// </summary>
    protected virtual void OnDestroy()
    {
        // Desuscribirse al soporte de controles
        JoystickSupport.onJoystick -= OnJoystick;
        // Desuscribirse al cambio del estado de menu
        OptionsMenu.onToggle -= OnToggleMenu;
    }

    /// <summary>
    /// Consulta la posiblidad de abrir menu
    /// </summary>
    /// <returns></returns>
    public virtual bool CanToggleMenu()
    {
        return true;
    }

    /// <summary>
    /// Recepcion del estado del menu
    /// </summary>
    /// <param name="state"></param>
    protected virtual void OnToggleMenu(OptionsMenu.OptionsMenuStates state){}

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    /// <returns>Indica si la accion puede continuar a los hijos</returns>
    protected virtual bool OnJoystick(JoystickAction action, int playerIndex)
    {
        if (!menuChecked) CheckIfMenuExist();
        if (!backCanvasChecked) CheckIfBackCanvasExist();

        bool canContinue = true;
        if (optionsMenu != null && optionsMenu.isActiveAndEnabled)  canContinue = optionsMenu.OnJoystick(action, playerIndex);
        if (canContinue && backCanvas != null)
        {
            switch (action)
            {
                case JoystickAction.L:
                case JoystickAction.Escape:
                    backCanvas.Back();
                    canContinue = false;
                    break;
            }
        }

        // Si existe un overlay, este tiene el control del joystcik
        if (canContinue && overlayObjects.Count > 0) canContinue = overlayObjects[overlayObjects.Count - 1].GetComponent<OverlayObject>().OnJoystick(action, playerIndex);

        return canContinue;
    }
    #endregion
}
