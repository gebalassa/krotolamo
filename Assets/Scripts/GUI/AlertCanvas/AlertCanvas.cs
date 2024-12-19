using DigitalRuby.Tween;
using System.Collections;
using UnityEngine;

public class AlertCanvas : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] [Range(0, 1)] float maxValue = .50f;
    [SerializeField] [Range(0, 2)] float blinkTime = 1f;

    // Utiles
    Coroutine blinkCoroutine;
    Tween<float> blinkTween;

    /// <summary>
    /// Llamado para iniciar parpadeo
    /// </summary>
    public void Blink() {
        if (blinkCoroutine != null) return;
        blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    /// <summary>
    /// Corutina de parpadeo
    /// </summary>
    /// <returns></returns>
    private IEnumerator BlinkCoroutine()
    {
        bool even = true;
        while (gameObject != null)
        {
            if (blinkTween != null) blinkTween.Stop(TweenStopBehavior.Complete);
            System.Action<ITween<float>> updateAlpha = (t) =>
            {
                if (canvasGroup != null) canvasGroup.alpha = t.CurrentValue;
            };

            blinkTween = gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), even? 0 : maxValue, even? maxValue : 0,
                    blinkTime, TweenScaleFunctions.QuadraticEaseInOut, updateAlpha);

            yield return new WaitForSeconds(blinkTime);
            even = !even;
        }
    }

    /// <summary>
    /// Detiene el proceso de parpadeo
    /// </summary>
    public void Stop() {
        if(blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            if (blinkTween != null) blinkTween.Stop(TweenStopBehavior.DoNotModify);
            if (canvasGroup)
            {
                System.Action<ITween<float>> updateAlpha = (t) =>
                {
                    if (canvasGroup != null) canvasGroup.alpha = t.CurrentValue;
                };
                blinkTween = gameObject.Tween(string.Format("Fade{0}", GetInstanceID()), canvasGroup.alpha, 0,
                    blinkTime, TweenScaleFunctions.QuadraticEaseInOut, updateAlpha);
            }
        }
    }

    /// <summary>
    /// Detener coroutina de estar activa
    /// </summary>
    private void OnDestroy()
    {
        Stop();
    }
}