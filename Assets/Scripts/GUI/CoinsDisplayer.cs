using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DigitalRuby.Tween;

public class CoinsDisplayer : MonoBehaviour
{
    // Hijos
    GameObject icon;
    GameObject label;
    TextMeshProUGUI textMeshProUGUI;

    // Start is called before the first frame update
    void Start()
    {
        icon = transform.Find("Icon").gameObject;
        label = transform.Find("Label").gameObject;
        textMeshProUGUI = label.GetComponent<TextMeshProUGUI>();
    }

    // Indicar la cantidad de vidas que quedan
    public void SetLabel(int number)
    {
        if (!label)
        {
            label = transform.Find("Label").gameObject;
            textMeshProUGUI = label.GetComponent<TextMeshProUGUI>();
        }

        System.Action<ITween<float>> updateText = (t) =>
        {
            textMeshProUGUI.text = ((int)t.CurrentValue).ToString();
        };

        // Aumentar la puntuación
        gameObject.Tween(string.Format("Score{0}", GetInstanceID()), float.Parse(textMeshProUGUI.text), number, .5f, TweenScaleFunctions.QuadraticEaseOut, updateText);

    }
}
