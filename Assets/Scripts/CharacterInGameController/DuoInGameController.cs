using System.Collections;
using UnityEngine;
public class DuoInGameController : CharacterInGameController
{
    [Header("SFX")]
    [SerializeField] AudioClip hitTheFloorSFX;

    [Header("HitTheFloor")]
    [SerializeField] [Range(0, 10)] float shakeMagnitude = 0.2f;

    // Componentes
    InGameSequence inGameSequence;

    // Metodo llamado al animar la perdida en un match. Debe indicarle al InGameSequence que anime un saltito de los otros actores
    public void HitTheFloor(){

        // Comprobar que el personaje este activo
        if (isDisappeared) return;

        // Indicar al InGameSequence que deben saltar los otros PJ
        if (!inGameSequence) inGameSequence = FindObjectOfType<InGameSequence>();
        if (inGameSequence) StartCoroutine(inGameSequence.JumpEverybodyMinus(this,true, shakeMagnitude));

    }

    // Funcion a llamar para reproducir el sonido de golpe al piso
    public void HitTheFloorSFX()
    {
        // Reproducir el sonido
        if(MasterSFXPlayer._player) MasterSFXPlayer._player.PlayOneShot(hitTheFloorSFX);
    }
}