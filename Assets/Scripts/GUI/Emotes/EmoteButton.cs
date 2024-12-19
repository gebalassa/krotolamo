using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmoteButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Setup")]
    [SerializeField] JankenUp.Online.Emotes.emotesID identifier;

    [Header("Focus/Blur Animation")]
    [SerializeField] Color focusColor = Color.white;
    [SerializeField] Color blurColor = new Color(1, 1, 1, 0.5f);
    [SerializeField] [Range(0, 1f)] float blurTimeFade = 0.5f;

    [Header("Others")]
    [SerializeField] [Range(0.1f, 2f)] float timeToToggle = 1f;

    // Animaciones
    ColorTween fadeAnimation;

    // Componentes
    EmotesController emoteController;
    Image imageRenderer;
    Button button;

    // Utiles
    bool isTrigger = false;
    bool selected = false;

    // Para considerar si es un tap o no
    float timeTap = 0.25f;
    float timeTapCurrent = 0;

    // Use this for initialization
    void Start()
    {
        imageRenderer = GetComponent<Image>();
        button = GetComponent<Button>();
        AnimationBlur();
    }

    // Update is called once per frame
    void Update()
    {
        // En caso de que sea un trigger y se este manteniendo el boron presionado, ver si se debe notificar al controlador
        if(isTrigger && timeTapCurrent > 0) {
            if(Time.time - timeTapCurrent >= timeTap)
            {
                NotifyChange();
            }
        }
    }

    /// <summary>
    /// Obtencion del identificador del emote como un ID
    /// </summary>
    /// <returns></returns>
    public int GetIDAsInt()
    {
        return (int)identifier; 
    }

    /// <summary>
    /// Indicar cual es el controlador de emotes asociado
    /// </summary>
    /// <param name="emoteController">Controlador de emotes</param>
    public void SetEmoteController(EmotesController emoteController)
    {
        this.emoteController = emoteController;
    }

    /// <summary>
    /// Indicar si el boton es el trigger actual y si debe ser animado o no el cambio a su ubicacion
    /// </summary>
    /// <param name="trigger">Indicar de si es el trigger</param>
    /// <param name="animated">Indicador de si debe animarse</param>
    public void SetTrigger(bool trigger, bool animated)
    {
        isTrigger = trigger;
        if (animated)
        {
            StartCoroutine(AnimateSetTrigger());
        }
        else
        {
            gameObject.SetActive(isTrigger);
        }
    }

    /// <summary>
    /// Metodo alternativo para indicar trigger y animar
    /// </summary>
    /// <param name="trigger"></param>
    public void SetTrigger(bool trigger) {
        SetTrigger(trigger, true);
    }

    /// <summary>
    /// Notificacion de apertura al controlador de emotes
    /// </summary>
    private void NotifyChange() {
        Reset();
        emoteController.Deselect();

        // Abrir o cerrar segun corresponda
        emoteController.Toggle();
    }

    /// <summary>
    /// Resetear las variables necesarias para volver al estado normal
    /// </summary>
    private void Reset()
    {
        timeTapCurrent = 0;
        selected = false;
    }

    /// <summary>
    /// Indicar emote como seleccionado
    /// </summary>
    private void Select() {
        selected = true;
        emoteController.Select(this);
    }

    #region Button Events

    /// <summary>
    /// Evento llamado cuando el boton es presionado
    /// </summary>
    /// <param name="eventData"></param>
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (isTrigger) timeTapCurrent = Time.time;
        Select();
    }
    int counter = 0;
    /// <summary>
    /// Al levantar el puntero, se debe verificar si esta o no seleccionado el emote
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        // En caso de que se haya haya notificado apertura, notificar seleccion de emote
        emoteController.ConfirmSelection();
        Reset();
    }

    /// <summary>
    /// Evento llamado al poner el puntero sobre el elemento.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Reset();
        Select();
        AnimationFocus();
    }

    /// <summary>
    /// Evento llamado al quitar el puntero del elemento
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        Reset();
        selected = false;
        emoteController.Deselect();
        AnimationBlur();
    }

    #endregion

    /// <summary>
    /// Mostrar el boton de emote
    /// </summary>
    public void Show()
    {
        AnimationShow();
    }

    /// <summary>
    /// Ocultar el boton de emote
    /// </summary>
    public void Hide()
    {
        AnimationHide();
    }

    /// <summary>
    /// Animar la seleccion del emote como trigger general
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimateSetTrigger()
    {
        Hide();
        yield return new WaitForSeconds(timeToToggle);
        transform.localPosition = new Vector3(0, 0, 0);
        Show();
    }

    #region Animations

    /// <summary>
    /// Realizar corutina especificada
    /// </summary>
    /// <param name="coroutine">Rutina a realizar</param>
    private void DoAnimation(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// Animar la aparicion del trigger
    /// </summary>
    public void AnimationShow()
    {
        gameObject.SetActive(true);
        DoAnimation(AnimationTrigger(0, 1, TweenScaleFunctionsExtension.EaseElasticOut));
    }

    /// <summary>
    /// Animar la desaparicion del trigger
    /// </summary>
    public void AnimationHide()
    {
        gameObject.SetActive(true);
        AnimationBlur();
        DoAnimation(AnimationTrigger(1, 0, TweenScaleFunctionsExtension.EaseElasticIn));
    }

    /// <summary>
    /// Aparicion del trigger de finta
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    public IEnumerator AnimationTrigger(float from, float to, Func<float, float> scaleFunction)
    {

        System.Action<ITween<float>> zoomInOut = (t) =>
        {
            if (gameObject != null) gameObject.transform.localScale = new Vector3(t.CurrentValue, t.CurrentValue, t.CurrentValue);
        };

        // Que ataque aparezca
        gameObject.Tween(string.Format("ZoomIn{0}", gameObject.GetInstanceID()), from, to,
            timeToToggle, scaleFunction, zoomInOut);

        yield return null;

    }

    /// <summary>
    /// Animar cuando el jugador se posa sobre el elemento
    /// </summary>
    public void AnimationFocus()
    {
        DoAnimation(AnimationFade(imageRenderer.color, focusColor, TweenScaleFunctions.Linear));
    }

    /// <summary>
    /// Animar cuando el jugador se posa fuera del elemento
    /// </summary>
    public void AnimationBlur()
    {
        if (isTrigger) return;
        //DoAnimation(AnimationFade(imageRenderer.color, blurColor, TweenScaleFunctions.Linear));
    }

    /// <summary>
    /// Animacion de fade para elemento 
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    /// <param name="scaleFunction">Funcion para tiempos de animacion</param>
    private IEnumerator AnimationFade(Color from, Color to, Func<float, float> scaleFunction)
    {
        if (fadeAnimation != null) fadeAnimation.Stop(TweenStopBehavior.DoNotModify);

        System.Action<ITween<Color>> fadeFn = (t) =>
        {
            if (imageRenderer != null) imageRenderer.color = t.CurrentValue;
        };

        // Que ataque aparezca
        fadeAnimation = gameObject.Tween(string.Format("zoom{0}", imageRenderer.GetInstanceID()), from, to,
            blurTimeFade, scaleFunction, fadeFn);

        yield return null;

    }

    #endregion

    /// <summary>
    /// Activar la interactividad del boton
    /// </summary>
    /// <param name="enable"></param>
    public void Enable(bool enable)
    {
        button.interactable = enable;
    }
}