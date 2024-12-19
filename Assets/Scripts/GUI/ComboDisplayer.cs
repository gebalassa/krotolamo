using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;

public class ComboDisplayer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI value;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] float distance = 20f;

    // Posiciones
    Vector2 showVector;
    Vector2 hideVector;
    bool readyVectors = false;

    // Componentes
    CanvasGroup canvasGroup;
    CanvasGroup newRecordCanvasGroup;

    // Configuraciones para nuevo record
    float showNewRecordTime = .6f;

    // Indica si se esta mostrando o no
    bool isShowing = false;

    // Tween
    Tween<Vector2> tweenPosition;
    Tween<float> tweenAlpha;
    Tween<float> tweenRecord;

    private void Start()
    {
        showVector = transform.position;
        hideVector = new Vector2(transform.position.x - distance, showVector.y);
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Setear el combo actual
    public void SetValue(int combo, bool newRecord = false)
    {
        if (combo == 0) Hide();
        else
        {
            value.text = combo.ToString();
            Show();
        }

        // Indicar si es un nuevo nivel
        if (newRecord) ShowNewRecord();
    }

    // Animar la aparicion de los combos
    public void Show()
    {
        if (isShowing) return;

        if (tweenAlpha != null) tweenAlpha.Stop(TweenStopBehavior.Complete);
        if (tweenPosition != null) tweenPosition.Stop(TweenStopBehavior.Complete);
        isShowing = true;

        gameObject.SetActive(true);

        System.Action<ITween<Vector2>> updatePosition = (t) =>
        {
            if(transform) transform.position = t.CurrentValue;
        };

        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if(canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        // Obtener las posiciones
        if (!readyVectors)
        {
            showVector = transform.position;
            hideVector = new Vector2(transform.position.x - distance, showVector.y);
            readyVectors = true;
        }

        // Mover displayer
        tweenPosition = gameObject.Tween(string.Format("Show{0}", GetInstanceID()), hideVector, showVector, .5f, TweenScaleFunctions.QuadraticEaseOut, updatePosition);
        tweenAlpha = gameObject.Tween(string.Format("FadeIn{0}", value.GetInstanceID()), 0, 1, .5f, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);

    }

    // Animar la desaparicion de los combos
    public void Hide()
    {
        if (!isShowing) return;
        if (tweenAlpha != null) tweenAlpha.Stop(TweenStopBehavior.Complete);
        if (tweenPosition != null) tweenPosition.Stop(TweenStopBehavior.Complete);
        isShowing = false;

        System.Action<ITween<Vector2>> updatePosition = (t) =>
        {
            if(transform && gameObject) transform.position = t.CurrentValue;
        };

        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if(canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        System.Action<ITween<Color>> updateColorLabel = (t) =>
        {
            if(label) label.color = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> completeUpdate = (t) =>
        {
            if(gameObject) gameObject.SetActive(false);
        };

        // Mover displayer
        tweenPosition = gameObject.Tween(string.Format("Hide{0}", GetInstanceID()), showVector, hideVector, .5f, TweenScaleFunctions.QuadraticEaseOut, updatePosition, completeUpdate);
        tweenAlpha = gameObject.Tween(string.Format("FadeOut{0}", value.GetInstanceID()), 1, 0, .5f, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);
    }

    // Mostrar indicador de nuevo record
    private void ShowNewRecord()
    {
        if (!newRecordCanvasGroup)
        {
            newRecordCanvasGroup = transform.Find("NewRecord").GetComponent<CanvasGroup>();
        }

        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if(newRecordCanvasGroup) newRecordCanvasGroup.alpha = t.CurrentValue;
        };

        Color clear = new Color(1, 1, 1, 0);

        // Hacer fade in fadeout
        tweenRecord = newRecordCanvasGroup.gameObject.Tween(string.Format("FadeInMove{0}", newRecordCanvasGroup.GetInstanceID()), 0, 1, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha)
            .ContinueWith(new FloatTween().Setup(1, 0, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha))
            .ContinueWith(new FloatTween().Setup(0, 1, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha))
            .ContinueWith(new FloatTween().Setup(1, 0, showNewRecordTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha));
    }

    private void OnDestroy()
    {
        if (tweenAlpha != null) tweenAlpha.Stop(TweenStopBehavior.DoNotModify);
        if (tweenPosition != null) tweenPosition.Stop(TweenStopBehavior.DoNotModify);
        if (tweenRecord != null) tweenRecord.Stop(TweenStopBehavior.DoNotModify);
    }

}
