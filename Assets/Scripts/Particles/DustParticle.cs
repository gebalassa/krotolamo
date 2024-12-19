using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustParticle : MonoBehaviour
{

    ParticleSystem dustSystem;

    // Dar inicio al sistema
    public void Play()
    {
        CheckDustSystem();
        StartCoroutine(PlayAndDestroyCoroutine());
        
    }

    // Reproduccion y destruccion al finalizar del sistema
    private IEnumerator PlayAndDestroyCoroutine()
    {
        dustSystem.Play();
        yield return new WaitForSeconds(dustSystem.main.duration * 3);
        Destroy(gameObject);
    }

    // Configuracion del sistema de particulas
    public void SetSorting(int sortingOrder)
    {
        CheckDustSystem();
        // Ubicar correctamente en la arena
        ParticleSystemRenderer psRenderer = dustSystem.gameObject.GetComponent<ParticleSystemRenderer>();
        psRenderer.sortingOrder = sortingOrder;
    }

    public void SetSpeed(float startSpeed)
    {
        CheckDustSystem();
        ParticleSystem.MainModule psMain = dustSystem.main;
        psMain.startSpeed = startSpeed;
    }

    public void SetXScaleMultiplier(int xScaleMultipler)
    {
        CheckDustSystem();
        ParticleSystem.ShapeModule psShape = dustSystem.shape;
        psShape.scale = new Vector3(psShape.scale.x * xScaleMultipler, psShape.scale.y, psShape.scale.z);
    }

    // Revisar si esta ya seteado el sistema
    private void CheckDustSystem()
    {
        if (dustSystem == null) dustSystem = GetComponent<ParticleSystem>();
    }

    // Configuracion de la capa de renderizado
    public void SetSortingLayer(string layerName)
    {
        CheckDustSystem();
        // Ubicar correctamente en la arena
        ParticleSystemRenderer psRenderer = dustSystem.gameObject.GetComponent<ParticleSystemRenderer>();
        psRenderer.sortingLayerName = layerName;
    }

    /// <summary>
    /// Cambio en el color de la particula
    /// </summary>
    /// <param name="newColor"></param>
    public void SetColor(Color newColor) {
        CheckDustSystem();
        ParticleSystem.MainModule psMain = dustSystem.main;
        psMain.startColor = newColor;
    }
}
