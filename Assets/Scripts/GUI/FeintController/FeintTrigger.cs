using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;

public class FeintTrigger : MonoBehaviour
{
    [SerializeField] FeintController feintController;

    [Header("Trigger Animation")]
    [SerializeField] [Range(0, 1f)] float triggerTimeToggle = 0.5f;

    /// <summary>
    /// Al hacer click, mostrar u ocultar todas las opciones de finta
    /// </summary>
    private void OnMouseDown()
    {
        if (feintController) feintController.Toggle();
    }

    /// <summary>
    /// Al hacer release, realizar la consulta por ataque seleccionado
    /// </summary>
    private void OnMouseUp()
    {
        if (feintController) feintController.OnDragFinish();
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
    /// Animar la aparicion del trigger
    /// </summary>
    public void AnimationShow()
    {
        DoAnimation(AnimationTrigger(0, 1, TweenScaleFunctionsExtension.EaseElasticOut));
    }

    /// <summary>
    /// Animar la desaparicion del trigger
    /// </summary>
    public void AnimationHide()
    {
        DoAnimation(AnimationTrigger(1, 0, TweenScaleFunctionsExtension.EaseElasticIn));
    }

    /// <summary>
    /// Aparicion del trigger de finta
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    public IEnumerator AnimationTrigger(float from, float to, Func<float,float> scaleFunction)
    {

        System.Action<ITween<float>> zoomInOut = (t) =>
        {
            if (gameObject != null) gameObject.transform.localScale = new Vector3(t.CurrentValue, t.CurrentValue, t.CurrentValue);
        };

        // Que ataque aparezca
        gameObject.Tween(string.Format("ZoomIn{0}", gameObject.GetInstanceID()), from, to,
            triggerTimeToggle, scaleFunction, zoomInOut);

        yield return null;

    }

    #endregion

}