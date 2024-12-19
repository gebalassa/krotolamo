using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameModeCard : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] public GameMode gameMode;
    [SerializeField] string originalTitle;
    [SerializeField] protected GameObject mainContainer;
    [SerializeField] protected float initialMove = -40;
    [SerializeField] [Range(0.1f, 1f)] protected float timeToMove = .5f;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Image illustration;

    // Animaciones
    Tween<float> fadeTween;
    Tween<float> moveTween;

    private void Start()
    {
        StartCoroutine(ShowCoroutine());
        // Suscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
        Localize();
    }

    /// <summary>
    /// Ajustes de los textos de la tarjeta
    /// </summary>
    private void Localize()
    {
        LocalizationHelper._this.TranslateThis(title, JankenUp.Localization.tables.InGame.tableName, gameMode.ToString());
        LocalizationHelper._this.TranslateThis(description, JankenUp.Localization.tables.InGame.tableName, string.Format("{0}_description", gameMode.ToString()));
        UpdateCurrentFont();
    }

    /// <summary>
    /// Actualizacion de la fuente del mensaje y de la localizacion del texto
    /// </summary>
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        Material material = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        title.font = mainFont;
        title.fontSharedMaterial = material;
        description.font = mainFont;
        description.fontSharedMaterial = material;
    }

    /// <summary>
    /// Corutina para mostrar la aparicion del card
    /// </summary>
    /// <param name="alpha">Indica si se debe ejecutar el cambio de alpha</param>
    /// <param name="movement">Indica si se debe ejecutar el cambio de posicion</param>
    /// <returns></returns>
    protected virtual IEnumerator ShowCoroutine(bool alpha = true, bool movement = true)
    {
        AdjustIllustration();
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
        if (alpha) fadeTween = gameObject.Tween(string.Format("FadeIn{0}", GetInstanceID()), 0, 1, timeToMove, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);
        if (movement) moveTween = gameObject.Tween(string.Format("Move{0}", GetInstanceID()), containerTransform.localPosition.x + initialMove, containerTransform.localPosition.x, timeToMove, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

        yield return null;
    }

    /// <summary>
    /// Ajuste de la ilustracion para mantener el tamano 16:9
    /// </summary>
    /// <returns></returns>
    protected virtual void AdjustIllustration()
    {
        float ratioX = 16f;
        float ratioY = 9f;
        float scaleFactor = 1.01f;
        int screenWidth = (int) Math.Ceiling((transform.parent as RectTransform).rect.width * scaleFactor);
        int imageHeight = (int) Math.Ceiling((transform.parent as RectTransform).rect.height * scaleFactor);
        int partSize = (int) Math.Ceiling(imageHeight / ratioY);
        int imageWidth = (int) (partSize * ratioX);
     
        if(imageWidth < screenWidth)
        {
            int diff = screenWidth - imageWidth;
            imageWidth += diff;
            imageHeight += (int)(diff / ratioX * ratioY);
        }

        illustration.rectTransform.sizeDelta = new Vector2(imageWidth, imageHeight);
    }

    /// <summary>
    /// Corutina para ocultar la aparicion del card
    /// </summary>
    /// <param name="alpha">Indica si se debe ejecutar el cambio de alpha</param>
    /// <param name="movement">Indica si se debe ejecutar el cambio de posicion</param>
    /// <returns></returns>
    protected virtual IEnumerator HideCoroutine(bool alpha = true, bool movement = true)
    {
        if (fadeTween != null) fadeTween.Stop(TweenStopBehavior.DoNotModify);
        if (moveTween != null) moveTween.Stop(TweenStopBehavior.DoNotModify);

        Transform containerTransform = mainContainer.transform;
        CanvasGroup canvas = GetComponent<CanvasGroup>();

        // Mover el contenedor
        System.Action<ITween<float>> updatePosition = (t) =>
        {
            if (containerTransform) containerTransform.localPosition = new Vector3(t.CurrentValue, containerTransform.localPosition.y, containerTransform.localPosition.z);
        };

        // Cambiar el alpha
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvas) canvas.alpha = t.CurrentValue;
        };

        System.Action<ITween<float>> onUpdateAlphaDone = (t) =>
        {
            if (gameObject != null) Destroy(gameObject);
        };

        // Hacer fade in y mostrar elemento
        if (alpha) fadeTween = gameObject.Tween(string.Format("FadeIn{0}", GetInstanceID()), 1, 0, timeToMove * 2, TweenScaleFunctions.QuadraticEaseOut, updateAlpha, onUpdateAlphaDone);
        if (movement) moveTween = gameObject.Tween(string.Format("Move{0}", GetInstanceID()), containerTransform.localPosition.x, containerTransform.localPosition.x - initialMove, timeToMove * 2, TweenScaleFunctions.QuadraticEaseOut, updatePosition);

        yield return null;
    }

    /// <summary>
    /// Eliminacion de objet
    /// </summary>
    /// <returns></returns>
    public virtual void ByeBye()
    {
        StartCoroutine(HideCoroutine());
    }

    /// <summary>
    /// Indica el nivel actual del modo
    /// </summary>
    /// <param name="level"></param>
    public void SetLevel(int level)
    {
        //title.text = string.Format("{0} (Nivel {1})", originalTitle, level);
    }

    private void OnDestroy()
    {
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;

        // Detener animaciones
        if (fadeTween != null) fadeTween.Stop(TweenStopBehavior.DoNotModify);
        if (moveTween != null) moveTween.Stop(TweenStopBehavior.DoNotModify);
    }
}