using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuperJanKenUPBase : MonoBehaviour
{
    [Header("Prepare Phase")]
    [SerializeField] Image overlay;
    [SerializeField] Color overlayColor = new Color(0, 0, 0, .6f);
    [SerializeField] [Range(0, 1f)] float overlayTime = 0.5f;

    // Mantener character asociado al super que se ejecutara
    protected CharacterInGameController character;

    // Orden para el sprite del personaje
    protected int spriteOverlayIndex = 25;
    protected int spriteOriginalIndex = 0;

    // Fase post super
    protected bool postAppearSuperReadyToExecute = false;

    // Saber si el referee desaparecera
    protected bool byeByeReferee = true;

    // Configuracion del super, indicando el character target y otros elementos. Se debe guardar el character para otros metodos
    public virtual void Setup(CharacterInGameController character, Dictionary<string, object> data)
    {
        this.character = character;
        if (data.ContainsKey("byeByeReferee")) byeByeReferee = (bool) data["byeByeReferee"];
    }

    // Metodo llamado para preparar el super
    public virtual IEnumerator Prepare()
    {
        yield return null;
    }

    // Ejecucion del super cuando el jugador gana la partida
    public virtual IEnumerator WinExecute()
    {
        yield return null;
    }

    // Ejecucion del super cuando el jugador empata la partida
    public virtual IEnumerator DrawExecute()
    {
        yield return null;
    }

    // Acciones posteriores a la ejecucion del super
    public virtual IEnumerator PostSuper()
    {
        yield return null;
    }

    // Finalizacion del super
    public virtual void Finish()
    {
        Destroy(gameObject);
    }

    // Mostrar o esconder el overlay
    protected void ToggleCameraOverlay(bool show)
    {
        // Determinar el color segun el estado deseado
        Color initialColor = show ? Color.clear : overlayColor;
        Color endColor = show ? overlayColor : Color.clear;

        ToggleCameraOverlay(show, initialColor, endColor);
    }

    // Mostrar o esconder el overlay
    protected void ToggleCameraOverlay(bool show, Color from, Color to)
    {
        // Activar de manera inmediata el overlay si es que se desea mostrar
        if (show)
        {
            overlay.gameObject.SetActive(show);
        }

        System.Action<ITween<Color>> updateOverlay = (t) =>
        {
            if(overlay) overlay.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> completeOverlay = (t) =>
        {
            if(overlay) overlay.gameObject.SetActive(show);
        };

        // Aumentar el color del fondo
        if(overlay) overlay.gameObject.Tween(string.Format("Toggle{0}", overlay.GetInstanceID()), from, to,
            overlayTime, TweenScaleFunctions.QuadraticEaseInOut, updateOverlay, completeOverlay);
    }

    /// <summary>
    /// Notificacion de que esta listo para aparecer otra vez el personaje
    /// </summary>
    public virtual void NofityIsReadyToReapper()
    {
        postAppearSuperReadyToExecute = true;
    }

    /// <summary>
    /// Revision de si existe camara especial
    /// </summary>
    /// <returns></returns>
    protected bool CheckCamerasLike()
    {
        // Si el juego actual tiene una rotacion de camara para dar efectos, solo usar el ZoomOut
        CameraLike[] camerasLike = FindObjectsOfType<CameraLike>();
        if (camerasLike.Length > 0)
        {
            foreach (CameraLike cameraLike in camerasLike)
            {
                if (cameraLike.gameObject.activeSelf && cameraLike.HasRotation())
                {
                    return true;
                }
            }
        }

        return false;
    }

}