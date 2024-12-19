using DigitalRuby.Tween;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CoinsCurrencyDisplayer : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Text coinText;

    // Otros utiles
    private float maxTimeToChangeCoins = 2;
    private float deltaBetweenChangeCoins = 2 / 10f;
    Tween<float> tweenChangeCoins;

    // Use this for initialization
    void Start()
    {
        // Actualizar en caso de que exista la instancia
        OnCoinsCurrencyUpdate();
    }

    /// <summary>
    /// Actualizacion de la cantidad de monedas en la displayer
    /// </summary>
    public void OnCoinsCurrencyUpdate()
    {
        ChangeCoins();
    }

    /// <summary>
    /// Realizar cambio animado de monedas
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    private void ChangeCoins()
    {
        SingleModeSession singleModeSession = FindObjectOfType < SingleModeSession >();
        if (!singleModeSession) return;
        if (tweenChangeCoins != null) tweenChangeCoins.Stop(TweenStopBehavior.DoNotModify);

        // Mostrar animando
        System.Action<ITween<float>> updateCoins = (t) =>
        {
            if (coinText) coinText.text = Convert.ToInt32(t.CurrentValue).ToString();
        };

        // Calcular el from
        int from = Convert.ToInt32(coinText.text);
        int to = singleModeSession.GetCoins();

        // Calcular el tiempo en base a la diferencia de humitas
        float timeToChangeCoins = deltaBetweenChangeCoins * Math.Abs(to - from);
        if (timeToChangeCoins > maxTimeToChangeCoins) timeToChangeCoins = maxTimeToChangeCoins;

        // Realizar las animaciones
        tweenChangeCoins = gameObject.Tween(string.Format("ChangeHumitas{0}", GetInstanceID()), from, to, timeToChangeCoins, TweenScaleFunctions.QuadraticEaseOut, updateCoins);
    }
}