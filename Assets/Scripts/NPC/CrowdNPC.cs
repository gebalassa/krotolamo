using DigitalRuby.Tween;
using System.Collections;
using UnityEngine;

public class CrowdNPC : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] string npcIdentifier = "";

    [Header("Animation")]
    [SerializeField] bool animatedByCode = false;
    [SerializeField] Sprite[] animationFrames;
    [SerializeField][Range(0,1)] float timePerFrame = 0.5f;
    [SerializeField][Range(0,1)] float timeToHide = 0.2f;

    SpriteRenderer spriteRenderer;
    Coroutine animationCoroutine;
    CanvasGroup canvasGroup;
    ITween fadeOutTween;

    protected void Start()
    {
        GetSpriteRenderer();
    }

    /// <summary>
    /// Cambio del color aplicado al componente SpriteRenderer del objeto
    /// </summary>
    /// <param name="newColor"></param>
    public void ChangeColor(Color newColor)
    {
        GetSpriteRenderer();
        spriteRenderer.color = newColor;
    }

    /// <summary>
    /// Obtencion del componente asociado
    /// </summary>
    protected void GetSpriteRenderer()
    {
        if (spriteRenderer != null) return;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Obtencion del canvas group
    /// </summary>
    protected void GetCanvasGroup()
    {
        if (canvasGroup != null) return;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Obtencion del identificador del NPC
    /// </summary>
    /// <returns></returns>
    public string GetIdentifier()
    {
        return npcIdentifier;
    }

    /// <summary>
    /// Animacion de personaje
    /// </summary>
    /// <returns></returns>
    IEnumerator Animate()
    {
        GetSpriteRenderer();
        int index = 0;
        while (true)
        {
            spriteRenderer.sprite = animationFrames[index++];
            if (index >= animationFrames.Length) index = 0;
            yield return new WaitForSeconds(timePerFrame);
        }
    }

    /// <summary>
    /// Iniciar la animacion por codigo si asi se establecio
    /// </summary>
    private void OnEnable()
    {
        if (!animatedByCode || animationFrames.Length == 0) return;
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(Animate());
    }

    /// <summary>
    /// Detener la animacion por codigo
    /// </summary>
    private void OnDisable()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        if (fadeOutTween != null) fadeOutTween.Stop(TweenStopBehavior.Complete);
    }

    /// <summary>
    /// Se oculta el personaje en base al canvasGroup
    /// </summary>
    public void Hide()
    {
        GetSpriteRenderer();
        if (fadeOutTween != null) fadeOutTween.Stop(TweenStopBehavior.Complete);

        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if (spriteRenderer != null) spriteRenderer.color = t.CurrentValue;
        };

        Color finalColor = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);

        // Hacer fade out
        fadeOutTween = gameObject.Tween(string.Format("FadeOut{0}", gameObject.GetInstanceID()), spriteRenderer.color, finalColor, timeToHide, TweenScaleFunctions.QuadraticEaseOut, updateColor);
    }
}