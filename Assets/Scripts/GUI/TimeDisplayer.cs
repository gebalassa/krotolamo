
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DigitalRuby.Tween;
using System;

public class TimeDisplayer : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI timeTimerText;
    [SerializeField] TextMeshProUGUI timeLabel;
    [SerializeField] bool showTimeElapsed = true;

    [Header("Time")]
    [SerializeField] float timeRemaining = 12;
    float lastTimeRemaining = 0;
    bool timeIsRunning = true;
    float timeElapsed = 0;

    // Guardado de animacion
    ColorTween toggleColorTween;
    float timeToToggle = .2f;

    // Escena asociada
    GamePlayScene gamePlayScene;

    // Eventos
    public delegate void OnTimeChange();
    public event OnTimeChange onTimeChangeDelegate;

    private void Start()
    {
        UpdateCurrentFont();
        UpdateDisplayer();
        FindGamePlayScene();

        // Eventos de localizacion
        LanguageController.onLanguageChangeDelegate += UpdateCurrentFont;

        // No mostrar el tiempo transcurrido
        if (!showTimeElapsed) timeTimerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Obtencion de la escena asociada
    /// </summary>
    void FindGamePlayScene()
    {
        gamePlayScene = FindObjectOfType<GamePlayScene>();
    }

    // Actualizacion del tiempo
    void Update()
    {
        if (timeIsRunning)
        {
            // Consideramos 1 como el tiempo normal
            float timeScaleFixed = Time.timeScale - 1;
            float timeScaleMultiplier = 1;
            if (timeScaleFixed < 0)
            {
                timeScaleMultiplier += Math.Abs(timeScaleFixed);
            }
            else if (timeScaleFixed > 0)
            {
                timeScaleMultiplier -= (timeScaleFixed / timeScaleMultiplier * .5f);
            }

            lastTimeRemaining = Mathf.FloorToInt(timeRemaining + 0.99f);
            timeRemaining -= Time.deltaTime * timeScaleMultiplier;
            timeElapsed += Time.deltaTime * timeScaleMultiplier;
            UpdateDisplayer();

            if (timeRemaining < 0)
            {
                timeIsRunning = false;
                timeRemaining = 0;

                // Avisar el fin del tiempo
                FindGamePlayScene();
                if (gamePlayScene != null) gamePlayScene.TimeOut();
            }

        }
    }

    // Actualizacion de displayer
    private void UpdateDisplayer()
    {
        float timeToDisplay = Mathf.FloorToInt(timeRemaining + 0.99f);
        timeText.text = string.Format("{0}", timeToDisplay);

        // Mostrar cuanto tiempo lleva jugando la persona
        float hours = Mathf.FloorToInt(timeElapsed / 3600);
        float minutes = Mathf.FloorToInt(timeElapsed / 60) - hours * 60;
        float seconds = Mathf.FloorToInt(timeElapsed % 60);

        if (showTimeElapsed) timeTimerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

        // Si ha pasado un segundo entre actualizaciones, indicar a la escena
        if (timeToDisplay < lastTimeRemaining)
        {
            // Avisar el paso de un particula de tiempo
            if (onTimeChangeDelegate != null) onTimeChangeDelegate();
        }
    }

    // Iniciar el conteo del tiempo
    public void StartTimer()
    {
        timeIsRunning = true;
    }

    // Detener el conteo del tiempo
    public void StopTimer()
    {
        timeIsRunning = false;
    }

    // Esconder o mostrar el score
    public void Toggle(bool show)
    {
        // Determinar el color a usar
        Color textInitialColor = show ? JankenUp.JankenColors.clearWhite : JankenUp.JankenColors.white;
        Color textEndColor = show ? JankenUp.JankenColors.white : JankenUp.JankenColors.clearWhite;

        // Si el color final es el mismo actual, no prosegir
        if (textEndColor == timeText.color) return;

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
            timeText.color = t.CurrentValue;
            timeLabel.color = t.CurrentValue;
            if (showTimeElapsed) timeTimerText.color = t.CurrentValue;
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
        timeLabel.font = mainFont;
        timeLabel.fontStyle = FontManager._mainManager.IsBold() ? FontStyles.Bold : FontStyles.Normal;

        LocalizationHelper.Translate(timeLabel, JankenUp.Localization.tables.InGame.tableName, JankenUp.Localization.tables.InGame.Keys.time);
    }

    // Agregar segundos al tiempo restante
    public void AddTime(int time)
    {
        timeRemaining += time;
    }

    // Quitar segundos al tiempo restante
    public void SubtractTime(int time)
    {
        timeRemaining -= time;
    }

    // Obtencion del tiempo actual
    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    // Obtencion del tiempo transcurrido
    public float GetTimeElapsed()
    {
        return timeElapsed;
    }

    // Obtener el tiempo de toolge
    public float GetTimeToToggle()
    {
        return timeToToggle;
    }

    // Sumatoria final del tiempo acumulado al tiempo que se mantuvo el jugador en la plataforma
    public void AddRemainingToElapsed(float timeToAnimate)
    {
        float finalTimeElapsed = timeElapsed + timeRemaining;
        float finalTimeRemaining = 0;

        System.Action<ITween<float>> updateTimeElapsed = (t) =>
        {
            timeElapsed = t.CurrentValue;
            UpdateDisplayer();
        };

        System.Action<ITween<float>> updateTimeRemaining = (t) =>
        {
            timeRemaining = t.CurrentValue;
            UpdateDisplayer();
        };

        // Realizar las animaciones
        gameObject.Tween(string.Format("TimeMixElapsed{0}", GetInstanceID()), timeElapsed, finalTimeElapsed, timeToAnimate, TweenScaleFunctions.QuadraticEaseOut, updateTimeElapsed);
        gameObject.Tween(string.Format("TimeMixRemaining{0}", GetInstanceID()), timeRemaining, finalTimeRemaining, timeToAnimate, TweenScaleFunctions.QuadraticEaseOut, updateTimeRemaining);
    }

    // Setear el tiempo restante
    public void SetRemainingTime(float time)
    {
        timeRemaining = time;
        StartTimer();
    }

    // Setear el tiempo transcurrido
    public void SetElapsedTime(float time)
    {
        timeElapsed = time;
    }

    // Saber si el timer esta corriendo o no
    public bool IsTimerRunning()
    {
        return timeIsRunning;
    }

    private void OnDestroy()
    {
        LanguageController.onLanguageChangeDelegate -= UpdateCurrentFont;
    }
}
