using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdDetailed : MonoBehaviour
{
    [Header("FX")]
    [SerializeField] ParticleSystem dust;

    // Utiles
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigidBody;
    int sortingOrder = 0;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sortingOrder = spriteRenderer.sortingOrder + 1;
    }

    // Efecto de particulas al entrar en colision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!rigidBody) rigidBody = GetComponent<Rigidbody2D>();
        if (rigidBody)
        {
            GameObject dustObject = Instantiate(dust.gameObject, transform.position, Quaternion.identity) as GameObject;
            DustParticle dustCopy = dustObject.GetComponent<DustParticle>();

            // Aplicar variaciones segun la velocidad del cuerpo
            dustCopy.SetSpeed(rigidBody.velocity.y);
            dustCopy.SetSorting(sortingOrder);
            dustCopy.SetSortingLayer(spriteRenderer.sortingLayerName);
            dustCopy.SetColor(spriteRenderer.color);
            dustCopy.Play();
        }
    }

}
