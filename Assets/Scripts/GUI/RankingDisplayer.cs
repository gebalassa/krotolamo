using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DigitalRuby.Tween;

public class RankingDisplayer : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI positionValue;
    [SerializeField] TextMeshProUGUI totalValue;
    [SerializeField] TextMeshProUGUI rankingLabel;

    private void Start()
    {
        UpdateCurrentFont();
    }

    // Setear el valor actual de posicion
    public void SetPosition(int position, int total) {

        System.Action<ITween<float>> updatePosition = (t) =>
        {
            positionValue.text = ((int)t.CurrentValue).ToString();
        };

        System.Action<ITween<float>> updateTotal = (t) =>
        {
            totalValue.text = ((int)t.CurrentValue).ToString();
        };

        // Aumentar la puntuación
        gameObject.Tween(string.Format("Score{0}", positionValue.GetInstanceID()), float.Parse(positionValue.text), position, .5f, TweenScaleFunctions.QuadraticEaseOut, updatePosition);
        gameObject.Tween(string.Format("Score{0}", totalValue.GetInstanceID()), float.Parse(totalValue.text), total, .5f, TweenScaleFunctions.QuadraticEaseOut, updateTotal);

    }

    // Esconder o mostrar el score
    public void Toggle(bool show) {

        if (show)
        {
            gameObject.SetActive(show);
        }

        System.Action<ITween<Color>> updateOverlay = (t) =>
        {
            positionValue.color = t.CurrentValue;
            rankingLabel.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> completeOverlay = (t) =>
        {
            gameObject.SetActive(show);
        };

        Color textInitialColor = show ? new Color(1, 1, 1, 0) : Color.white;
        Color textEndColor = show ? Color.white : new Color(1, 1, 1, 0);

        // Aumentar el color del texto
        gameObject.Tween(string.Format("Toggle{0}", gameObject.GetInstanceID()), textInitialColor, textEndColor,
            .4f, TweenScaleFunctions.QuadraticEaseInOut, updateOverlay);
    }

    // Actualizacion de la fuente que se esta usando segun idioma
    public void UpdateCurrentFont()
    {
        TMP_FontAsset mainFont = FontManager._mainManager.GetScoreFont();
        rankingLabel.font = mainFont;
        rankingLabel.fontStyle = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;

        LocalizationHelper.Translate(rankingLabel, JankenUp.Localization.tables.Online.tableName, JankenUp.Localization.tables.Online.Keys.ranking);
    }

}
