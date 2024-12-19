using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBackground : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material shopMaterial;
    [SerializeField] Material orangeMaterial;
    [SerializeField] Material yellowMaterial;

    [Header("Colors")]
    [SerializeField] Color defaultColor;
    [SerializeField] Color shopColor;
    [SerializeField] Color orangeColor;
    [SerializeField] Color yellowColor;

    [Header("Setup")]
    [SerializeField] BackgroundEnvironment currentMaterial = BackgroundEnvironment.Default;
    [SerializeField] [Range(-5f, 5f)] float speed = -0.1f;
    float lastSpeed = -0.1f;

    Renderer materialRenderer;
    Vector2 offset;

    BackgroundEnvironment lastEnvironment = BackgroundEnvironment.Default;

    // Start is called before the first frame update
    void Start()
    {
        ChangeMaterial(currentMaterial);
        offset = new Vector2(speed, speed);
        lastSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        SetSpeed();
        ChangeMaterial(currentMaterial);
    }

    /// <summary>
    /// Establecer la velocidad de movimiento del material
    /// </summary>
    void SetSpeed()
    {
        if (lastSpeed != speed)
        {
            offset = new Vector2(speed, speed);
            lastSpeed = speed;
        }

        CheckMaterialRenderer();
        materialRenderer.material.mainTextureOffset += offset * Time.deltaTime;
    }

    /// <summary>
    /// Cambio del material asignado a la escena
    /// </summary>
    public void ChangeMaterial(BackgroundEnvironment environment)
    {
        if (lastEnvironment == environment) return;
        CheckMaterialRenderer();

        currentMaterial = environment;
        lastEnvironment = currentMaterial;

        switch (currentMaterial)
        {
            case BackgroundEnvironment.Shop:
                materialRenderer.material = shopMaterial;
                Camera.main.backgroundColor = shopColor;
                break;
            case BackgroundEnvironment.Orange:
                materialRenderer.material = orangeMaterial;
                Camera.main.backgroundColor = orangeColor;
                break;
            case BackgroundEnvironment.Yellow:
                materialRenderer.material = yellowMaterial;
                Camera.main.backgroundColor = yellowColor;
                break;
            case BackgroundEnvironment.Default:
            default:
                materialRenderer.material = defaultMaterial;
                Camera.main.backgroundColor = defaultColor;
                break;
        }
    }

    /// <summary>
    /// Obtener el renderer del material
    /// </summary>
    private void CheckMaterialRenderer()
    {
        if (materialRenderer) return;
        materialRenderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Obtencion del material que se esta usando
    /// </summary>
    /// <returns></returns>
    public BackgroundEnvironment GetCurrentMaterial()
    {
        return currentMaterial;
    }
}
