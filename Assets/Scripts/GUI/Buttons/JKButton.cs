using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JKButton : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] Text text;
    [SerializeField] Text extraText;
    [SerializeField] List<Image> images = new List<Image>();

    [Header("Others")]
    [SerializeField] [Range(0, 1)] float fadeTime = 0.2f;

    [Header("BWColor")]
    [SerializeField] List<Color> bwColors = new List<Color>();
    List<Color> originalColors = new List<Color>();

    // Eventos de click
    public delegate void OnClickEvent();
    public event OnClickEvent onClickDelegate;

    // Componentes
    Button button;
    CanvasGroup canvasGroup;

    // Tweens
    Tween<float> tweenFade;

    // Use this for initialization
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        UpdateCurrentFont();
    }

    /// <summary>
    /// Evento al hacer click sobre el boton
    /// </summary>
    public void OnClick()
    {
        if (onClickDelegate != null) onClickDelegate();
    }

    /// <summary>
    /// Indica si el boton es interactuable o no
    /// </summary>
    /// <param name="value"></param>
    public void SetInteractable(bool value = true)
    {
        if(!button) button = GetComponent<Button>();
        button.interactable = value;
    }

    /// <summary>
    /// Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    public void UpdateCurrentFont()
    {
        Font plainFont = FontManager._mainManager.GetPlainFont();
        FontStyle style = FontManager._mainManager.IsBold() ? FontStyle.Bold : FontStyle.Normal;

        if (text != null){
            text.font = plainFont;
            text.fontStyle = style;
        }
        if (extraText != null){
            extraText.font = plainFont;
            extraText.fontStyle = style;
        }
    }

    /// <summary>
    /// Obtencion del Text principal del boton
    /// </summary>
    /// <returns></returns>
    public Text GetText() {
        return text;
    }

    /// <summary>
    /// Obtencion del Text extra del boton
    /// </summary>
    /// <returns></returns>
    public Text GetExtraText()
    {
        return extraText;
    }

    private void OnEnable()
    {
        if(!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        Show();
    }

    /// <summary>
    /// Animacion de aparicion del boton
    /// </summary>
    public void Show()
    {
        if (tweenFade != null) tweenFade.Stop(TweenStopBehavior.Complete);

        // Mostrar
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        tweenFade = gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), 0, 1, fadeTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);
    }

    /// <summary>
    ///  Animacion de ocultar boton
    /// </summary>
    public void Hide(bool toLeft = true)
    {
        if (tweenFade != null) tweenFade.Stop(TweenStopBehavior.Complete);

        // Ocultar
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        System.Action<ITween<float>> updateAlphaComplete = (t) =>
        {
            if (gameObject) gameObject.SetActive(false);
        };

        tweenFade = gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), 1, 0, fadeTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha, updateAlphaComplete);
    }

    /// <summary>
    /// Obtencion del Button
    /// </summary>
    /// <returns></returns>
    public Button GetButton()
    {
        return button;
    }

    /// <summary>
    /// Indicar si el boton esta bloqueado o no
    /// </summary>
    /// <param name="lockValue"></param>
    public virtual void SetBW(bool lockValue)
    {
        CheckOriginalColors();

        for (int i = 0; i < images.Count; i++)
        {
            images[i].color = lockValue && i < bwColors.Count? bwColors[i] : originalColors[i];
        }
    }

    /// <summary>
    /// Guardado de colores originales
    /// </summary>
    void CheckOriginalColors()
    {
        if (originalColors.Count == 0)
        {
            // Guardar los colores originales en base a la imagenes
            foreach (Image image in images)
            {
                originalColors.Add(image.color);
            }
        }
    }
}