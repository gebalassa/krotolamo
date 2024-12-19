﻿using DigitalRuby.Tween;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class PJISelectorllustration : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Image illustration;

    [Header("Animation")]
    [SerializeField] [Range(0, 100)] float deltaX = 40;
    [SerializeField] [Range(0, 2)] float fadeTime = .25f;

    // Reutilizable
    CanvasGroup canvasGroup;
    RectTransform rectTransform;

    // Materiales para ilustraciones y elementos
    private Material normalMaterial;
    private Material blackAndWhiteMaterial;

    /// <summary>
    /// Copia del material asociado a la imagen
    /// </summary>
    private void Awake()
    {
        normalMaterial = illustration.material;
        blackAndWhiteMaterial = new Material(illustration.material);
        blackAndWhiteMaterial.SetFloat("_GrayscaleAmount", 1);
    }

    /// <summary>
    /// Cambio en el sprite de la ilustracion
    /// </summary>
    /// <param name="newSprite"></param>
    public void SetIllustration(Sprite newSprite)
    {
        illustration.sprite = newSprite;
    }

    /// <summary>
    /// Animacion de aparicion del personaje
    /// </summary>
    /// <param name="fromRight">Direccion desde donde sale el personaje</param>
    public void Show(bool fromRight = true)
    {
        CheckRectTransform();
        CheckCanvasGroup();

        // Mover
        System.Action<ITween<float>> updatePosition = (t) =>
        {
            if (rectTransform) rectTransform.localPosition = new Vector3(t.CurrentValue, rectTransform.localPosition.y, rectTransform.localPosition.z);
        };

        // Mostrar
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        float initialX = fromRight ? deltaX : -deltaX;

        // Hacer fade in y mostrar elemento
        gameObject.Tween(string.Format("Move{0}", GetInstanceID()), initialX, 0, fadeTime, TweenScaleFunctions.QuadraticEaseOut, updatePosition);
        gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), 0, 1, fadeTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha);
    }

    /// <summary>
    ///  Animacion de ocultar (y destruir) al personaje
    /// </summary>
    /// <param name="toLeft">Direccion hacia donde se enconde el personaje</param>
    /// <param name="destroy">Determina si debe o no ser destruido el objeto posterior a la animacion</param>
    public void Hide(bool toLeft = true, bool destroy = true)
    {
        CheckRectTransform();
        CheckCanvasGroup();

        // Mover
        System.Action<ITween<float>> updatePosition = (t) =>
        {
            if (rectTransform) rectTransform.localPosition = new Vector3(t.CurrentValue, rectTransform.localPosition.y, rectTransform.localPosition.z);
        };

        // Ocultar
        System.Action<ITween<float>> updateAlpha = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        System.Action<ITween<float>> updateAlphaComplete = (t) =>
        {
            if (gameObject) Destroy(gameObject);
        };

        float finalX = toLeft ? -deltaX : deltaX;

        // Hacer fade out y mostrar elemento
        gameObject.Tween(string.Format("Move{0}", GetInstanceID()), 0, finalX, fadeTime, TweenScaleFunctions.QuadraticEaseOut, updatePosition);
        gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), 1, 0, fadeTime, TweenScaleFunctions.QuadraticEaseOut, updateAlpha, updateAlphaComplete);
    }

    /// <summary>
    /// Revision de si ya esta identificado el componente RectTransform
    /// </summary>
    void CheckRectTransform()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Revision de si ya esta identificado el componente CanvasGroup
    /// </summary>
    void CheckCanvasGroup()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Indicar si imagen debe estar en blanco y negro
    /// </summary>
    public virtual void UpdateBlackAndWhite(bool blackAndWhite)
    {
        illustration.material = blackAndWhite ? blackAndWhiteMaterial : normalMaterial;
    }
}