using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimeSpeedParticle : MonoBehaviour
{

    ParticleSystem animeSpeedSystem;

    // Dar inicio al sistema
    public void Play()
    {
        CheckSpeedSystem();
        StartCoroutine(PlayAndDestroyCoroutine());
        
    }

    // Reproduccion y destruccion al finalizar del sistema
    private IEnumerator PlayAndDestroyCoroutine()
    {
        animeSpeedSystem.Play();
        yield return new WaitForSeconds(animeSpeedSystem.main.duration * 3);
        Destroy(gameObject);
    }

    // Revisar si esta ya seteado el sistema
    private void CheckSpeedSystem()
    {
        if (animeSpeedSystem == null) animeSpeedSystem = GetComponent<ParticleSystem>();
    }
}
