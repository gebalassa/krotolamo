using System.Collections;
using UnityEngine;

public class HumbleHouseLamp : ArenaObject
{
    [Header("Light")]
    [SerializeField] GameObject lightObject;
    [SerializeField] [Range(0, 10)] float minLightFlicks = 3;
    [SerializeField] [Range(0, 10)] float maxLightFlicks = 6;
    [SerializeField] [Range(0, 10)] float minLightTimeBetweenLoops = 3;
    [SerializeField] [Range(0, 10)] float maxLightTimeBetweenLoops = 6;
    [SerializeField] [Range(0, 10)] float minLightTimeBetweenFlicks = .1f;
    [SerializeField] [Range(0, 10)] float maxLightTimeBetweenFlicks = .2f;

    // Referencia a corutina
    Coroutine flickering;

    protected override void Start()
    {
        base.Start();
        LightFlicker();
    }

    /// <summary>
    /// Override de jump para aplicar parpadeo en ataques
    /// </summary>
    /// <param name="minForce"></param>
    /// <param name="maxForce"></param>
    /// <param name="multiplier"></param>
    public override void Jump(float minForce, float maxForce, int multiplier)
    {
        base.Jump(minForce, maxForce, multiplier);
        LightFlicker();
    }

    /// <summary>
    /// Inicia la animacion de parpadeo de las luces
    /// </summary>
    private void LightFlicker()
    {
        if (flickering != null) StopCoroutine(flickering);
        flickering = StartCoroutine(CoroutineLightFlicker());
    }

    /// <summary>
    /// Coturina para hacer parpadear la luz
    /// </summary>
    /// <returns></returns>
    IEnumerator CoroutineLightFlicker()
    {
        while (gameObject != null) {

            int flickSteps = (int) Random.Range(minLightFlicks, maxLightFlicks);
            for(int i = 0; i < flickSteps; i++){
                lightObject.SetActive(!lightObject.activeSelf);
                yield return new WaitForSeconds( Random.Range(minLightTimeBetweenFlicks, maxLightTimeBetweenFlicks) );
            }
            lightObject.SetActive(true);

            yield return new WaitForSeconds( Random.Range(minLightTimeBetweenLoops, maxLightTimeBetweenLoops) );
        }
    }
}