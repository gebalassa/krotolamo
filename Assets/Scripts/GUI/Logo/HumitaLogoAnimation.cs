using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class HumitaLogoAnimation : MonoBehaviour
{
    [Header("Letters")]
    [SerializeField] RectTransform letterH;
    [SerializeField] RectTransform letterU;
    [SerializeField] RectTransform letterM;
    [SerializeField] RectTransform letterI;
    [SerializeField] RectTransform iconCircle;
    [SerializeField] RectTransform letterT;
    [SerializeField] RectTransform iconStar;
    [SerializeField] RectTransform letterA;
    [SerializeField] RectTransform lettersContainer;
    [SerializeField] Transform lettersOverlay;

    [Header("Animation")]
    [SerializeField] [Range(0, 10)] float letterHDelay = 0;
    [SerializeField] [Range(0, 10)] float letterUDelay = .1f;
    [SerializeField] [Range(0, 10)] float letterMDelay = .3f;
    [SerializeField] [Range(0, 10)] float letterIDelay = .4f;
    [SerializeField] [Range(0, 10)] float iconCircleDelay = 1;
    [SerializeField] [Range(0, 10)] float letterTDelay = .7f;
    [SerializeField] [Range(0, 10)] float iconStarDelay = 1.1f;
    [SerializeField] [Range(0, 10)] float letterADelay = .8f;
    [SerializeField] [Range(0, 10)] float timeToMove = .6f;
    [SerializeField] [Range(0, 100)] float positionDelay = 32;
    [SerializeField] [Range(0, 1)] float initialScale = .8f;
    [SerializeField] [Range(0, 1)] float finalScale = .9f;
    [SerializeField] [Range(0, 10)] float isReadyAfterLetters = 2;
    [SerializeField] [Range(0, 10)] float timeToScale = 10;
    [SerializeField] [Range(0, 10)] float timeBeforeAnimateLogo = .25f;
    [SerializeField] [Range(0, 10)] float timeToRemove = .25f;

    // Componentes
    AudioSource audioSource;

    // Utiles
    bool readyToPlay = false;
    int maxSecondUntilPlay = 10;
    float containerHeight = 0;
    bool isPlaying = false;

    // Animaciones
    List<Tween<Vector2>> tweens = new List<Tween<Vector2>>();
    Tween<float> tweenRemove;

    // Singleton
    private void Awake()
    {
        int length = FindObjectsOfType<HumitaLogoAnimation>().Length;
        if (length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start(){}

    // Update is called once per frame
    void Update(){}

    /// <summary>
    /// Actualizacion de las letras a su ubicacion inicial
    /// </summary>
    void UpdateLettersPosition()
    {
        containerHeight = lettersContainer.rect.height;
        letterH.anchoredPosition = new Vector2(letterH.anchoredPosition.x, letterH.anchoredPosition.y - containerHeight);
        letterU.anchoredPosition = new Vector2(letterU.anchoredPosition.x, letterU.anchoredPosition.y - containerHeight);
        letterM.anchoredPosition = new Vector2(letterM.anchoredPosition.x, letterM.anchoredPosition.y - containerHeight);
        letterI.anchoredPosition = new Vector2(letterI.anchoredPosition.x, letterI.anchoredPosition.y - containerHeight);
        letterT.anchoredPosition = new Vector2(letterT.anchoredPosition.x, letterT.anchoredPosition.y - containerHeight);
        letterA.anchoredPosition = new Vector2(letterA.anchoredPosition.x, letterA.anchoredPosition.y - containerHeight);
        iconCircle.anchoredPosition = new Vector2(iconCircle.anchoredPosition.x, iconCircle.anchoredPosition.y - containerHeight);
        iconStar.anchoredPosition = new Vector2(iconStar.anchoredPosition.x, iconStar.anchoredPosition.y - containerHeight);
    }

    /// <summary>
    /// Ejecucion de la animacion del logo
    /// </summary>
    public void Play()
    {
        isPlaying = true;
        StartCoroutine(PlayCoroutine());
    }

    /// <summary>
    /// Reproduccion de la animacion
    /// </summary>
    protected virtual IEnumerator PlayCoroutine()
    {
        isPlaying = true;
        foreach (Tween<Vector2> tween in tweens) tween.Stop(TweenStopBehavior.Complete);
        lettersOverlay.SetAsLastSibling();
        UpdateLettersPosition();
        CheckComponents();
        StartCoroutine(ContinuePlaying());
        while (!readyToPlay && audioSource.clip.loadState != AudioDataLoadState.Loaded) yield return null;
        readyToPlay = true;
        yield return new WaitForSeconds(timeBeforeAnimateLogo);
        audioSource.Play();
        StartCoroutine(AnimateLetter(letterH, letterHDelay));
        StartCoroutine(AnimateLetter(letterU, letterUDelay));
        StartCoroutine(AnimateLetter(letterM, letterMDelay));
        StartCoroutine(AnimateLetter(letterI, letterIDelay));
        StartCoroutine(AnimateLetter(iconCircle, iconCircleDelay));
        StartCoroutine(AnimateLetter(letterT, letterTDelay));
        StartCoroutine(AnimateLetter(iconStar, iconStarDelay));
        StartCoroutine(AnimateLetter(letterA, letterADelay));

        // Escalar el contenedor
        System.Action<ITween<Vector2>> updateScale = (t) => {
            lettersContainer.localScale = t.CurrentValue;
        };

        tweens.Add(lettersContainer.gameObject.Tween(string.Format("Scale{0}", lettersContainer.GetInstanceID()), new Vector2(initialScale, initialScale), new Vector2(finalScale, finalScale), timeToScale, TweenScaleFunctions.Linear, updateScale));

        // Esperar el tiempo necesario para indicar que la animacion esta finalizada
        yield return new WaitForSeconds(iconStarDelay + isReadyAfterLetters);
        isPlaying = false;
    }

    /// <summary>
    /// Animacion de la letra en especifico
    /// </summary>
    /// <param name="letter"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    protected virtual IEnumerator AnimateLetter(RectTransform letter, float delay = 0){
        yield return new WaitForSeconds(delay);

        Vector2 finalPosition = new Vector2(letter.anchoredPosition.x, letter.anchoredPosition.y + containerHeight);
        Vector2 intermediatePositionFirst = new Vector2(letter.anchoredPosition.x, letter.anchoredPosition.y + containerHeight + positionDelay);
        Vector2 intermediatePositionSecond = new Vector2(letter.anchoredPosition.x, letter.anchoredPosition.y + containerHeight - positionDelay / 2);

        System.Action<ITween<Vector2>> updatePosition = (t) => {
            letter.anchoredPosition = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> updatePositionCompleteSecond = (t) => {
            tweens.Add(letter.gameObject.Tween(string.Format("ThirdMove{0}", letter.GetInstanceID()), letter.anchoredPosition, finalPosition, timeToMove / 3 * 1, TweenScaleFunctions.QuadraticEaseInOut, updatePosition));
        };

        System.Action<ITween<Vector2>> updatePositionCompleteFirst = (t) => {
            letter.SetAsLastSibling();
            tweens.Add(letter.gameObject.Tween(string.Format("SecondMove{0}", letter.GetInstanceID()), letter.anchoredPosition, intermediatePositionSecond, timeToMove / 3 * 1, TweenScaleFunctions.QuadraticEaseInOut, updatePosition, updatePositionCompleteSecond));
        };

        tweens.Add(letter.gameObject.Tween(string.Format("FirstMove{0}", letter.GetInstanceID()), letter.anchoredPosition, intermediatePositionFirst, timeToMove / 3 * 2, TweenScaleFunctions.QuadraticEaseInOut, updatePosition, updatePositionCompleteFirst));
    }

    /// <summary>
    /// En caso de error con el estado de carga del SFX
    /// </summary>
    /// <returns></returns>
    IEnumerator ContinuePlaying()
    {
        yield return new WaitForSeconds(maxSecondUntilPlay);
        readyToPlay = true;
    }

    /// <summary>
    /// Revision de componentes recurrentes
    /// </summary>
    protected virtual void CheckComponents()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Indica si la animacion se esta reproduciendo
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return isPlaying;
    }

    /// <summary>
    /// Eliminacion del elemento de animacion de logo
    /// </summary>
    public void Remove(bool force = false)
    {
        Destroy(lettersOverlay.gameObject);
        if (force && gameObject){
            Destroy(gameObject);
            return;
        }

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        System.Action<ITween<float>> completeUpdateAlpha = (t) =>
        {
            if (gameObject) Destroy(gameObject);
        };

        tweenRemove = gameObject.Tween(string.Format("FadeOut{0}", GetInstanceID()), 1, 0, timeToRemove, TweenScaleFunctions.QuadraticEaseInOut, updateAlpha, completeUpdateAlpha);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        foreach(Tween<Vector2> tween in tweens) tween.Stop(TweenStopBehavior.DoNotModify);
        if (tweenRemove != null) tweenRemove.Stop(TweenStopBehavior.DoNotModify);
    }
}