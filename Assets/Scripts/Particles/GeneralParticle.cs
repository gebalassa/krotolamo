using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralParticle : MonoBehaviour
{

    ParticleSystem generalSystem;

    // Dar inicio al sistema
    public void Play()
    {
        CheckSpeedSystem();
        StartCoroutine(PlayAndDestroyCoroutine());
    }

    // Reproduccion y destruccion al finalizar del sistema
    private IEnumerator PlayAndDestroyCoroutine()
    {
        generalSystem.Play();
        yield return new WaitForSeconds(generalSystem.main.duration * 3);
        if(gameObject) Destroy(gameObject);
    }

    // Revisar si esta ya seteado el sistema
    private void CheckSpeedSystem()
    {
        if (generalSystem == null) generalSystem = GetComponent<ParticleSystem>();
    }

    // Camiar la posicion en la que se muestra el efecto
    public void SetLocalPosition(Transform parent, Vector2 localPosition)
    {
        CheckSpeedSystem();
        transform.parent = parent;
        transform.localPosition = localPosition;
    }

    // Camiar el color de la particula
    public void SetColor(Color color)
    {
        CheckSpeedSystem();
        ParticleSystem.MainModule main = generalSystem.main;
        var startColor = main.startColor;
        startColor.colorMax = color;
        startColor.colorMin = Color.white;
        main.startColor = startColor;
    }

    // Configuracion de la capa de renderizado
    public void SetSortingLayer(string layerName)
    {
        CheckSpeedSystem();
        // Ubicar correctamente en la arena
        ParticleSystemRenderer psRenderer = generalSystem.gameObject.GetComponent<ParticleSystemRenderer>();
        psRenderer.sortingLayerName = layerName;
    }

}
