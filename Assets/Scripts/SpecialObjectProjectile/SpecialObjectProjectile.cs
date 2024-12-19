using DigitalRuby.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialObjectProjectile : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Forces")]
    [SerializeField] [Range(0, 200f)] float minXForce = 10;
    [SerializeField] [Range(0, 200f)] float maxXForce = 20;
    [SerializeField] [Range(0, 10f)] float minTorque = 1f;
    [SerializeField] [Range(0, 10f)] float maxTorque = 10f;

    [Header("Position")]
    [SerializeField] [Range(0, 1)] float minYPosition = .5f;
    [SerializeField] [Range(0, 1)] float maxYPosition = 1f;

    // Tiempo para desparecer despues de haber sido lanzado
    float timeToDestroy = 5f;
    float timeToFade = 2f;
    Coroutine coroutineToDestroyInstance;
    ColorTween colorTweenInstance;

    // Objetivo del disparo
    Transform target;

    // Componentes recurrentes
    Rigidbody2D rigidbody2D;
    Collider2D collider2D;
    Color clearWhite = new Color(255, 255, 255, 0);

    // Flag para determinar que esta completo la inicializacion
    bool startReady = false;

    // Flag para indicar que las configuraciones de fuerza y posicion vienen ya seteadas
    bool fixedParams = false;

    // Parametros de lanzamiento
    float force = 0;
    float torque = 0;
    float yPosition = 0.5f;

    // Listado de objetos activos
    public static List<SpecialObjectProjectile> specialObjectInstances = new List<SpecialObjectProjectile>();

    // Use this for initialization
    void Start()
    {
        // Cambiar el color del spirte par ahacerlo transparente
        spriteRenderer.color = clearWhite;

        // Obtener componentes recurrentes
        rigidbody2D = GetComponent<Rigidbody2D>();
        collider2D = GetComponent<Collider2D>();

        // Iniciar destruccion de elemento
        coroutineToDestroyInstance = StartCoroutine(CoroutineToDestroy());
        startReady = true;

        specialObjectInstances.Add(this);
    }

    /// <summary>
    /// Indica cual sera el objetivo del lanzamiento
    /// </summary>
    /// <param name="targetSelected">Objetivo del lanzamiento</param>
    public void SetTarget(Transform targetSelected)
    {
        target = targetSelected;

        // Como medida preventiva, se cargara el collider2D aca para evitar nulos
        collider2D = GetComponent<Collider2D>();

        // Se deberia ignorar cualquier otro tipo de colision con personajes, ya que no son el objetivo
        CharacterInGameController[] charactersInGameController = FindObjectsOfType<CharacterInGameController>();
        foreach(CharacterInGameController character in charactersInGameController)
        {
            if(!character.CanRecieveHitsByObjects() || character.transform.GetInstanceID() != target.GetInstanceID())
            {
                if(collider2D && character.GetComponent<Collider2D>()) Physics2D.IgnoreCollision(collider2D, character.GetComponent<Collider2D>());
            }
        }

        StartCoroutine(ThrowIt());
    }

    /// <summary>
    /// Indica cual sera el objetivo del lanzamiento ademas de los otros parametros necesarios para lanzar
    /// </summary>
    /// <param name="targerSelected"></param>
    /// <param name="force"></param>
    /// <param name="torque"></param>
    /// <param name="yPosition"></param>
    public void SetTarget(Transform targerSelected, float force, float torque, float yPosition)
    {
        this.force = force;
        this.torque = torque;
        this.yPosition = yPosition;
        fixedParams = true;

        // Llamar al metodo base
        SetTarget(targerSelected);
    }

    /// <summary>
    /// Inicio del proceso de destruccion
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoroutineToDestroy()
    {
        yield return new WaitForSecondsRealtime(timeToDestroy);

        // Fade del elemento
        System.Action<ITween<Color>> updateColor = (t) =>
        {
            if (spriteRenderer) spriteRenderer.color = t.CurrentValue;
        };

        System.Action<ITween<Color>> updateColorComplete = (t) =>
        {
            if (gameObject) Destroy(gameObject);
        };

        colorTweenInstance = gameObject.Tween(string.Format("FadeOut{0}", gameObject.GetInstanceID()), Color.white, clearWhite, timeToFade, TweenScaleFunctions.Linear, updateColor, updateColorComplete);

    }

    /// <summary>
    /// Lanzamiento hacia el objetivo
    /// </summary>
    /// <returns></returns>
    private IEnumerator ThrowIt()
    {
        while (!startReady) yield return null;

        // Calcular la fuerza necesaria para ser enviado
        if (!fixedParams)
        {
            force = Random.Range(minXForce, maxXForce);
            torque = Random.Range(minTorque, maxTorque);
            yPosition = Random.Range(minYPosition, maxYPosition);
        }

        // Calculo del factor de posicion para el lanzamiento
        int positionFactor = target.localPosition.x < 0 ? -1 : 1;

        // Calculo y seteo de la posicion inicial del proyectil (Fuera de la pantalla)
        Vector3 positionOutsiCamera = new Vector3(positionFactor == -1 ? 1 : 0, yPosition, 1);
        Vector3 initialPosition = Camera.main.ViewportToWorldPoint(positionOutsiCamera);
        transform.localPosition = initialPosition;
        spriteRenderer.color = Color.white;

        rigidbody2D.AddForce(new Vector2(positionFactor * force, 0));
        rigidbody2D.AddTorque(torque);
        yield return null;
    }

    /// <summary>
    /// Detener corutinas, animaciones, etc
    /// </summary>
    private void OnDestroy()
    {
        specialObjectInstances.Remove(this);
        if (coroutineToDestroyInstance != null) StopCoroutine(coroutineToDestroyInstance);
        if (colorTweenInstance != null) colorTweenInstance.Stop(TweenStopBehavior.DoNotModify);
    }
}