using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GamePlayScene : SceneController
{
    // Arena de juego
    protected ArenaController arena;

    // Elementos de UI en escena
    List<UIGameplayCanvas> uiCanvasGroups = new List<UIGameplayCanvas>();

    // Tag de los Canvas que deben ocultarse al momento de un super
    string canvasTag = "UICanvasGameplay";

    // Util para saber si escaneo ya fue realizado
    bool scannerReady = false;

    protected new void Start()
    {
        base.Start();
        StartGetUIGameplay();
    }

    /// <summary>
    /// Obtencion de los elemento GUI asociados al gameplay
    /// </summary>
    protected void StartGetUIGameplay()
    {
        StartCoroutine(GetUIGameplay());
    }

    /// <summary>
    /// Corutina para buscar elementos asociados a la UI
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetUIGameplay()
    {
        if (!scannerReady)
        {
            yield return null;

            // Encontrar todos los elementos UI de imagenes
            GameObject[] gameObjectImages = GameObject.FindGameObjectsWithTag(canvasTag);
            foreach (GameObject gm in gameObjectImages)
            {
                uiCanvasGroups.Add(gm.AddComponent<UIGameplayCanvas>());
            }

            scannerReady = true;
        }
    }

    /// <summary>
    /// Metodo a implementar para la recepcion de un mensaje para emitir una finta
    /// </summary>
    public virtual void SetFeint(Attacks attack){}

    /// <summary>
    /// Mostrar u ocultar UI
    /// </summary>
    /// <param name="show"></param>
    private void ToogleUI(bool show)
    {
        foreach (UIGameplayCanvas uiCanvasGroup in uiCanvasGroups)
        {
            if(uiCanvasGroup && uiCanvasGroup.gameObject)
            {
                if (!show) uiCanvasGroup.Hide();
                else uiCanvasGroup.Show();
            }
        }
    }

    /// <summary>
    /// Muestra todos los elementos UI de la escena
    /// </summary>
    public void ShowUI() {
        ToogleUI(true);
    }

    /// <summary>
    /// Oculta todos los elementos UI de la escena
    /// </summary>
    public void HideUI() {
        ToogleUI(false);
    }

    /// <summary>
    /// Metodo a implementar para la recepcion de un mensaje para emitir un emote
    /// </summary>
    public virtual void SendEmote(int emoteID) {}

    /// <summary>
    /// Llamada al finalizar el temporizador de la escena
    /// </summary>
    public virtual void TimeOut() {}

    /// <summary>
    /// Ha transcurrido un segundo entre actualizaciones de tiempo
    /// </summary>
    public virtual void TimeChange() { }

    /// <summary>
    /// Actualizacion del UI de ataque
    /// TODO: Se debe tener centralizado los parametros que se usan en todas las escenas de juego, como lo es el UI de ataque
    /// </summary>
    public virtual void UpdateAttackUI() {}

    /// <summary>
    /// Indicar a la arena de juego que personajes no pueden aparecer en el fondo
    /// </summary>
    protected virtual void SetNotAllowedSpecialNPC()
    {
        // TODO: Implementar segun tipo de juego
    }
}
