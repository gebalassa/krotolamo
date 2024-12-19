using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DigitalRuby.Tween;

public class ScoreDisplayer : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI scoreLabel;

    // Guardado de animacion
    ColorTween toggleColorTween;
    float timeToToggle = .2f;

    private void Start()
    {
        UpdateCurrentFont();
        LanguageController.onLanguageChangeDelegate += UpdateCurrentFont;
    }

    // Setear el valor actual del puntaje
    public void SetScore(int score)
    {

        System.Action<ITween<float>> updateText = (t) =>
        {
            scoreText.text = ((int)t.CurrentValue).ToString();
        };

        // Aumentar la puntuación
        gameObject.Tween(string.Format("Score{0}", GetInstanceID()), float.Parse(scoreText.text), score, .5f, TweenScaleFunctions.QuadraticEaseOut, updateText);

    }

    // Esconder o mostrar el score
    public void Toggle(bool show)
    {
        // Determinar el color a usar
        Color textInitialColor = show ? JankenUp.JankenColors.clearWhite : JankenUp.JankenColors.white;
        Color textEndColor = show ? JankenUp.JankenColors.white : JankenUp.JankenColors.clearWhite;

        // Si el color final es el mismo actual, no prosegir
        if (textEndColor == scoreText.color) return;

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
            scoreText.color = t.CurrentValue;
            scoreLabel.color = t.CurrentValue;
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
        scoreLabel.font = mainFont;
        scoreLabel.fontStyle = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;

        LocalizationHelper.Translate(scoreLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.score);
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