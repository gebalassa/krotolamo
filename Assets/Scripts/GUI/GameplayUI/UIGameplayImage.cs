using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;

public class UIGameplayImage : MonoBehaviour
{
    // Componentes
    Image imageComponent;

    // Para animacion de fade
    float timeFade = 0.25f;

    // Colores a utilizar
    Color lastColor = Color.white;
    Color hideColor;

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
        if (imageComponent) return;
        imageComponent = GetComponent<Image>();
    }

    /// <summary>
    /// Animar la aparicion del elemento
    /// </summary>
    public void Show()
    {
        CheckComponent();
        if (!imageComponent) return;

        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if(imageComponent) imageComponent.color = t.CurrentValue;
        };

        gameObject.Tween(string.Format("FadeIn{0}", imageComponent.GetInstanceID()), imageComponent.color, lastColor, timeFade, TweenScaleFunctions.QuadraticEaseOut, updateColor);

    }

    /// <summary>
    /// Ocultar la imagen y guardar el ultimo color que tiene el elemento
    /// </summary>
    public void Hide()
    {
        CheckComponent();
        if (!imageComponent) return;

        // Si no esta establecido el color de hide, es momento de calcularlo
        if (hideColor == null)
        {
            hideColor = new Color(imageComponent.color.r, imageComponent.color.g, imageComponent.color.b, 0);
        }
        lastColor = imageComponent.color;

        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if(imageComponent) imageComponent.color = t.CurrentValue;
        };

        gameObject.Tween(string.Format("FadeOut{0}", imageComponent.GetInstanceID()), lastColor, hideColor, timeFade, TweenScaleFunctions.QuadraticEaseOut, updateColor);
    }

}
