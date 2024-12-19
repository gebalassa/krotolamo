using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadesController : MonoBehaviour
{
    [Header("Arcades")]
    [SerializeField] Sprite[] arcadesSprites;
    [SerializeField] Color[] lightsColor;
    [SerializeField] GameObject arcadePrefab;
    [SerializeField] GameObject lightPrefab;

    [Header("Setup")]
    [SerializeField] float widthPerArcade = 5.38f;

    // Arcades necesarios para fondo
    int arcades = -1;
    List<SpriteRenderer> arcadeSpriteRenderers = new List<SpriteRenderer>();
    List<ArenaObject> arcadeArenaObjects = new List<ArenaObject>();

    /// <summary>
    /// Generacion o actualizacion de los arcades utilizados en el escenario
    /// </summary>
    public void GenerateArcades()
    {
        // Cambiar el orden de los arcades
        ShuffleArcades();

        // Generar nuevos arcades
        if(arcades == -1)
        {
            arcades = Mathf.CeilToInt((Camera.main.orthographicSize * Camera.main.aspect * 2) / widthPerArcade);

            // Los arcades se dejan en montos pares. De las pruebas realizadas, con este tecnica se evitaba espacios vacios
            if (arcades % 2 != 0) arcades++;

            float firstPosition = -widthPerArcade * Mathf.CeilToInt((arcades - 1) / 2f);

            int lastLightIndex = 0;

            for (int i = 0; i < arcades; i++)
            {
                GameObject arcade = Instantiate(arcadePrefab, transform);

                // Mover una unidad de 'widthPerArcade' por cada arcade creado
                float xPosition = firstPosition + widthPerArcade * i;
                arcade.transform.localPosition = new Vector3(xPosition, 0, 0);

                // Cambiar el sprite del elemento
                SpriteRenderer spriteRenderer = arcade.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = arcadesSprites[i % arcadesSprites.Length];

                // Anadir el elemento para cambiar a futuro
                arcadeSpriteRenderers.Add(spriteRenderer);

                // Anadir el objeto de arena para aplicar saltos
                arcadeArenaObjects.Add(arcade.GetComponent<ArenaObject>());

                // Generar las luces en cada elemento impar
                if (i % 2 == 1)
                {
                    GameObject light = Instantiate(lightPrefab, arcade.transform);
                    ArcadeLightController arcadeLightController = light.GetComponent<ArcadeLightController>();
                    arcadeLightController.SetColor(lightsColor[lastLightIndex++ % lightsColor.Length]);
                }
            }
        }
        else
        {
            // Cambiar los sprites de las maquinas
            for (int i = 0; i < arcadeSpriteRenderers.Count; i++)
            {
                SpriteRenderer spriteRenderer = arcadeSpriteRenderers[i];
                spriteRenderer.sprite = arcadesSprites[i % arcadesSprites.Length];
            }
        }
        
    }

    /// <summary>
    /// Cambio en el orden de los sprites de arcades
    /// </summary>
    void ShuffleArcades()
    {
        Sprite temp;
        for (int i = 0; i < arcadesSprites.Length; i++)
        {
            int rnd = Random.Range(0, arcadesSprites.Length);
            temp = arcadesSprites[rnd];
            arcadesSprites[rnd] = arcadesSprites[i];
            arcadesSprites[i] = temp;
        }
    }

    /// <summary>
    /// Realizar salto de las arcades en terreno
    /// </summary>
    /// <param name="minForce"></param>
    /// <param name="maxForce"></param>
    /// <param name="multiplier"></param>
    public void Jump(float minForce, float maxForce, int multiplier)
    {
        // Aplicar con distinta fuerza a los distintos arcades
        foreach (ArenaObject arcade in arcadeArenaObjects)
        {
            arcade.Jump(minForce, maxForce, multiplier);
        }
    }
}