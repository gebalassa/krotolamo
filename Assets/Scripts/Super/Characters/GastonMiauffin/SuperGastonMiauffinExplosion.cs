using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperGastonMiauffinExplosion : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particleSystems;
    [SerializeField] [Range(0, 3f)] float waitToDestroy = 1.5f;

    // Dar inicio al sistema
    public void Play()
    {
        StartCoroutine(PlayAndDestroyCoroutine());
    }

    // Reproduccion y destruccion al finalizar del sistema
    private IEnumerator PlayAndDestroyCoroutine()
    {
        foreach(ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Play();
        }
        yield return new WaitForSeconds(waitToDestroy);
        Destroy(gameObject);
    }

}
