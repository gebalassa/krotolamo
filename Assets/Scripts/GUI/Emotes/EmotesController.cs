using DigitalRuby.Tween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmotesController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] GameObject emotesButtonsContainer;
    [SerializeField] List<EmoteButton> emotesButtonsPrefabs;
    [SerializeField] Image overlay;

    [Header("Posiciones")]
    [SerializeField] List<Vector2> emotesPositions;

    [Header("Overlay")]
    [SerializeField] Color overlayColor = new Color(0, 0, 0, .25f);
    [SerializeField] Color overlayColorHide = Color.clear;
    [SerializeField] [Range(0, 1)] float timeOverlayFade = 0.25f;

    EmoteButton currentTrigger;
    EmoteButton candidate;
    List<EmoteButton> emotesButtons = new List<EmoteButton>();

    // Estado
    bool isOpen = false;

    // Controlador de partida
    private GamePlayScene gamePlayScene;

    // Animaciones
    ColorTween fadeAnimation;

    /// <summary>
    /// El ultimo emoji seleccionado debe hacer de trigger
    /// En caso de que este boton sea presionado como tap (Osea, < 0.25 s (ajustable)), se envia ese ataque. Se debe ademas inhabilitar la posibilidad de enviar emotes por X tiempo
    /// En caso de que se mantenga presionado, deben mostrarse todos los emotes dejando un espacio entre cada uno
    /// Al seleccionarse uno, ese queda como trigger para lo mismo del primer paso
    /// </summary>

    /// <summary>
    /// Generar todos los emotes que esten en los prefabs.
    /// Mantener el primero como trigger, los demas se mantienen invisibles
    /// </summary>
    void Start()
    {
        int index = 0;
        foreach(EmoteButton emotePrefab in emotesButtonsPrefabs)
        {
            EmoteButton newEmoteButton = Instantiate(emotePrefab, emotesButtonsContainer.transform);
            newEmoteButton.SetEmoteController(this);
            newEmoteButton.SetTrigger(index++ == 0, false);
            emotesButtons.Add(newEmoteButton);
        }
        currentTrigger = emotesButtons[0];

        // Encontrar el controlador de la partida actual
        gamePlayScene = FindObjectOfType<GamePlayScene>();
    }

    
    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Abrir o cerrar el controlador
    /// </summary>
    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    /// <summary>
    /// Apertura del controlador de emotes
    /// </summary>
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        // Mostrar overlay
        ShowOverlay();

        int index = 1;
        foreach(EmoteButton emoteButton in emotesButtons)
        {
            if (emoteButton == currentTrigger) continue;
            emoteButton.transform.localPosition = emotesPositions[index++];
            emoteButton.Show();
            // Si existen mas emotes que posiciones, terminar
            if (index >= emotesPositions.Count) break;
        }
    }

    /// <summary>
    /// Cerrar el controlador de emotes
    /// </summary>
    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        // Ocultar overlay
        HideOverlay();

        foreach (EmoteButton emoteButton in emotesButtons)
        {
            if (emoteButton == currentTrigger) continue;
            emoteButton.Hide();
        }
    }

    /// <summary>
    /// Seleccion de uno de los emotes
    /// </summary>
    /// <param name="emoteButton"></param>
    public void Select(EmoteButton emoteButton)
    {
        candidate = emoteButton;
    }

    /// <summary>
    /// Quitar la seleccion del emote
    /// </summary>
    public void Deselect()
    {
        candidate = null;
    }

    /// <summary>
    /// Confirmacion del emote a emitir
    /// </summary>
    public void ConfirmSelection()
    {
        if (!candidate) return;

        // Cambiar el trigger actual
        if (candidate != currentTrigger)
        {
            currentTrigger.SetTrigger(false, false);
            currentTrigger = candidate;

            // Cambiar posicion del trigger actual
            currentTrigger.SetTrigger(true, true);

        }

        // Cerrar todos los demas emotes
        Close();

        // Notificar de la seleccion
        if (gamePlayScene) gamePlayScene.SendEmote(candidate.GetIDAsInt());
        candidate = null;
    }

    /// <summary>
    /// Obtencion de la propiedad para saber si esta abierto o no el controloador
    /// </summary>
    /// <returns></returns>
    public bool IsOpen()
    {
        return isOpen;
    }

    /// <summary>
    /// Habilitar la seleccion de emotes
    /// </summary>
    /// <param name="enable"></param>
    public void Enable(bool enable)
    {
        currentTrigger.Enable(enable);
    }

    /// <summary>
    /// El overlay queda habilitado para ser presionado.
    /// </summary>
    private void ShowOverlay()
    {
        overlay.raycastTarget = true;
        //DoAnimation(AnimationFade(overlay.color, overlayColor, TweenScaleFunctions.Linear));
    }

    /// <summary>
    /// El overlay queda inhabilitado para ser presionado.
    /// </summary>
    private void HideOverlay()
    {
        overlay.raycastTarget = false;
        //DoAnimation(AnimationFade(overlay.color, overlayColorHide, TweenScaleFunctions.Linear));
    }

    #region Animation

    /// <summary>
    /// Realizar corutina especificada
    /// </summary>
    /// <param name="coroutine">Rutina a realizar</param>
    private void DoAnimation(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    /// <summary>
    /// Animacion de fade para elemento 
    /// </summary>
    /// <param name="from">Escala inicial</param>
    /// <param name="to">Escala final</param>
    /// <param name="scaleFunction">Funcion para tiempos de animacion</param>
    private IEnumerator AnimationFade(Color from, Color to, Func<float, float> scaleFunction)
    {
        if (fadeAnimation != null) fadeAnimation.Stop(TweenStopBehavior.DoNotModify);

        System.Action<ITween<Color>> fadeFn = (t) =>
        {
            if (overlay != null) overlay.color = t.CurrentValue;
        };

        // Que ataque aparezca
        fadeAnimation = gameObject.Tween(string.Format("fade{0}", overlay.GetInstanceID()), from, to,
            timeOverlayFade, scaleFunction, fadeFn);

        yield return null;

    }

    #endregion

}