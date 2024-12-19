using DigitalRuby.Tween;
using System.Collections;
using UnityEngine;

public class OverlayObject : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] protected BackgroundEnvironment backgroundEnvironment = BackgroundEnvironment.Default;
    [SerializeField] protected GameObject mainContainer;
    [SerializeField] protected float initialMove = 40;
    [SerializeField] [Range(0.1f, 1f)] protected float timeToMove = .5f;
    [SerializeField] protected bool userCanCloseIt = true;
    [SerializeField] protected bool useSameBackground = false;

    // Utiles
    protected bool isClosing = false;

    protected virtual void Start()
    {
        SceneController sceneController = FindObjectOfType<SceneController>();
        if (sceneController == null) return;

        if(useSameBackground) backgroundEnvironment = sceneController.GetCurrentBackgroundEnvironment();

        sceneController.AddOverlayObject(gameObject);
        sceneController.ChangeBackgroundEnvironment(backgroundEnvironment);
    }

    /// <summary>
    /// Corutina para mostrar la aparicion del modal
    /// </summary>
    /// <param name="alpha">Indica si se debe ejecutar el cambio de alpha</param>
    /// <param name="movement">Indica si se debe ejecutar el cambio de posicion</param>
    /// <returns></returns>
    protected virtual IEnumerator ShowCoroutine(bool alpha = true, bool movement = true)
    {
        Transform containerTransform = mainContainer.transform;
        CanvasGroup canvas = GetComponent<CanvasGroup>();

        // Mover el contenedor
        System.Action<ITween<float>> updatePosition = (t) =>
        {
            if (containerTransform) containerTransform.localPosition = new Vector3(t.CurrentValue, containerTransform.localPosition.y, containerTransform.localPosition.z);
        };

        // Cmabiar el alpha
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvas) canvas.alpha = t.CurrentValue;
        };

        // Hacer fade in y mostrar elemento
        if(alpha) gameObject.Tween(string.Format("FadeIn{0}", GetInstanceID()), 0, 1, timeToMove, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);
        if(movement) gameObject.Tween(string.Format("Move{0}", GetInstanceID()), containerTransform.localPosition.x + initialMove, containerTransform.localPosition.x, timeToMove, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

        yield return null;
    }

    /// <summary>
    /// Cambiar la posibilidad de que  el jugador pueda cerrar un mensaje overlay
    /// </summary>
    /// <returns></returns>
    public virtual void SetUserCanClose(bool value)
    {
        userCanCloseIt = value;
    }

    /// <summary>
    /// Indica si el usuario esta autorizado a cerrar el overlay
    /// </summary>
    /// <returns></returns>
    public virtual bool UserCanClose()
    {
        return userCanCloseIt;
    }

    /// <summary>
    /// Metodo para el cierre del overlay por defecto
    /// </summary>
    public virtual void Close()
    {
        if (isClosing) return;
        isClosing = true;
        StartCoroutine(CloseCoroutine());
    }

    /// <summary>
    /// Corutina para realizar el cerrado del overlay con alpha incluido
    /// </summary>
    /// <returns></returns>
    protected IEnumerator CloseCoroutine()
    {
        yield return null;
        Destroy(gameObject);
    }

    /// <summary>
    /// Obtencion del ambiente del background para este overlay
    /// </summary>
    /// <returns></returns>
    public BackgroundEnvironment GetBackgroundEnvironment()
    {
        return backgroundEnvironment;
    }

    /// <summary>
    /// Realizar la aparicion del elemento
    /// </summary>
    protected virtual void OnEnable()
    {
        StartCoroutine(ShowCoroutine());
    }

    /// <summary>
    /// Realizar la desaparicion del elemento
    /// </summary>
    protected virtual void OnDisable()
    {
        
    }

    /// <summary>
    /// Peticion de cerrado de overlay
    /// </summary>
    protected void RequestClose()
    {
        SceneController sceneController = FindObjectOfType<SceneController>();
        if (sceneController) sceneController.CloseOverlay();
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    public virtual bool OnJoystick(JoystickAction action, int playerIndex)
    {
        return false;
    }
    #endregion
}