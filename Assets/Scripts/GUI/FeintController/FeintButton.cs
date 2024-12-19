using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;

public class FeintButton : MonoBehaviour
{
    [SerializeField] FeintController feintController;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Attacks attack;

    [Header("Move Animation")]
    [SerializeField] Vector2 hidePosition;
    [SerializeField] Vector2 showPosition;

    [Header("Focus/Blur Animation")]
    [SerializeField] Color focusColor = Color.white;
    [SerializeField] Color blurColor = new Color(1, 1, 1, 0.5f);
    [SerializeField] [Range(0, 1f)] float blurTimeFade = 0.5f;

    [Header("Show/Hide Animation")]
    [SerializeField] [Range(0, 1f)] float showHideTimeToggle = 0.1f;
    [SerializeField] [Range(0, 1f)] float showScale = 1f;
    [SerializeField] [Range(0, 1f)] float hideScale = .6f;

    // Animaciones
    ColorTween fadeAnimation;
    ColorTween showHideAnimation;
    ColorTween showHideScaleAnimation;

    // Utiles
    bool canBeSelected = false;

    /// <summary>
    /// Al ingresar al boton, se envia la senal de ataque seleccionado
    /// </summary>
    private void OnMouseEnter()
    {
        if (!canBeSelected) return;
        if (feintController) feintController.SelectAttack(attack,false);
        AnimationFocus();
    }

    /// <summary>
    /// En caso de que se salga del objeto, se debe quitar la seleccion
    /// </summary>
    private void OnMouseExit()
    {
        if (!canBeSelected) return;
        if (feintController) feintController.DeSelectAttack();
        AnimationBlur();
    }

    /// <summary>
    /// Al presionar el boton, se envia la senal de ataque seleccionado y fijado
    /// </summary>
    private void OnMouseDown()
    {
        if (!canBeSelected) return;
        if (feintController) feintController.SelectAttack(attack,true);
    }

    /// <summary>
    /// Cambiar el valor de la propiedad que permite la seleccion de finta
    /// </summary>
    /// <param name="value"></param>
    public void SetCanBeSelected(bool value)
    {
        canBeSelected = value;
    }

    #region Animations

    /// <summary>
    /// Realizar corutina especificada
    /// </summary>
    /// <param name="coroutine">Rutina a realizar</param>
    private void DoAnimation(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// Animar cuando el jugador se posa sobre el elemento
    /// </summary>
    public void AnimationFocus()
    {
        DoAnimation(AnimationFade(spriteRenderer.color, focusColor, TweenScaleFunctions.Linear));
    }

    /// <summary>
    /// Animar cuando el jugador se posa fuera del elemento
    /// </summary>
    public void AnimationBlur()
    {
        DoAnimation(AnimationFade(spriteRenderer.color, blurColor, TweenScaleFunctions.Linear));
    }

    /// <summary>
    /// Animacion de fade para elemento 
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    /// <param name="scaleFunction">Funcion para tiempos de animacion</param>
    private IEnumerator AnimationFade(Color from, Color to, Func<float, float> scaleFunction)
    {
        if (fadeAnimation != null) fadeAnimation.Stop(TweenStopBehavior.DoNotModify);

        System.Action<ITween<Color>> zoomFn = (t) =>
        {
            if (spriteRenderer != null) spriteRenderer.color = t.CurrentValue;
        };

        // Que ataque aparezca
        fadeAnimation = gameObject.Tween(string.Format("zoom{0}", spriteRenderer.GetInstanceID()), from, to,
            blurTimeFade, scaleFunction, zoomFn);

        yield return null;

    }

    /// <summary>
    /// Animar la aparicion del boton
    /// </summary>
    public void AnimationShow()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        Vector3 finalScale = new Vector3(showScale, showScale, showScale);
        DoAnimation(AnimationShowHide(transform.localPosition, showPosition, transform.localScale, finalScale, true, TweenScaleFunctions.Linear));
    }

    /// <summary>
    /// Animar la desaparicion del boton
    /// </summary>
    public void AnimationHide()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        Vector3 finalScale = new Vector3(hideScale, hideScale, hideScale);
        DoAnimation(AnimationShowHide(transform.localPosition, hidePosition, transform.localScale, finalScale, false, TweenScaleFunctions.Linear));
    }

    /// <summary>
    /// Aparicion del boton de ataque
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    private IEnumerator AnimationShowHide(Vector2 from, Vector2 to, Vector3 fromScale, Vector3 toScale, bool finalActiveStatus, Func<float, float> scaleFunction)
    {
        if (showHideAnimation != null) showHideAnimation.Stop(TweenStopBehavior.DoNotModify);
        if (showHideScaleAnimation != null) showHideScaleAnimation.Stop(TweenStopBehavior.DoNotModify);

        // Impedir la seleccion de ataque si se estan ocultando
        if (!finalActiveStatus) canBeSelected = false;

        System.Action<ITween<Vector2>> showHideFn = (t) =>
        {
            if (gameObject != null && gameObject.activeSelf) gameObject.transform.localPosition = t.CurrentValue;
        };

        System.Action<ITween<Vector2>> showHideFnEnd = (t) =>
        {
            if (gameObject != null)
            {
                gameObject.SetActive(finalActiveStatus);
                canBeSelected = finalActiveStatus;
                spriteRenderer.color = focusColor;
            }
        };

        System.Action<ITween<Vector3>> changeScale = (t) =>
        {
            if (gameObject != null && gameObject.activeSelf) gameObject.transform.localScale = t.CurrentValue;
        };

        // Que ataque aparezca
        gameObject.Tween(string.Format("showHide{0}", gameObject.GetInstanceID()), from, to,
            showHideTimeToggle, scaleFunction, showHideFn, showHideFnEnd);

        // Modificar el tamaño del elemento
        gameObject.Tween(string.Format("scale{0}", gameObject.GetInstanceID()), fromScale, toScale,
            showHideTimeToggle, scaleFunction, changeScale);

        yield return null;

    }

    #endregion
}