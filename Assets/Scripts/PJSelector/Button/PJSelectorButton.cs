using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PJSelectorButton : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Button button;
    [SerializeField] Text text;
    [SerializeField] Image characterAvatar;
    [SerializeField] GameObject overlaySelector;

    // Use this for initialization
    void Start()
    {
        button.onClick.AddListener(ShowPJSelector);
        PJSelectorOverlay.onValidSelectionDelegate += OnSelectCharacter;

        // Actualizar texto segun idioma
        Localize();

        // Iniciar el primer cambio de personaje
        StartCoroutine(SetInitialCharacter());

        // Evento de cambio de idioma
        LanguageController.onLanguageChangeDelegate += Localize;
    }

    /// <summary>
    /// Localizacion del texto de cambio de personaje
    /// </summary>
    private void Localize()
    {
        LocalizationHelper.Translate(text, JankenUp.Localization.tables.Options.tableName, JankenUp.Localization.tables.Options.Keys.character);
        UpdateCurrentFont();
    }

    /// <summary>
    /// Actualizacion de la fuente del mensaje y de la localizacion del texto
    /// </summary>
    public void UpdateCurrentFont()
    {
        Font plainFont = FontManager._mainManager.GetPlainFont();
        text.font = plainFont;

        FontStyle fontStyle = FontManager._mainManager.IsBold() ? FontStyle.Bold : FontStyle.Normal;
        text.fontStyle = fontStyle;
    }

    /// <summary>
    /// Realiza la apertura del overlay de selecciona de PJ
    /// </summary>
    private void ShowPJSelector()
    {
        // Obtener la escena actual
        SceneController sceneController = FindObjectOfType<SceneController>();
        sceneController.SwitchUI(OpenPJSelector());
    }

    /// <summary>
    /// Apertura del selector de personajes
    /// </summary>
    /// <returns></returns>
    public IEnumerator OpenPJSelector()
    {
        PJSelectorOverlay selector = Instantiate(overlaySelector).GetComponent<PJSelectorOverlay>();
        yield return null;
    }

    /// <summary>
    /// Se realizo el cambio de personaje. Debe actualizarse el avatar
    /// </summary>
    /// <param name="characterConfiguration"></param>
    void OnSelectCharacter(CharacterConfiguration characterConfiguration)
    {
        characterAvatar.sprite = characterConfiguration.GetAvatar();
    }

    /// <summary>
    /// Solicitud para cambiar al personaje seleccionado en un inicio
    /// </summary>
    /// <returns></returns>
    IEnumerator SetInitialCharacter()
    {
        string identifier = GameController.Load().characterIdentifier;

        // Si esta desbloqueado, continuar. De lo contrario, seleccionar al primero de la lista// Obtener los personajes
        List<GameObject> charactersAvailables = CharacterPool.Instance.GetAvailables();
        GameObject selectedCharacter = charactersAvailables.Find(c => c.GetComponent<CharacterConfiguration>().GetIdentifier() == identifier);
        CharacterConfiguration characterConfiguration = selectedCharacter != null ? selectedCharacter.GetComponent<CharacterConfiguration>() : charactersAvailables[0].GetComponent<CharacterConfiguration>();
        OnSelectCharacter(characterConfiguration);
        yield return null;
    }

    /// <summary>
    /// Quitar los eventos suscritos
    /// </summary>
    private void OnDestroy()
    {
        PJSelectorOverlay.onValidSelectionDelegate -= OnSelectCharacter;
        LanguageController.onLanguageChangeDelegate -= Localize;
    }

}