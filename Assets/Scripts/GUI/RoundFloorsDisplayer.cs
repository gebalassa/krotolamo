using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DigitalRuby.Tween;

public class RoundFloorsDisplayer : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI floorText;
    [SerializeField] TextMeshProUGUI floorLabel;

    // Guardado de animacion
    ColorTween toggleColorTween;
    float timeToToggle = .2f;
    int currentRound = 0;

    private void Start()
    {
        UpdateCurrentFont();
        LanguageController.onLanguageChangeDelegate += UpdateCurrentFont;
    }

    // Setearel valor actual del nivel / niveles requeridos
    public void SetFloor(int level, int levelsRequired)
    {
        floorText.text = string.Format("{0} / {1}", level, levelsRequired);
    }

    // Indicar cual es la ronda que se esta jugando
    public void SetRound(int round = -1)
    {
        if (round == -1) round = currentRound;
        currentRound = round;
        var roundData = new[] { new { round = currentRound } };
        LocalizationHelper.FormatTranslate(floorLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.round, roundData);
    }

    // Esconder o mostrar el piso
    public void Toggle(bool show)
    {
        // Determinar el color a usar
        Color textInitialColor = show ? JankenUp.JankenColors.clearWhite : JankenUp.JankenColors.white;
        Color textEndColor = show ? JankenUp.JankenColors.white : JankenUp.JankenColors.clearWhite;

        // Si el color final es el mismo actual, no prosegir
        if (textEndColor == floorText.color) return;

        if (toggleColorTween != null)
        {
            if ((show && toggleColorTween.EndValue == textEndColor)
                || (!show && toggleColorTween.EndValue == textEndColor)) return;

            toggleColorTween.Stop(TweenStopBehavior.DoNotModify);
        }

        if (show)
        {
            gameObject.SetActive(show);
        }

        System.Action<ITween<Color>> updateOverlay = (t) =>
        {
            floorText.color = t.CurrentValue;
            floorLabel.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> completeOverlay = (t) =>
        {
            gameObject.SetActive(show);
            toggleColorTween = null;
        };

        // Aumentar el color del texto
        toggleColorTween = gameObject.Tween(string.Format("Toggle{0}", gameObject.GetInstanceID()), textInitialColor, textEndColor,
            timeToToggle, TweenScaleFunctions.QuadraticEaseInOut, updateOverlay);
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetScoreFont();
        floorLabel.font = mainFont;
        floorLabel.fontStyle = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;
        SetRound();
    }

    // Obtener el tiempo de toolge
    public float GetTimeToToggle()
    {
        return timeToToggle;
    }

    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= UpdateCurrentFont;
    }

}