using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Emote : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] JankenUp.Online.Emotes.emotesID identifier;

    [Header("Zoom Animation")]
    [SerializeField] [Range(0, 1f)] float zoomTime = 0.25f;

    [Header("Focus/Blur Animation")]
    [SerializeField] Color focusColor = Color.white;
    [SerializeField] Color blurColor = new Color(1, 1, 1, 0.5f);
    [SerializeField] [Range(0, 1f)] float blurTimeFade = 0.25f;

    [Header("Move animation")]
    [SerializeField] [Range(0, 10f)] float minSpeed = .5f;
    [SerializeField] [Range(0, 10f)] float maxSpeed = 1f;
    [SerializeField] [Range(0.1f, 2f)] float timeMoving = 1.2f;
    [SerializeField] [Range(0.1f, 15f)] float maxRotation = 15f;
    [SerializeField] [Range(0, 10f)] float speedOnWorld = .01f;

    [Header("Others")]
    [SerializeField] [Range(0.1f, 2f)] float timeFloating = 1f;
    [SerializeField] [Range(1, 2)] float scale = 1.5f;

    // Animaciones
    ColorTween fadeAnimation;

    // Componentes
    Image imageRenderer;
    SpriteRenderer spriteRenderer;

    // Indicador de si existe en el plano UI (Por defecto) o en un objeto del mundo
    bool isPartOfUI = true;

    // Direccion que toma el emote en caso de ser parte del mundo (Salir de un PJ)
    int directionOnCharacter = -1;

    // Use this for initialization
    void Start()
    {
        GetComponents();
        StartCoroutine(AnimateTheWholeThing());
    }

    /// <summary>
    /// Obtener los componentes frecuentes
    /// </summary>
    private void GetComponents()
    {
        if (imageRenderer || spriteRenderer) return;
        imageRenderer = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Configuracion inicial del emote, estableciendo su posicion y animando su aparicion
    /// </summary>
    public void SetupUIContainer(EmotesContainer container)
    {
        Transform spawnPortal = container.RandomSpawnPortal();
        transform.parent = spawnPortal;
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// Asignar una posicion del elemento
    /// </summary>
    /// <param name="position"></param>
    /// <param name="localScale"></param>
    public void SetLocalPosition(Vector2 position, float localScale)
    {
        transform.localPosition = position;
        transform.localScale = new Vector3(localScale, transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Indica que el emote es parte del mundo y no del UI
    /// </summary>
    public void SetPartOfWorld()
    {
        isPartOfUI = false;
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
    /// Obtencion del sprite asociado al emote
    /// </summary>
    /// <returns></returns>
    public Sprite GetSprite()
    {
        GetComponents();
        return imageRenderer ? imageRenderer.sprite : spriteRenderer.sprite;
    }

    /// <summary>
    /// Asignar el sprite al componente correspondiente
    /// </summary>
    /// <param name="sprite"></param>
    public void SetSprite(Sprite sprite)
    {
        GetComponents();
        if (imageRenderer) imageRenderer.sprite = sprite;
        else spriteRenderer.sprite = sprite;
    }

    #region Animations

    /// <summary>
    /// Animacion completa de aparicion y desaparicion del emote
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimateTheWholeThing() {
        AnimationFocus();
        AnimationMove();
        yield return new WaitForSeconds(timeFloating);
        AnimationBlur();
        yield return new WaitForSeconds(blurTimeFade);
        if(gameObject) Destroy(gameObject);
    }

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
        DoAnimation(AnimationZoom(0, 1, TweenScaleFunctions.SineEaseIn));
    }

    /// <summary>
    /// Animar la desaparicion del trigger
    /// </summary>
    public void AnimationHide()
    {
        gameObject.SetActive(true);
        AnimationBlur();
        DoAnimation(AnimationZoom(1, 0, TweenScaleFunctions.SineEaseOut));
    }

    /// <summary>
    /// Aparicion del trigger de finta
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    public IEnumerator AnimationZoom(float from, float to, Func<float, float> scaleFunction)
    {

        System.Action<ITween<float>> zoomInOut = (t) =>
        {
            if (gameObject != null) gameObject.transform.localScale = new Vector3(t.CurrentValue, t.CurrentValue, t.CurrentValue);
        };

        // Que ataque aparezca
        gameObject.Tween(string.Format("ZoomIn{0}", gameObject.GetInstanceID()), from, to,
            zoomTime, scaleFunction, zoomInOut);

        yield return null;

    }

    /// <summary>
    /// Animar cuando el jugador se posa sobre el elemento
    /// </summary>
    public void AnimationFocus()
    {
        DoAnimation(AnimationFade(blurColor, focusColor, TweenScaleFunctions.Linear));
    }

    /// <summary>
    /// Animar cuando el jugador se posa fuera del elemento
    /// </summary>
    public void AnimationBlur()
    {
        Color initialColor = imageRenderer != null ? imageRenderer.color : spriteRenderer.color;
        DoAnimation(AnimationFade(initialColor, blurColor, TweenScaleFunctions.Linear));
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
            if (spriteRenderer != null) spriteRenderer.color = t.CurrentValue;
        };

        // Que ataque aparezca
        int gmID = imageRenderer != null ? imageRenderer.GetInstanceID() : spriteRenderer.GetInstanceID();
        fadeAnimation = gameObject.Tween(string.Format("zoom{0}", gmID), from, to,
            blurTimeFade, scaleFunction, fadeFn);

        yield return null;

    }

    /// <summary>
    /// Mover el elemento a una nueva posicion
    /// </summary>
    public void AnimationMove()
    {
        DoAnimation(AnimationMoveTo());
    }

    /// <summary>
    /// Mueve el emote en una direccion
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimationMoveTo()
    {
        // Obtener la orientacion, rotacion y direccion del emote
        // En caso de que sea parte del mundo, se debe ver la orientacion del PJ asociado
        int direction = isPartOfUI? (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1) : directionOnCharacter;
        float speed = isPartOfUI ? UnityEngine.Random.Range(minSpeed, maxSpeed) : speedOnWorld;

        float lastSpeed = speed * 0.25f;
        float rotation = UnityEngine.Random.Range(0, maxRotation) * -direction;

        // Rotar el elemento
        transform.Rotate(0, 0, rotation);

        System.Action<ITween<float>> moveIn = (t) =>
        {
            if (gameObject != null && transform != null)
            {
                float xSpeed = t.CurrentValue * direction;
                transform.localPosition = new Vector2( transform.localPosition.x + xSpeed, transform.localPosition.y + t.CurrentValue );
            }
        };

        gameObject.Tween(string.Format("Move{0}", gameObject.GetInstanceID()), speed, lastSpeed,
            timeMoving, TweenScaleFunctions.QuadraticEaseInOut, moveIn);

        yield return null;

    }

    #endregion
}