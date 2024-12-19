using DigitalRuby.Tween;
using System;
using System.Collections;
using UnityEngine;

public class FeintExtraItem : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] [Range(0, 1f)] float timeToggle = 0.1f;

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
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        DoAnimation(AnimationShowHide(0, 1, TweenScaleFunctions.CubicEaseIn));
    }

    /// <summary>
    /// Animar la desaparicion del trigger
    /// </summary>
    public void AnimationHide()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        DoAnimation(AnimationShowHide(1, 0, TweenScaleFunctions.CubicEaseOut));
    }

    /// <summary>
    /// Aparicion del trigger de finta
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    public IEnumerator AnimationShowHide(float from, float to, Func<float,float> scaleFunction)
    {

        System.Action<ITween<float>> zoomInOut = (t) =>
        {
            if (gameObject != null) gameObject.transform.localScale = new Vector3(t.CurrentValue, t.CurrentValue, t.CurrentValue);
        };

        // Que ataque aparezca
        gameObject.Tween(string.Format("ZoomIn{0}", gameObject.GetInstanceID()), from, to,
            timeToggle, scaleFunction, zoomInOut);

        yield return null;

    }

    #endregion

}