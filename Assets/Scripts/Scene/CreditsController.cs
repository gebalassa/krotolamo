using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CreditsController : SceneController
{
    [Header("General Setup")]
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] [Range(0,100f)]float speed = 0f;
    [SerializeField] [Range(0,1000f)]float joystickSpeed = 0f;
    [SerializeField] float WaitBeforeAutoScroll = 1f;
    [SerializeField] GameObject socialNetworksContainer;
    [SerializeField] bool showSocialNetworks = true;

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI gameDesign;
    [SerializeField] TextMeshProUGUI artDesign;
    [SerializeField] TextMeshProUGUI programmer;
    [SerializeField] TextMeshProUGUI music;
    [SerializeField] TextMeshProUGUI voices;
    [SerializeField] TextMeshProUGUI sfx;
    [SerializeField] Text specialThanks;

    [Header("Special Thanks Names")]
    [SerializeField] string[] specialThanksNames;
    [SerializeField] TextMeshProUGUI specialThanksValue;

    // Utiles para parar movimiento
    float deceleration = 0.135f;
    bool userinteract = false;

    new void Start()
    {
        base.Start();
        GameController.SetGameplayActive(false);

        if (!showSocialNetworks) socialNetworksContainer.SetActive(false);

        // Actualizar la fuente de todos los labels
        UpdateCurrentFont();

        // Asignar los nombres de los special thanks
        SpecialThanksShuffle();

        // Asignar velocidad inicial pasado el tiempo de espera
        StartCoroutine(AutoScroll());
    }

    // Mover automaticamente el scroll de nombres
    private IEnumerator AutoScroll() {
        yield return new WaitForSeconds(WaitBeforeAutoScroll);
        if(!userinteract) scrollRect.velocity = new Vector2(0, speed);
    }

    /// <summary>
    /// Indica que el usuario hizo alguna opcion sobre el scroll
    /// </summary>
    public void OnPointerDownEvent()
    {
        userinteract = true;
        scrollRect.decelerationRate = deceleration;
    }

    public void OnPointerDownEvent(BaseEventData eventData)
    {
        OnPointerDownEvent();
    }

    // Actualiza todos los elementos ligados a un translate
    protected override void Localize()
    {
        LocalizationHelper.Translate(gameDesign, JankenUp.Localization.tables.Credits.tableName, JankenUp.Localization.tables.Credits.Keys.gameDesign);
        LocalizationHelper.Translate(artDesign, JankenUp.Localization.tables.Credits.tableName, JankenUp.Localization.tables.Credits.Keys.artDesign);
        LocalizationHelper.Translate(programmer, JankenUp.Localization.tables.Credits.tableName, JankenUp.Localization.tables.Credits.Keys.programmer);
        LocalizationHelper.Translate(music, JankenUp.Localization.tables.Credits.tableName, JankenUp.Localization.tables.Credits.Keys.music);
        LocalizationHelper.Translate(voices, JankenUp.Localization.tables.Credits.tableName, JankenUp.Localization.tables.Credits.Keys.voices);
        LocalizationHelper.Translate(sfx, JankenUp.Localization.tables.Credits.tableName, JankenUp.Localization.tables.Credits.Keys.sounds);
        LocalizationHelper.Translate(specialThanks, JankenUp.Localization.tables.Credits.tableName, JankenUp.Localization.tables.Credits.Keys.specialThanks);
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetMainFont();
        gameDesign.font = mainFont;
        artDesign.font = mainFont;
        programmer.font = mainFont;
        music.font = mainFont;
        voices.font = mainFont;
        sfx.font = mainFont;
        specialThanks.font = FontManager._mainManager.GetPlainFont();

        // Cambiar el estilo
        FontStyles style = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;

        gameDesign.fontStyle = style;
        artDesign.fontStyle = style;
        programmer.fontStyle = style;
        music.fontStyle = style;
        voices.fontStyle = style;
        sfx.fontStyle = style;
        specialThanks.fontStyle = FontManager._mainManager.IsBold() ? FontStyle.Bold : FontStyle.Normal;
    }

    // Cambiar el orden de los nombres en creditos
    private void SpecialThanksShuffle()
    {
        // Desordenar los elementos
        string temp;
        for (int i = 0; i < specialThanksNames.Length; i++)
        {
            int rnd = Random.Range(0, specialThanksNames.Length);
            temp = specialThanksNames[rnd];
            specialThanksNames[rnd] = specialThanksNames[i];
            specialThanksNames[i] = temp;
        }

        // Agregar al value
        string value = "";
        foreach (string name in specialThanksNames)
        {
            value += string.Format("{0}\n", name);
        }

        specialThanksValue.text = value;
    }

    // Abrir URL
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    #region Joystick Support
    /// <summary>
    /// Soporte para controles y acciones de jugadores
    /// </summary>
    /// <param name="action"></param>
    /// <param name="playerIndex"></param>
    protected override bool OnJoystick(JoystickAction action, int playerIndex)
    {
        if (!base.OnJoystick(action, playerIndex)) return false;
        OnPointerDownEvent();
        switch (action)
        {
            case JoystickAction.Up:
                scrollRect.velocity = new Vector2(0, -joystickSpeed);
                break;
            case JoystickAction.Down:
                scrollRect.velocity = new Vector2(0, joystickSpeed);
                break;
        }

        return true;
    }
    #endregion

}
