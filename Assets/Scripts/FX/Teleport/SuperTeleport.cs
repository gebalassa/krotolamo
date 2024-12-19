using System.Collections;
using UnityEngine;

public class SuperTeleport : MonoBehaviour
{
    SuperJanKenUPBase superController;
    Animator animator;

    // Estados
    string stateKey = "State";
    public enum animationStates{
        TeleportOne,
        TeleportTwo
    }

    /// <summary>
    /// Obtencion del animador
    /// </summary>
    void GetAnimator()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Asociacion con controlador de super para notificar eventos
    /// </summary>
    /// <param name="superController">Controlador de super asociado</param>
    public void SetSuperController(SuperJanKenUPBase superController) {
        this.superController = superController;
    }

    /// <summary>
    /// Notificacion al controladore de que esta listo para aparecer
    /// </summary>
    public void NotifyController() {
        if(superController != null) superController.NofityIsReadyToReapper();
    }

    /// <summary>
    /// Cambio de estado de animacion
    /// Nota: Al ser solo un cambio que se necesita, esta funcion por ahora solo cambia a ese estado
    /// </summary>
    public void ChangeState(SuperTeleport.animationStates state) {
        GetAnimator();
        int newState = (int) state;
        animator.SetInteger(stateKey, newState);
    }

    /// <summary>
    /// Destruccion del elemento una vez finalizada la animacion
    /// </summary>
    public void AutoDestroy(float delay = 0)
    {
        if (delay > 0) StartCoroutine(AutoDestroyCoroutine(delay));
        else Destroy(gameObject);
    }

    /// <summary>
    /// Corutina para destruir el objeto pasa un delay de tiempo
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator AutoDestroyCoroutine(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}