using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;

public class UIGameplayText : MonoBehaviour
{
    // Componentes
    Text textComponent;

    // Para animacion de fade
    float timeFade = 0.25f;

    // Colores a utilizar
    Color lastColor = Color.white;
    Color hideColor;

    /// <summary>
    /// Obtencion del componente Text asociado
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
        if (textComponent) return;
        textComponent = GetComponent<Text>();
    }

    /// <summary>
    /// Animar la aparicion del elemento
    /// </summary>
    public void Show()
    {
        CheckComponent();
        if (!textComponent) return;

        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if(textComponent) textComponent.color = t.CurrentValue;
        };

        gameObject.Tween(string.Format("FadeIn{0}", textComponent.GetInstanceID()), textComponent.color, lastColor, timeFade, TweenScaleFunctions.QuadraticEaseOut, updateColor);

    }

    /// <summary>
    /// Ocultar la imagen y guardar el ultimo color que tiene el elemento
    /// </summary>
    public void Hide()
    {
        CheckComponent();
        if (!textComponent) return;

        // Si no esta establecido el color de hide, es momento de calcularlo
        if (hideColor == null)
        {
            hideColor = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 0);
        }
        lastColor = textComponent.color;

        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if(textComponent) textComponent.color = t.CurrentValue;
        };

        gameObject.Tween(string.Format("FadeOut{0}", textComponent.GetInstanceID()), lastColor, hideColor, timeFade, TweenScaleFunctions.QuadraticEaseOut, updateColor);
    }

}
