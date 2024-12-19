using DigitalRuby.Tween;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenIllustration : MonoBehaviour{

    [SerializeField] string identifier;

    [Header("Animation")]
    [SerializeField] [Range(-100, 100)] float xMovement = 10;
    [SerializeField] [Range(-100, 100)] float yMovement = 10;
    [SerializeField] [Range(0, 2)] float timeToMove = 1f;
    [SerializeField] [Range(0, 2)] float timeToShow = .25f;

    Image illustration;
    CanvasGroup canvasGroup;

    // Materiales para ilustraciones y elementos
    private Material normalMaterial;
    private Material blackAndWhiteMaterial;

    // Animaciones
    Tween<float> fadeInTween;
    Tween<Vector2> moveTween;


    /// <summary>
    /// Copia del material asociado a la imagen
    /// </summary>
    private void Awake()
    {
        illustration = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        normalMaterial = illustration.material;
        blackAndWhiteMaterial = new Material(illustration.material);
        blackAndWhiteMaterial.SetFloat("_GrayscaleAmount", 1);
    }

    public void Start()
    {
        if (!GameController.GetFirstLoad()) StartCoroutine(CheckIllustration());
    }

    /// <summary>
    /// Una vez cargado el characterpool, revisar si pj esta desbloqueado
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckIllustration() {
        while (!CharacterPool.Instance) yield return null;
        GameObject character = CharacterPool.Instance.Get(identifier);
        if (character && !character.GetComponent<CharacterConfiguration>().IsUnlocked()) UpdateBlackAndWhite(true);
    }

    /// <summary>
    /// Indicar si imagen debe estar en blanco y negro
    /// </summary>
    public virtual void UpdateBlackAndWhite(bool blackAndWhite)
    {
        illustration.material = blackAndWhite ? blackAndWhiteMaterial : normalMaterial;
    }

    /// <summary>
    /// Realiza la animacion de aparicion la ilustracion
    /// </summary>
    /// <param name="noTransition"></param>
    public void Show(bool noTransition = false)
    {
        if (noTransition)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            // Calcular posiciones de inicio y fin
            Vector2 finalPosition = transform.localPosition;
            Vector2 initialPositon = new Vector2(finalPosition.x - xMovement, finalPosition.y - yMovement);

            // Mover el contenedor
            System.Action<ITween<Vector2>> updatePosition = (t) =>
            {
                if (transform) transform.localPosition = t.CurrentValue;
            };

            // Cmabiar el alpha
            System.Action<ITween<float>> updateAlpha = (t) =>
            {
                if (canvasGroup) canvasGroup.alpha = t.CurrentValue;
            };

            // Hacer fade in y mostrar elemento
            fadeInTween = gameObject.Tween(string.Format("FadeIn{0}", GetInstanceID()), 0, 1, timeToShow, TweenScaleFunctions.CubicEaseOut, updateAlpha);
            moveTween = gameObject.Tween(string.Format("Move{0}", GetInstanceID()), initialPositon, finalPosition, timeToMove, TweenScaleFunctions.CubicEaseOut, updatePosition);
        }
    }

    /// <summary>
    /// Al destruir, detener la animacion de la ilustracion
    /// </summary>
    private void OnDestroy()
    {
        if (fadeInTween != null) fadeInTween.Stop(TweenStopBehavior.Complete);
        if (moveTween != null) moveTween.Stop(TweenStopBehavior.Complete);
    }
}