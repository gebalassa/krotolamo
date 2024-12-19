using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChorizaApple : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField][Range(0.1f,40f)] float minXForce = 20f;
    [SerializeField][Range(0.1f,40f)] float maxXForce = 30f;
    [SerializeField][Range(0.1f,20f)] float minYForce = 0f;
    [SerializeField][Range(0.1f,20f)] float maxYForce = 10f;
    [SerializeField][Range(0.1f,20f)] float minTorque = 10f;
    [SerializeField][Range(0.1f,20f)] float maxTorque = 20f;
    [SerializeField] int finalOrderSorting = 30;
    [SerializeField] float changeOrderSortingAfter = .1f;
    [SerializeField] float shakeTime = .1f;
    [SerializeField] float waitBeforeDisappear = 2f;
    [SerializeField] float timeToDissapear = 1f;

    // Listado de personajes con los que ya se ha colisionado
    List<GameObject> characterToIgnore = new List<GameObject>();
    bool isFlipped = false;

    // Flag para saber si golpeo al rival
    bool hitEnemy = false;

    /// <summary>
    /// Aplicar fuerza sobre la guitarra
    /// </summary>
    private void Start()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        Vector2 newForce = new Vector2(Random.Range(minXForce, maxXForce) * (isFlipped? -1 : 1), Random.Range(minYForce, maxYForce));
        float newTorque = Random.Range(minTorque, maxTorque) * (isFlipped ? -1 : 1);
        rigidbody.AddForce(newForce,ForceMode2D.Impulse);
        rigidbody.AddTorque(newTorque, ForceMode2D.Impulse);

        // Solicitar el cambio de orden del sprite
        StartCoroutine(ChangeOrderSorting());
    }

    /// <summary>
    /// Guardado del personaje padre de la guitarra
    /// </summary>
    /// <param name="characterParent"></param>
    /// <param name="negative"></param>
    public void Setup(GameObject characterParent, bool flipped, string layer = null, int sortingOrder = -1)
    {
        Collider2D selfCollider2D = GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(characterParent.GetComponent<Collider2D>(), selfCollider2D);
        isFlipped = flipped;

        // Buscaremos ademas los demas personajes que hayan desaparecido para que no les afecte la guitarra
        CharacterInGameController[] characterInGameControllers = FindObjectsOfType<CharacterInGameController>();
        foreach(CharacterInGameController characterInGameController in characterInGameControllers)
        {
            if(characterInGameController.IsDisappeared()) Physics2D.IgnoreCollision(characterInGameController.GetComponent<Collider2D>(), selfCollider2D);
        }

        // Cambiar layer de der necesario
        if (layer != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = layer;
            if (sortingOrder != -1) spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    /// <summary>
    /// Cambiar el orden de renderizado de la guitarra
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeOrderSorting()
    {
        yield return new WaitForSeconds(changeOrderSortingAfter);
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = finalOrderSorting;
    }

    /// <summary>
    /// Condiciones para que se produzca una colision
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // En caso de ser un personaje, solo ocurrira una colision
        CharacterInGameController isCharacter = collision.gameObject.GetComponent<CharacterInGameController>();
        if (isCharacter != null)
        {
            if (!characterToIgnore.Contains(collision.gameObject))
            {
                Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
                characterToIgnore.Add(collision.gameObject);
                if(!hitEnemy) StartCoroutine(Dissapear());
                hitEnemy = true;
            }
            return;
        }
    }

    /// <summary>
    /// Corutina para desaparecer
    /// </summary>
    /// <returns></returns>
    private IEnumerator Dissapear()
    {
        yield return new WaitForSeconds(waitBeforeDisappear);

        // Colores
        Color initColor = new Color(1, 1, 1, 1);
        Color finalColor = new Color(1, 1, 1, 0);

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        // Ocultar
        System.Action<ITween<Color>> fadeOut = (t) =>
        {
            if (sprite) sprite.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> fadeOutComplete = (t) =>
        {
            if (gameObject != null) Destroy(gameObject);
        };

        gameObject.Tween(string.Format("Dissapear{0}", GetInstanceID()), initColor, finalColor,
            timeToDissapear, TweenScaleFunctions.QuadraticEaseInOut, fadeOut, fadeOutComplete);
    }

    /// <summary>
    /// Permite saber si enemigo fue golpeado
    /// </summary>
    /// <returns></returns>
    public bool EnemyHit() {
        return hitEnemy;
    }
}