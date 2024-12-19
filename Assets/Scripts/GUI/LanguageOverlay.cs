using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalRuby.Tween;

public class LanguageOverlay : MonoBehaviour
{
    [Header("Object")]
    [SerializeField] LanguageController languageController;

    [Header("Times")]
    [SerializeField] float timeToFade = .5f;
    [SerializeField] float timeToChange = 3f;

    // Utiles
    CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        StartCoroutine(Show());
    }

    /// <summary>
    /// Aparicion de todos los elementos
    /// </summary>
    /// <returns></returns>
    private IEnumerator Show()
    {
        Fade(true);
        yield return null;
    }

    /// <summary>
    /// Fadein del overlay
    /// </summary>
    /// <param name="show"></param>
    private void Fade(bool show)
    {
        System.Action<ITween<float>> updateColor = (t) =>
        {
            if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
        };

        gameObject.Tween("FadePanel", (show? 0 : 1), (show? 1 : 0), timeToFade, TweenScaleFunctions.QuadraticEaseInOut, updateColor);
    }

    /// <summary>
    /// Fadeout del elemento
    /// </summary>
    /// <returns></returns>
    private IEnumerator Hide()
    {
        Fade(false);
        yield return new WaitForSeconds(timeToFade * 1.1f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Llamada de cierre
    /// </summary>
    public void Close()
    {
        StartCoroutine(Hide());
    }

}
