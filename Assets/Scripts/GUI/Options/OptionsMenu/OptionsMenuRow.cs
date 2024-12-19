using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenuRow : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] Text text;

    [Header("Localization")]
    [SerializeField] protected bool localizationOn = true;
    [SerializeField] protected string localizationTable = "";
    [SerializeField] protected string localizationKey = "";

    // Flag para ajustar contenido
    bool pendingContentAdjustment = false;

    // Use this for initialization
    protected void Start()
    {
        if (localizationOn)
        {
            Localize();
            // Suscribirse al cambio de idioma
            LanguageController.onLanguageChangeDelegate += Localize;
        }
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        LocalizationHelper.Translate(text, localizationTable, localizationKey, action: delegate {
            if (isActiveAndEnabled) StartCoroutine(AdjustSize());
            else pendingContentAdjustment = true;
        });
        UpdateCurrentFont();
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        Font mainFont = FontManager._mainManager.GetPlainFont();
        FontStyle style = FontManager._mainManager.IsBold() ? FontStyle.Bold : FontStyle.Normal;

        text.font = mainFont;
        text.fontStyle = style;
    }

    /// <summary>
    /// Ajuste del tamano del elemento
    /// </summary>
    /// <returns></returns>
    private IEnumerator AdjustSize()
    {
        pendingContentAdjustment = false;
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Ajuste del tamano del elemento
    /// </summary>
    private void OnEnable()
    {
        if (pendingContentAdjustment) StartCoroutine(AdjustSize());
    }

    /// <summary>
    /// Quitar el evento de localizacion
    /// </summary>
    private void OnDestroy()
    {
        // Desuscribirse al cambio de idioma
        LanguageController.onLanguageChangeDelegate -= Localize;
    }
}