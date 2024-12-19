using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JanKenShopContentCard : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] string titleText;
    [SerializeField] bool useLocalization;
    [SerializeField] string localizationTable;
    [SerializeField] string localizationKey;

    [Header("Elements")]
    [SerializeField] TextMeshProUGUI title;

    // Componentes recurrentes
    protected Button button; 

    // Use this for initialization
    protected virtual void Start()
    {
        // Asignar titulo
        if(titleText != "") title.text = titleText;

        // Obtener el boton
        button = GetComponent<Button>();

        // Ver si es necesario localizar el titulo
        if (useLocalization) Localize();
    }

    /// <summary>
    /// Actualizacion de los distintos elementos al idioma seleccionado por el jugador
    /// </summary>
    protected virtual void Localize()
    {
        // Actualizar la fuente
        UpdateCurrentFont();
        LocalizationHelper.Translate(title, localizationTable, localizationKey);
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        title.font = mainFont;

        // Cambiar el material
        Material mainMaterial = FontManager._mainManager.GetMainMaterial(FontManager.MainFontMaterial.White);
        title.fontSharedMaterial = mainMaterial;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        title.fontStyle = style;
    }
}