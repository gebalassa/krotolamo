using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;

public class LevelDisplayer : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] Sprite spriteNone;
    [SerializeField] Sprite spriteUp;
    [SerializeField] Sprite spriteDown;

    [Header("Level")]
    [SerializeField] Text levelLabel;

    [Header("Colors")]
    [SerializeField] Color colorUp;
    [SerializeField] Color colorDown;

    [Header("Positions")]
    [SerializeField] float yHide = 20f;
    [SerializeField] float yShow = -66.5f;
    float distance = 0f;
    float minDistance = 10f;
    float timeToShow = 0.1f;
    Vector2 vectorHide;
    Vector2 vectorShow;

    // Componentes
    Image imageRenderer;
    RectTransform rectTransform;
    GameObject newRecordIndicator;

    // Configuraciones para nuevo record
    float showNewRecordTime = .4f;

    // Guardar referencia a la actual rutina de mostrar/ocultar displayer
    Coroutine showHideCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        imageRenderer = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        newRecordIndicator = transform.Find("NewRecord").gameObject;

        // Calcular la distancia entre las 2 posiciones
        vectorHide = new Vector2(rectTransform.anchoredPosition.x, yHide);
        vectorShow = new Vector2(rectTransform.anchoredPosition.x, yShow);
        distance = Vector2.Distance(vectorShow, vectorHide);
    }

    // Indicar el nivel y si se va subiendo o no
    public void UpdateDisplayer(int level, bool isGoingUp, bool noneState, bool newRecord)
    {
        levelLabel.text = level.ToString();
        if(!imageRenderer) imageRenderer = GetComponent<Image>();

        if (!noneState)
        {
            levelLabel.color = isGoingUp ? colorUp : colorDown;
            imageRenderer.color = isGoingUp ? colorUp : colorDown;
            imageRenderer.sprite = isGoingUp ? spriteUp : spriteDown;
        }
        else
        {
            imageRenderer.sprite = spriteNone;
        }

        // Indicar si es un nuevo nivel
        if (newRecord) ShowNewRecord();

    }

    // Indicar el nombre del nivel
    public void UpdateDisplayer(string name)
    {
        levelLabel.text = name;
    }

    public Text GetDisplayer()
    {
        return levelLabel;
    }

    // Mostrar el nuevo nivel, además de indicar si se esta subiendo o bajando
    public void Show()
    {
        if (showHideCoroutine != null) StopCoroutine(showHideCoroutine);
        showHideCoroutine = StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        // Si la distancia es practicamente igual, terminar el movimiento
        while (Vector2.Distance(rectTransform.anchoredPosition, vectorShow) > minDistance)
        {

            // Recalculo de la distancia
            distance = Vector2.Distance(vectorShow, rectTransform.anchoredPosition);

            // Calculo de la velocidad
            float step = (distance / timeToShow) * Time.deltaTime;

            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                vectorShow,
                step
                );

            yield return null;
        }

        // Terminar de centrar la ubicación
        rectTransform.anchoredPosition = new Vector3(
            rectTransform.anchoredPosition.x,
            yShow
            );

    }

    // Ocultar el displayer
    public void Hide()
    {
        if (showHideCoroutine != null) StopCoroutine(showHideCoroutine);
        showHideCoroutine = StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        // Si la distancia es practicamente igual, terminar el movimiento
        while (Vector2.Distance(rectTransform.anchoredPosition, vectorHide) > minDistance)
        {

            // Recalculo de la distancia
            distance = Vector2.Distance(vectorHide, rectTransform.anchoredPosition);

            // Calculo de la velocidad
            float step = (distance / timeToShow) * Time.deltaTime;

            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                vectorHide,
                step
                );

            yield return null;
        }

        // Terminar de centrar la ubicación
        rectTransform.anchoredPosition = new Vector3(
            rectTransform.anchoredPosition.x,
            yHide
            );

    }

    // Mostrar indicador de nuevo record
    private void ShowNewRecord()
    {
        if (!newRecordIndicator) newRecordIndicator = transform.Find("NewRecord").gameObject;
        newRecordIndicator.SetActive(true);

        Image image = newRecordIndicator.GetComponent<Image>();
        Text textGUI = newRecordIndicator.transform.Find("Text").GetComponent<Text>();

        // Realizar aparicion del ataque
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if(image) image.color = t.CurrentValue;
            if(textGUI) textGUI.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> updateColorComplete = (t) =>
        {
            if(newRecordIndicator) newRecordIndicator.SetActive(false);
        };

        // Hacer fade in fadeout
        textGUI.gameObject.Tween(string.Format("FadeInMove{0}", newRecordIndicator.GetInstanceID()), Color.clear, Color.white, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateColor)
            .ContinueWith(new ColorTween().Setup(Color.white, Color.clear, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateColor))
            .ContinueWith(new ColorTween().Setup(Color.clear, Color.white, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateColor))
            .ContinueWith(new ColorTween().Setup(Color.white, Color.clear, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateColor, updateColorComplete));

    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        Font plainFont = FontManager._mainManager.GetPlainFont();
        levelLabel.font = plainFont;

        FontStyle style = FontManager._mainManager.IsBold() ? FontStyle.Bold : FontStyle.Normal;
        levelLabel.fontStyle = style;
    }

}
