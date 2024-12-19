using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;

public class UIGameplayCanvas : MonoBehaviour
{
    // Componentes
    CanvasGroup canvasGroupComponent;

    // Para animacion de fade
    float timeFade = 0.25f;

    /// <summary>
    /// Obtencion del componente Image asociado
    /// </summary>
    private void Start()
    {
        CheckComponent();
    }

    /// <summary>
    /// Obtencion del elemento
    /// </summary>
    private void CheckComponent()
    {
        if (canvasGroupComponent) return;
        canvasGroupComponent = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Animar la aparicion del elemento
    /// </summary>
    public void Show()
    {
        CheckComponent();
        if (!canvasGroupComponent) return;

        System.Action<ITween<float>> updateColor = (t) =>
        {
            if(canvasGroupComponent) canvasGroupComponent.alpha = t.CurrentValue;
        };

        gameObject.Tween(string.Format("FadeIn{0}", canvasGroupComponent.GetInstanceID()), 0, 1, timeFade, TweenScaleFunctions.QuadraticEaseOut, updateColor);

    }

    /// <summary>
    /// Ocultar la imagen y guardar el ultimo color que tiene el elemento
    /// </summary>
    public void Hide()
    {
        CheckComponent();
        if (!canvasGroupComponent) return;

        System.Action<ITween<float>> updateColor = (t) =>
        {
            if(canvasGroupComponent) canvasGroupComponent.alpha = t.CurrentValue;
        };

        gameObject.Tween(string.Format("FadeOut{0}", canvasGroupComponent.GetInstanceID()), 1, 0, timeFade, TweenScaleFunctions.QuadraticEaseOut, updateColor);
    }

}
