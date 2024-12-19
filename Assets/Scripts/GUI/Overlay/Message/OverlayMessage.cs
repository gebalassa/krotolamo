using System.Collections;
using UnityEngine;
using TMPro;

public class OverlayMessage : OverlayObject
{
    [SerializeField] TextMeshProUGUI message;

    protected override void Start()
    {
        SceneController sceneController = FindObjectOfType<SceneController>();
        if (sceneController) backgroundEnvironment = sceneController.GetCurrentBackgroundEnvironment();
        base.Start();
        UpdateCurrentFont();
    }

    /// <summary>
    /// Mostrar el mensaje en pantalla
    /// </summary>
    /// <param name="table">Tabla de la cual debe ser extraido el mensaje</param>
    /// <param name="key">Llave asociada al mensaje</param>
    public void Localize(string table, string key) {
        LocalizationHelper.Translate(message, table, key);
    }

    /// <summary>
    ///  Actualizacion de la fuente que se esta usando segun idioma
    /// </summary>
    protected virtual void UpdateCurrentFont()
    {
        // Fuente para TMPro
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        message.font = mainFont;

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        message.fontStyle = style;
    }

}